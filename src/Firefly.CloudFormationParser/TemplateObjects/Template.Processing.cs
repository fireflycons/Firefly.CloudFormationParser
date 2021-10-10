namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Firefly.CloudFormationParser.GraphObjects;
    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization;
    using Firefly.CloudFormationParser.Serialization.Deserializers;
    using Firefly.CloudFormationParser.Serialization.Settings;

    using QuikGraph;
    using QuikGraph.Algorithms;
    using QuikGraph.Graphviz;
    using QuikGraph.Graphviz.Dot;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents an entire CloudFormation Template
    /// </summary>
    public partial class Template : ITemplate
    {
        /// <summary>
        /// Names of implicit SAM resources created by event declarations in Serverless::Function
        /// </summary>
        private static readonly List<string> ImplicitSAMResources = new List<string> { "ServerlessRestApi", "ServerlessHttpApi", "ServerlessDeploymentApplication" };

        /// <summary>
        /// Template graph vertices
        /// </summary>
        private readonly List<IVertex> vertices = new List<IVertex>();

        /// <summary>
        /// Template graph edges
        /// </summary>
        private readonly List<TaggedEdge<IVertex, EdgeDetail>> edges = new List<TaggedEdge<IVertex, EdgeDetail>>();

        /// <summary>
        /// The current context. When walking the template to fix up unresolved intrinsics,
        /// this helps determine whether the dictionary being examined should actually be replaced by a concrete intrinsic.
        /// </summary>
        private DeserializationContext currentContext = DeserializationContext.None;

        /// <inheritdoc />
        [YamlIgnore]
        public BidirectionalGraph<IVertex, TaggedEdge<IVertex, EdgeDetail>> DependencyGraph { get; private set; } = new BidirectionalGraph<IVertex, TaggedEdge<IVertex, EdgeDetail>>();

        /// <inheritdoc />
        [YamlIgnore]
        public Dictionary<string, bool> EvaluatedConditions { get; } = new Dictionary<string, bool>();

        /// <inheritdoc />
        [YamlIgnore]
        public bool IsSAMTemplate
        {
            get
            {
                const string Serverless = "AWS::Serverless-";

                switch (this.Transform)
                {
                    case null:

                        return false;

                    case string s when s.StartsWith(Serverless):
                    case List<object> lo when lo.Any(o => o.ToString().StartsWith(Serverless)):

                        return true;

                    default:

                        return false;
                }
            }
        }

        /// <summary>
        /// Gets vertices which are valid as the source of a relationship between two vertices
        /// </summary>
        [YamlIgnore]
        private IEnumerable<IVertex> SourceVertices => this.vertices.Where(v => v.GetType() != typeof(OutputVertex));

        /// <summary>
        /// Gets output vertices
        /// </summary>
        [YamlIgnore]
        private IEnumerable<IVertex> OutputVertices => this.vertices.Where(v => v.GetType() == typeof(OutputVertex));

        /// <summary>
        /// Gets resource vertices
        /// </summary>
        [YamlIgnore]
        private IEnumerable<IVertex> ResourceVertices =>
            this.vertices.Where(v => v.GetType() == typeof(ResourceVertex));

        /// <summary>
        /// Gets the pseudo parameters.
        /// </summary>
        /// <value>
        /// The pseudo parameters.
        /// </value>
        [YamlIgnore]
        internal List<PseudoParameter> PseudoParameters { get; } = new List<PseudoParameter>();

        /// <summary>
        /// Deserializes a YAML or JSON template.
        /// </summary>
        /// <param name="settings">A <see cref="IDeserializerSettings"/> implementation defining what to deserialize and how.</param>
        /// <returns>Deserialized template.</returns>
        /// <exception cref="System.ArgumentException">Content property cannot be null - settings</exception>
        public static async Task<ITemplate> Deserialize(IDeserializerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentException("Argument cannot be null", nameof(settings));
            }

            return (await TemplateSerializer.Deserialize(settings)).PostProcess(
                settings.ExcludeConditionalResources,
                settings.ParameterValues);
        }

        /// <summary>
        /// Serializes a template to YAML.
        /// </summary>
        /// <param name="template">The template to serialize.</param>
        /// <returns>YAML string.</returns>
        public static string Serialize(ITemplate template)
        {
            return TemplateSerializer.Serialize(template);
        }

        /// <summary>
        /// Adds a pseudo parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        internal void AddPseudoParameter(PseudoParameter param)
        {
            if (this.PseudoParameters.All(p => p.Name != param.Name))
            {
                this.PseudoParameters.Add(param);
                this.vertices.Add(new PseudoParameterVertex(param));
            }
        }

        /// <summary>
        /// <para>
        /// Post process the template following deserialization.
        /// </para>
        /// <para>
        /// <list type="number">
        /// <item>
        /// <description>Associate names with template objects by copying them in from the dictionary key that contains the object as its value.</description>
        /// </item>
        /// <item>
        /// <description>Create list of graph vertices for parameters, resources and outputs.</description>
        /// </item>
        /// <item>
        /// <description>Walk resources and outputs converting long form intrinsics (dictionaries) to <see cref="IIntrinsic"/> implementations.</description>
        /// </item>
        /// <item>
        /// <description>Construct bidirectional graph of object relationships.</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="excludeConditionalResources">If <c>true</c>, omit resources excluded by evaluated conditions.</param>
        /// <param name="parameterValues">Optional dictionary of values to assign to parameters</param>
        /// <returns>Reference to the template, for method chaining.</returns>
        internal Template PostProcess(bool excludeConditionalResources, IDictionary<string, object>? parameterValues)
        {
            FixNames(this.ParsedParameters);
            FixNames(this.ParsedResources);
            FixNames(this.ParsedOutputs);

            this.ProcessConditions();

            if (excludeConditionalResources)
            {
                foreach (var objectToRemove in this.Resources.Cast<IConditionalTemplateObject>()
                    .Concat(this.Outputs.Cast<Output>())
                    .Where(conditionalTemplateObject => !string.IsNullOrEmpty(conditionalTemplateObject.Condition)
#pragma warning disable 8604 // string.IsNullOrEmpty checks this
                                                        && !this.EvaluatedConditions[conditionalTemplateObject.Condition]))
#pragma warning restore 8604
                {
                    switch (objectToRemove)
                    {
                        case Resource _:

                            this.ParsedResources!.Remove(objectToRemove.Name);
                            break;

                        case Output _:

                            this.ParsedOutputs!.Remove(objectToRemove.Name);
                            break;
                    }
                }
            }

            this.vertices.AddRange(
                this.Parameters.Select(p => (IVertex)new ParameterVertex(p))
                    .Concat(this.Resources.Select(r => new ResourceVertex(r)))
                    .Concat(this.Outputs.Select(o => new OutputVertex(o))));

            // Resources and parameters cannot share identifiers
            var sb = new StringBuilder();

            foreach (var g in this.vertices.Where(v => v.GetType() != typeof(OutputVertex)).GroupBy(v => v.Name)
                .Where(ng => ng.Count() > 1))
            {
                var vertexList = g.ToList();
                var templateSections = string.Join(
                    ", ",
                    vertexList.Select(v => v.TemplateObject.GetType().Name + "s").Distinct());

                sb.AppendLine($"Ambiguous identifier '{g.Key}' in sections {templateSections}.");
            }

            if (sb.Length > 0)
            {
                throw new FormatException(sb.ToString());
            }

            if (parameterValues != null)
            {
                // Assign parameters
                foreach (var param in this.Parameters)
                {
                    param.SetCurrentValue(parameterValues);
                }
            }

            this.ProcessResources();
            this.ProcessOutputs();

            this.DependencyGraph = new BidirectionalGraph<IVertex, TaggedEdge<IVertex, EdgeDetail>>();
            this.DependencyGraph.AddVertexRange(this.vertices);
            this.DependencyGraph.AddEdgeRange(this.edges);

#if DEBUG
            // Generate a DOT graph of resources.
            // View by pasting the string content of dotGraph variable into https://dreampuf.github.io/GraphvizOnline
            var dotGraph = this.DependencyGraph.ToGraphviz(
                algorithm =>
                    {
                        var font = new GraphvizFont("Arial", 9);

                        algorithm.CommonVertexFormat.Font = font;
                        algorithm.CommonEdgeFormat.Font = font;
                        algorithm.GraphFormat.RankDirection = GraphvizRankDirection.LR;
                        algorithm.FormatVertex += (sender, args) =>
                            {
                                args.VertexFormat.Shape = args.Vertex.Shape;
                                args.VertexFormat.Label = args.Vertex.Name;
                            };

                        algorithm.FormatEdge += (sender, args) =>
                            {
                                args.EdgeFormat.Label.Value = args.Edge.Tag?.AttributeName;
                            };
                    });
#endif

            return this;
        }

        /// <summary>
        /// Fix up names of template objects by copying them in from the dictionary key that contains the object as its value. 
        /// </summary>
        /// <typeparam name="T">Type of template section being processed</typeparam>
        /// <param name="section">The section.</param>
        private static void FixNames<T>(IDictionary<string, T>? section)
            where T : ITemplateObject
        {
            if (section == null)
            {
                return;
            }

            foreach (var kv in section)
            {
                // ReSharper disable once PossibleStructMemberModificationOfNonVariableStruct - These are ITemplateObject which are references, not values.
                kv.Value.Name = kv.Key;
            }
        }

        /// <summary>
        /// Process any template conditions, evaluating all to get a true/false result for each.
        /// </summary>
        private void ProcessConditions()
        {
            if (this.Conditions == null || !this.Conditions.Any())
            {
                return;
            }

            this.currentContext = DeserializationContext.None;

            var changes = new List<IntrinsicConvert>();

            this.WalkDict(this.Conditions, null, null, changes);

            // Resolve all intrinsics within each condition tree.
            foreach (var change in changes)
            {
                if (change.Reference is UnresolvedTagProperty utp)
                {
                    utp.SetValue(change.Intrinsic);
                }
                else if (change.Key is string key)
                {
                    this.Conditions[key] = change.Intrinsic ?? throw new InvalidOperationException("Missing required intrinsic function.");
                }
            }

            // Create a graph of dependencies between conditions
            var graph = new BidirectionalGraph<ConditionVertex, Edge<ConditionVertex>>();
            var conditionVertices = this.Conditions.Select(kv => new ConditionVertex(kv.Key)).ToList();
            graph.AddVertexRange(conditionVertices);

            foreach (var condition in this.Conditions.Select(
                cond => new KeyValuePair<string, AbstractLogicalIntrinsic>(cond.Key, (AbstractLogicalIntrinsic)cond.Value)))
            {
                var target = conditionVertices.First(v => v.Name == condition.Key);

                foreach (ConditionIntrinsic referencedCondition in condition.Value.GetConditionIntrinsics())
                {
                    var source = conditionVertices.First(v => v.Name == referencedCondition.Value.ToString());
                    graph.AddEdge(new Edge<ConditionVertex>(source, target));
                }
            }

            // Sort conditions accounting for dependencies and evaluate
            foreach (var vertex in graph.TopologicalSort())
            {
                var condition = (AbstractLogicalIntrinsic)this.Conditions.First(kv => kv.Key == vertex.Name).Value;
                this.EvaluatedConditions.Add(vertex.Name, (bool)condition.Evaluate(this));
            }
        }

        /// <summary>
        /// Process template resources, building the dependency graph.
        /// </summary>
        /// <exception cref="System.FormatException">Resource '{dependency}' in DependsOn for resource '{resource.Name}' does not exist.</exception>
        private void ProcessResources()
        {
            this.currentContext = DeserializationContext.Resources;

            // TODO: Strike out resources where any Condition is false
            foreach (var resource in this.ParsedResources.Select(kv => kv.Value))
            {
                var vertex = this.vertices.First(v => v.Name == resource.Name);

                if (resource.Properties != null)
                {
                    // Some resources don't require properties, like AWS::CloudFormation::WaitConditionHandle
                    var changes = new List<IntrinsicConvert>();
                    this.WalkDict(resource.Properties, null, null, changes);

                    foreach (var change in changes)
                    {
                        switch (change.Reference)
                        {
                            case IDictionary dict when change.Key != null:

                                dict[change.Key] = change.Intrinsic;
                                break;

                            case IList list when change.Key != null:

                                list[(int)change.Key] = change.Intrinsic;
                                break;

                            case UnresolvedTagProperty utp:

                                utp.SetValue(change.Intrinsic);
                                break;
                        }
                    }

                    this.GenerateGraphEdges(vertex, resource.Properties);
                }

                if (resource.Metadata != null)
                {
                    // Some resources don't require properties, like AWS::CloudFormation::WaitConditionHandle
                    this.GenerateGraphEdges(vertex, resource.Metadata);
                }

                // Explicit dependencies
                foreach (var dependency in resource.ExplicitDependencies)
                {
                    var source = this.ResourceVertices.FirstOrDefault(v => v.Name == dependency);
                    var samLinked = this.SourceVertices.FirstOrDefault(v => dependency.StartsWith(v.Name)) != null;

                    if (source == null)
                    {
                        if (!this.IsSAMReference(this.SourceVertices, dependency))
                        {
                            throw new FormatException(
                                $"Resource '{dependency}' in DependsOn for resource '{resource.Name}' does not exist.");
                        }

                        // SAM template, but can't create reference now
                        return;
                    }

                    this.edges.Add(
                        new TaggedEdge<IVertex, EdgeDetail>(
                            source,
                            vertex,
                            new EdgeDetail(ReferenceType.DependsOn)));
                }
            }
        }

        /// <summary>
        /// Processes the outputs, adding to the dependency graph.
        /// </summary>
        private void ProcessOutputs()
        {
            this.currentContext = DeserializationContext.Outputs;

            // TODO: Strike out outputs where any Condition is false
            foreach (var output in this.Outputs)
            {
                var vertex = this.OutputVertices.First(v => v.Name == output.Name);

                switch (output.Value)
                {
                    case IDictionary dict:

                        var changes = new List<IntrinsicConvert>();
                        this.WalkDict(dict, output, "Value", changes);

                        foreach (var change in changes)
                        {
                            switch (change.Reference)
                            {
                                case IDictionary dict1:

                                    if (change.Key == null)
                                    {
                                        change.Reference = change.Intrinsic;
                                    }
                                    else
                                    {
                                        dict1[change.Key] = change.Intrinsic;
                                    }

                                    break;

                                case IList list when change.Key != null:

                                    list[(int)change.Key] = change.Intrinsic;
                                    break;

                                case Output output1 when change.Key != null:

                                    typeof(Output).GetProperties().FirstOrDefault(p => p.Name == (string)change.Key)
                                        ?.SetValue(output1, change.Intrinsic);
                                    break;

                                case UnresolvedTagProperty utp:

                                    utp.SetValue(change.Intrinsic);
                                    break;
                            }
                        }

                        if (output.Value is AbstractIntrinsic tag)
                        {
                            foreach (var @ref in tag.GetReferencedObjects(this))
                            {
                                this.AddEdge(vertex, @ref);
                            }
                        }
                        else
                        {
                            this.GenerateGraphEdges(vertex, dict);
                        }

                        break;

                    case AbstractIntrinsic tag1:

                        foreach (var @ref in tag1.GetReferencedObjects(this))
                        {
                            if (@ref.Contains("."))
                            {
                                this.AddGetAttEdge(vertex, @ref);
                            }
                            else
                            {
                                this.AddReferenceEdge(vertex, @ref);
                            }
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// For the given target vertex, walk a dictionary in the template model finding objects
        /// that reference the target vertex and create edges from each object to the target.
        /// Recursively examine any dictionary or list encountered.
        /// </summary>
        /// <param name="targetVertex">The target vertex for any edge.</param>
        /// <param name="dict">The dictionary being examined.</param>
        private void GenerateGraphEdges(IVertex targetVertex, IDictionary dict)
        {
            foreach (DictionaryEntry kv in dict)
            {
#if DEBUG
                var key = (string)kv.Key;
#endif
                switch (kv.Value)
                {
                    case IDictionary dict1:
                        this.GenerateGraphEdges(targetVertex, dict1);
                        break;

                    case IList list:
                        this.GenerateGraphEdges(targetVertex, list);
                        break;

                    case IfIntrinsic iftag:

                        foreach (var @ref in iftag.GetReferencedObjects(this))
                        {
                            this.AddEdge(targetVertex, @ref);
                        }

                        break;
                        
                    case AbstractIntrinsic tag1:

                        this.AddEdgesFromIntrinsic(targetVertex, tag1);

                        break;
                }
            }
        }

        /// <summary>
        /// Adds graph edges for what is referenced by an intrinsic.
        /// </summary>
        /// <param name="targetVertex">The target vertex.</param>
        /// <param name="intrinsic">The intrinsic.</param>
        private void AddEdgesFromIntrinsic(IVertex targetVertex, AbstractIntrinsic intrinsic)
        {
            foreach (var @ref in intrinsic.GetReferencedObjects(this))
            {
                if (@ref.Contains("."))
                {
                    this.AddGetAttEdge(targetVertex, @ref);
                }
                else
                {
                    this.AddReferenceEdge(targetVertex, @ref);
                }
            }
        }

        /// <summary>
        /// For the given target vertex, walk a list in the template model finding objects
        /// that reference the target vertex and create edges from each object to the target.
        /// Recursively examine any dictionary or list encountered.
        /// </summary>
        /// <param name="targetVertex">The target vertex for any edge.</param>
        /// <param name="list">The list being examined.</param>
        private void GenerateGraphEdges(IVertex targetVertex, IList list)
        {
            foreach (var obj in list)
            {
                switch (obj)
                {
                    case IDictionary dict:

                        this.GenerateGraphEdges(targetVertex, dict);
                        break;

                    case IList list1:
                        this.GenerateGraphEdges(targetVertex, list1);
                        break;

                    case AbstractIntrinsic tag1:

                        switch (tag1)
                        {
                            case RefIntrinsic @ref:

                                var sourceObject = @ref.GetReferencedObjects(this).First();
                                this.AddReferenceEdge(targetVertex, sourceObject);
                                break;

                            case GetAttIntrinsic getAtt:

                                var attributeReference = (Tuple<string, string>)getAtt.Evaluate(this);
                                this.AddGetAttEdge(targetVertex, attributeReference);
                                break;
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Adds an edge, which amy be a direct reference or an attribute reference in dotted form.
        /// </summary>
        /// <param name="targetVertex">The target vertex.</param>
        /// <param name="reference">The reference.</param>
        private void AddEdge(IVertex targetVertex, string reference)
        {
            switch (reference)
            {
                case string s1 when s1.Contains("."):

                    this.AddGetAttEdge(targetVertex, s1);
                    break;

                case string s2:

                    this.AddReferenceEdge(targetVertex, s2);
                    break;
            }
        }

        /// <summary>
        /// Adds a graph edge for a direct reference, either via !Ref or explicit DependsOn.
        /// </summary>
        /// <param name="targetVertex">The target vertex for the edge.</param>
        /// <param name="sourceObject">Object whose vertex will be the source of the edge relationship.</param>
        private void AddReferenceEdge(IVertex targetVertex, string sourceObject)
        {
            var sourceVertex = this.SourceVertices.FirstOrDefault(v => v.Name == sourceObject);
            var samLinked = this.SourceVertices.FirstOrDefault(v => sourceObject.StartsWith(v.Name)) != null;

            if (sourceVertex == null)
            {
                if (!this.IsSAMReference(this.SourceVertices, sourceObject))
                {
                    throw new FormatException(
                        $"Reference '{sourceObject}' not found for {targetVertex.TemplateObject.GetType().Name} '{targetVertex.Name}'");
                }

                // SAM template, but can't create reference now
                return;
            }

            var edge = new EdgeDetail(
                sourceVertex is IParameterVertex ? ReferenceType.ParameterReference : ReferenceType.DirectReference);

            var taggedEdge = new TaggedEdge<IVertex, EdgeDetail>(sourceVertex, targetVertex, edge);

            if (!this.edges.Contains(taggedEdge, new EdgeEqualityComparer()))
            {
                this.edges.Add(taggedEdge);
            }
        }

        /// <summary>
        /// Determines whether the given reference is an implicit SAM reference.
        /// </summary>
        /// <param name="sourceVertices">The source vertices.</param>
        /// <param name="reference">The reference.</param>
        /// <returns>
        ///   <c>true</c> if [is sam reference] [the specified source vertices]; otherwise, <c>false</c>.
        /// </returns>
        // ReSharper disable once InconsistentNaming
        private bool IsSAMReference(IEnumerable<IVertex> sourceVertices, string reference)
        {
            return this.IsSAMTemplate && (ImplicitSAMResources.Contains(reference)
                   || sourceVertices.FirstOrDefault(v => reference.StartsWith(v.Name)) != null);
        }

        /// <summary>
        /// Adds a graph edge for an attribute reference.
        /// </summary>
        /// <param name="targetVertex">The target vertex for the edge.</param>
        /// <param name="attributeReference">Attribute reference in <c>LogicalId.Attribute</c> form.</param>
        private void AddGetAttEdge(IVertex targetVertex, string attributeReference)
        {
            var parts = attributeReference.Split('.');

            this.AddGetAttEdge(targetVertex, new Tuple<string, string>(parts[0], parts[1]));
        }

        /// <summary>
        /// Adds a graph edge for an attribute reference.
        /// </summary>
        /// <param name="targetVertex">The target vertex for the edge.</param>
        /// <param name="attributeReference">Attribute reference as a tuple of LogicalId and Attribute.</param>
        private void AddGetAttEdge(IVertex targetVertex, Tuple<string, string> attributeReference)
        {
            var (sourceObject, item) = attributeReference;
            var edge = new EdgeDetail(item);

            var sourceVertex = this.SourceVertices.FirstOrDefault(v => v.Name == sourceObject);

            if (sourceVertex == null)
            {
                if (!this.IsSAMReference(this.SourceVertices, sourceObject))
                {
                    throw new FormatException(
                    $"GetAtt '{sourceObject}' not found for {targetVertex.TemplateObject.GetType().Name} '{targetVertex.Name}'");
                }

                // SAM template, but can't create reference now
                return;
            }

            var taggedEdge = new TaggedEdge<IVertex, EdgeDetail>(sourceVertex, targetVertex, edge);

            if (!this.edges.Contains(taggedEdge, new EdgeEqualityComparer()))
            {
                this.edges.Add(taggedEdge);
            }
        }

        /// <summary>
        /// Walks a dictionary in the deserialized template hierarchy looking for objects that should be replaced with their intrinsic function representations.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="changes">The changes to apply to the model.</param>
        private void WalkDict(IDictionary dict, object? parent, object? parentKey, List<IntrinsicConvert> changes)
        {
            foreach (DictionaryEntry kv in dict)
            {
                var key = (string)kv.Key;

                if (TagRepository.AllTags.FirstOrDefault(t => t.LongName == key) is AbstractIntrinsic tag && tag.ShouldDeserialize(this.currentContext))
                {
                    changes.Add(
                        new IntrinsicConvert((IIntrinsic)kv.Value, parent, parentKey));
                }

                switch (kv.Value)
                {
                    case IDictionary dict1:

                        this.WalkDict(dict1, dict, key, changes);
                        break;

                    case IList list:
                        this.WalkList(list, changes);
                        break;

                    case Resource r when r.Properties != null:
                        this.WalkDict(r.Properties, r, parentKey, changes);
                        break;

                    case AbstractIntrinsic tag1:

                        foreach (var unresolved in tag1.GetUnresolvedDictionaryProperties())
                        {
                            this.WalkDict(unresolved.GetDictionaryValue(), unresolved, null, changes);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Walks a list in the deserialized template hierarchy looking for other collections that should be walked.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="changes">The changes to apply to the model.</param>
        private void WalkList(IList list, List<IntrinsicConvert> changes)
        {
            var index = -1;

            foreach (var obj in list)
            {
                ++index;

                switch (obj)
                {
                    case IDictionary dict:

                        this.WalkDict(dict, list, index, changes);
                        break;

                    case IList list1:

                        this.WalkList(list1, changes);
                        break;
                }
            }
        }

        /// <summary>
        /// Represents a change to be made to the model. 
        /// </summary>
        private class IntrinsicConvert
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IntrinsicConvert"/> class.
            /// </summary>
            /// <param name="intrinsic">The intrinsic.</param>
            /// <param name="reference">The reference.</param>
            /// <param name="key">The dictionary key, when the reference is an <see cref="IDictionary"/>, or an integer index when the reference is an <see cref="IList"/>.</param>
            public IntrinsicConvert(IIntrinsic intrinsic, object? reference, object? key)
            {
                this.Reference = reference;
                this.Intrinsic = intrinsic;
                this.Key = key;
            }

            /// <summary>
            /// Gets or sets the reference.
            /// </summary>
            /// <value>
            /// The reference.
            /// </value>
            public object? Reference { get; set; }

            /// <summary>
            /// Gets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public object? Key { get; }

            /// <summary>
            /// Gets the intrinsic.
            /// </summary>
            /// <value>
            /// The intrinsic.
            /// </value>
            public IIntrinsic Intrinsic { get; }
        }

        /// <summary>
        /// Equality comparer for graph edges. In the parse, the same relationship may be found more than once
        /// </summary>
        private class EdgeEqualityComparer : IEqualityComparer<TaggedEdge<IVertex, EdgeDetail>>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type T to compare.</param>
            /// <param name="y">The second object of type T to compare.</param>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            public bool Equals(TaggedEdge<IVertex, EdgeDetail>? x, TaggedEdge<IVertex, EdgeDetail>? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Source.Equals(y.Source) && x.Tag.Equals(y.Tag) && x.Target.Equals(y.Target);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public int GetHashCode(TaggedEdge<IVertex, EdgeDetail> obj)
            {
                return obj.Tag?.GetHashCode() ?? 0 ^ obj.Source.GetHashCode() ^ obj.Target.GetHashCode();
            }
        }
    }
}