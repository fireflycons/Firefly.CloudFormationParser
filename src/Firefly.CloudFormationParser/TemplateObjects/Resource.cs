namespace Firefly.CloudFormationParser.TemplateObjects
{
#pragma warning disable 1574
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents a template resource
    /// </summary>
    public class Resource : IConditionalTemplateObject, IResource
    {
        /// <summary>
        /// For SAM resources, list of types that can have properties in global section.
        /// </summary>
        private static readonly List<string> ValidGlobalSectionResources =
            new List<string> { "Function", "Api", "HttpApi", "SimpleTable" };

        /// <inheritdoc cref="IResource.Condition"/>
        [YamlMember(Order = 0)]
        public string? Condition { get; set; }

        /// <inheritdoc cref="IResource.CreationPolicy"/>
        [YamlMember(Order = 4)]
        public Dictionary<string, object>? CreationPolicy { get; set; }

        /// <inheritdoc cref="IResource.DeletionPolicy"/>
        [YamlMember(Order = 5)]
        public string? DeletionPolicy { get; set; }

        /// <inheritdoc cref="IResource.DependsOn"/>
        [YamlMember(Order = 1)]
        public object? DependsOn { get; set; }

        /// <inheritdoc cref="IResource.Description"/>
        [YamlMember(Order = 3)]
        public object? Description { get; set; }

        /// <inheritdoc cref="IResource.ExplicitDependencies"/>
        [YamlIgnore]
        public IEnumerable<string> ExplicitDependencies =>
            this.DependsOn switch
                {
                    null => new List<string>(),
                    string s => new List<string> { s },
                    List<object> l => l.Select(d => (string)d),
                    _ => throw new InvalidCastException(
                             $"Unexpected type {this.DependsOn.GetType().Name} for DependsOn in resource {this.Name}")
                };

        /// <inheritdoc cref="IResource.IsSAMResource"/>
        [YamlIgnore]
        public bool IsSAMResource => this.Type != null && this.Type.StartsWith("AWS::Serverless");

        /// <summary>
        /// Gets or sets the function transform.
        /// </summary>
        /// <value>
        /// The function transform.
        /// </value>
        [YamlMember(Order = 11, Alias = "Fn::Transform", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public object? FnTransform { get; set; }

        /// <inheritdoc cref="IResource.Metadata"/>
        [YamlMember(Order = 9)]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <inheritdoc cref="IResource.Name"/>
        [YamlIgnore]
        public string Name { get; set; } = "Unresolved";

        /// <inheritdoc cref="IResource.Properties"/>
        [YamlMember(Order = 10)]
        public Dictionary<string, object>? Properties { get; set; }

        /// <summary>
        /// Gets or sets the transform.
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        [YamlMember(Order = 12)]
        public object? Transform
        {
            get => null;

            set => this.FnTransform = value;
        }

        /// <inheritdoc cref="IResource.Type"/>
        [YamlMember(Order = 2)]
        public string? Type { get; set; }

        /// <inheritdoc cref="IResource.UpdatePolicy"/>
        [YamlMember(Order = 6)]
        public Dictionary<string, object>? UpdatePolicy { get; set; }

        /// <inheritdoc cref="IResource.UpdateReplacePolicy"/>
        [YamlMember(Order = 7)]
        public string? UpdateReplacePolicy { get; set; }

        /// <inheritdoc cref="IResource.Version"/>
        [YamlMember(Order = 8)]
        public string? Version { get; set; }

        /// <inheritdoc cref="IResource.Template"/>
        [YamlIgnore]
        public ITemplate? Template { get; set; }

        /// <inheritdoc cref="IResource.GetResourcePropertyValue"/>
        public object? GetResourcePropertyValue(string propertyPath)
        {
            var prop = this.GetPropertyAtPath(this.Properties, propertyPath, false);
            var propValue = prop?.GetValue();

            if (propValue is IIntrinsic intrinsic)
            {
                if (this.Template == null)
                {
                    // Should only get this for incorrectly set up tests.
                    throw new InvalidOperationException(
                        $"Resource {this.Name}: Cannot evaluate intrinsic when template property is null");
                }

                // Evaluate the intrinsic and return its result
                return intrinsic.Evaluate(this.Template);
            }

            // Value is null, scalar, list or dictionary
            if (this.IsSAMResource)
            {
                if (this.Template?.Globals == null)
                {
                    return propValue;
                }

                // Maybe property is in globals section
                var globalType = this.Type!.Split(new[] { "::" }, StringSplitOptions.None).Last();

                if (!ValidGlobalSectionResources.Contains(globalType))
                {
                    return propValue;
                }

                if (this.Template == null)
                {
                    // Should only get this for incorrectly set up tests.
                    throw new InvalidOperationException(
                        $"Resource {this.Name}: Cannot evaluate globals when template property is null");
                }

                var section = this.Template.Globals.GetSection(globalType);

                if (section == null)
                {
                    return propValue;
                }

                var globalProp = this.GetPropertyAtPath(section, propertyPath, false);

                if (globalProp == null)
                {
                    return propValue;
                }

                var globalValue = globalProp.GetValue();

                switch (propValue)
                {
                    case null:
                        return globalValue;

                    case IDictionary<object, object> dict
                        when globalValue is IDictionary<object, object> globalValueDict:

                        foreach (var kv in dict)
                        {
                            globalValueDict[kv.Key] = dict[kv.Key];
                        }

                        return globalValueDict;

                    case IList<object> list when globalValue is IList<object> globalList:

                        return list.Union(globalList).ToList();
                }
            }

            return propValue;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Resource {this.Name}";
        }

        /// <inheritdoc cref="IResource.UpdateResourceProperty"/>
        public void UpdateResourceProperty(string propertyPath, object newValue)
        {
            var prop = this.GetPropertyAtPath(this.Properties, propertyPath, true)!;

            if (!prop.IsIncompleteMatch)
            {
                prop.SetValue(newValue);
                return;
            }

            var current = prop.Container;
            var leafPropName = prop.UnresolvedProperties.Last();

            while (prop.UnresolvedProperties.Count > 1)
            {
                var propName = prop.UnresolvedProperties.Dequeue();

                switch (current)
                {
                    case IDictionary<string, object> dict:

                        current = new Dictionary<object, object>();
                        dict[propName] = current;
                        break;

                    case IDictionary<object, object> dict:

                        current = new Dictionary<object, object>();
                        dict[propName] = current;
                        break;
                }
            }

            // Last one gets the value
            switch (current)
            {
                case IDictionary<string, object> dict:

                    dict[leafPropName] = newValue;
                    break;

                case IDictionary<object, object> dict:

                    dict[leafPropName] = newValue;
                    break;
            }
        }

        /// <summary>
        /// Gets an object that references a property value within the properties hierarchy.
        /// </summary>
        /// <param name="propertiesToSearch">Object graph to search. Can be resource or global properties.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="returnClosestNode">If <c>true</c> return the closest node to the requested property.</param>
        /// <returns>Object that can be used to get or set the value of the property within the resource.</returns>
        private ResourceProperty? GetPropertyAtPath(IDictionary<string, object>?  propertiesToSearch, string propertyPath, bool returnClosestNode)
        {
            if (propertyPath == null)
            {
                throw new ArgumentNullException(nameof(propertyPath), "Argument cannot be null");
            }

            var pathSegments = new System.Collections.Generic.Queue<string>(propertyPath.Split('.'));
            var lastSegment = pathSegments.Last();

            if (propertiesToSearch == null)
            {
                throw new FormatException($"Cannot find resource property {propertyPath} in resource {this.Name}");
            }

            object? current = propertiesToSearch;
            object? parent = propertiesToSearch;
            
            string finalSegment = string.Empty;
            string key = string.Empty;
            var finished = false;

            while (pathSegments.Any())
            {
                var segment = pathSegments.Peek();
                finalSegment = segment;
                parent = current;
                key = segment;

                switch (current)
                {
                    case IDictionary dict:

                        if (dict.Contains(segment))
                        {
                            current = dict[segment];
                            break;
                        }

                        return returnClosestNode ? new ResourceProperty(current, pathSegments) : null;

                    case IList<object> list:

                        if (int.TryParse(segment, out var index))
                        {
                            current = list[index];
                            break;
                        }

                        return returnClosestNode ? new ResourceProperty(current, pathSegments) : null;

                    default:

                        finished = true;
                        break;
                }

                if (finished)
                {
                    break;
                }

                pathSegments.Dequeue();
            }

            return finalSegment == lastSegment ? new ResourceProperty(parent!, key) : null;
        }

        /// <summary>
        /// Object to manipulate the value at given location within resource properties
        /// </summary>
        private class ResourceProperty
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ResourceProperty"/> class.
            /// </summary>
            /// <param name="container">The container.</param>
            /// <param name="unresolvedProperties">The unresolved properties.</param>
            public ResourceProperty(
                object container,
                System.Collections.Generic.Queue<string> unresolvedProperties)
                : this(container, (string?)null)
            {
                this.UnresolvedProperties = unresolvedProperties;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ResourceProperty"/> class.
            /// </summary>
            /// <param name="container">The container.</param>
            /// <param name="key">The key.</param>
            public ResourceProperty(object container, string? key)
            {
                this.Container = container;
                this.Key = key;
            }

            /// <summary>
            /// Gets the container within the property tree. Either a list or a dictionary
            /// </summary>
            /// <value>
            /// The container.
            /// </value>
            public object Container { get; }

            /// <summary>
            /// Gets a value indicating whether this represents an incomplete match which it will if there are ant unresolved properties. 
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is incomplete match; otherwise, <c>false</c>.
            /// </value>
            public bool IsIncompleteMatch => this.UnresolvedProperties.Any();
            
            /// <summary>
            /// Gets the key to the container - key name for dict, index for list.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            private string? Key { get; }

            public System.Collections.Generic.Queue<string> UnresolvedProperties { get; } = new System.Collections.Generic.Queue<string>();

            /// <summary>
            /// Gets the value at the selected container and key.
            /// </summary>
            /// <returns>The value</returns>
            public object GetValue()
            {
                if (this.Key == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot get value on partially matched property.");
                }

                return this.Container switch
                    {
                        IDictionary dict => dict[this.Key],
                        IList list => list[int.Parse(this.Key)],
                        _ => throw new InvalidOperationException(
                                 $"Unexpected type {this.Container.GetType().Name} in resource properties.")
                    };
            }

            /// <summary>
            /// Sets the value at the selected container and key.
            /// </summary>
            /// <param name="value">The value.</param>
            public void SetValue(object? value)
            {
                if (this.Key == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot set value on partially matched property.");
                }

                switch (this.Container)
                {
                    case IDictionary dict:

                        dict[this.Key] = value.ToResourceSchema();
                        break;

                    case IList list:

                        list[int.Parse(this.Key)] = value.ToResourceSchema();
                        break;

                    default:

                        throw new InvalidOperationException(
                            $"Unexpected type {this.Container.GetType().Name} in resource properties.");
                }
            }
        }
    }
}