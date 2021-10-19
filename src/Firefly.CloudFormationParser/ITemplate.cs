namespace Firefly.CloudFormationParser
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.GraphObjects;
    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;

    using QuikGraph;

    /// <summary>
    /// Interface describing a CloudFormation Template.
    /// </summary>
    public interface ITemplate
    {
        /// <summary>
        /// <para>
        /// Gets or sets the AWS template format version.
        /// </para>
        /// <para>
        /// The AWS CloudFormation template version that the template conforms to.
        /// The template format version isn't the same as the API or WSDL version.
        /// The template format version can change independently of the API and WSDL versions.
        /// </para>
        /// </summary>
        /// <value>
        /// The AWS template format version.
        /// </value>
        // ReSharper disable once InconsistentNaming
        string? AWSTemplateFormatVersion { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the globals.
        /// </para>
        /// <para>
        /// Globals are unique to AWS SAM
        /// </para>
        /// </summary>
        /// <value>
        /// The conditions.
        /// </value>
        GlobalSection? Globals { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the conditions.
        /// </para>
        /// <para>
        /// Conditions that control whether certain resources are created or whether certain resource properties are assigned a value during stack creation or update.
        /// For example, you could conditionally create a resource that depends on whether the stack is for a production or test environment.
        /// </para>
        /// </summary>
        /// <value>
        /// The conditions.
        /// </value>
        ConditionSection? Conditions { get; set; }

        /// <summary>
        /// Gets a directed edge graph of dependencies between inputs, resources and outputs.
        /// </summary>
        /// <value>
        /// The dependency graph.
        /// </value>
        BidirectionalGraph<IVertex, TaggedEdge<IVertex, EdgeDetail>> DependencyGraph { get; }

        /// <summary>
        /// <para>
        /// Gets or sets the template's description.
        /// </para>
        /// <para>
        /// A text string that describes the template. This section must always follow the template format version section.
        /// </para> 
        /// </summary>
        /// <value>
        /// The template's description which will be <c>null</c> if the template did not provide this property.
        /// </value>
        string? Description { get; set; }

        /// <summary>
        /// Gets the value of each condition where the dictionary key is the condition name, post evaluation.
        /// </summary>
        /// <value>
        /// The evaluated conditions.
        /// </value>
        Dictionary<string, bool> EvaluatedConditions { get; }

        /// <summary>
        /// <para>
        /// Gets or sets the mappings.
        /// </para>
        /// <para>
        /// A mapping of keys and associated values that you can use to specify conditional parameter values, similar to a lookup table.
        /// You can match a key to a corresponding value by using the <c>Fn::FindInMap</c> intrinsic function in the Resources and Outputs sections.
        /// </para>
        /// </summary>
        /// <value>
        /// The mappings which will be <c>null</c> if the template did not provide this property.
        /// </value>
        MappingSection? Mappings { get; set; }

        /// <summary>
        /// Gets or sets the metadata - objects that provide additional information about the template.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        MetadataSection? Metadata { get; set; }

        /// <summary>
        /// <para>
        /// Gets the outputs.
        /// </para>
        /// <para>
        /// Describes the values that are returned whenever you view your stack's properties.
        /// For example, you can declare an output for an S3 bucket name and then call the <c>aws cloudformation describe-stacks</c> AWS CLI command to view the name.
        /// </para>
        /// </summary>
        /// <value>
        /// The outputs.
        /// </value>
        IEnumerable<IOutput> Outputs { get; }

        /// <summary>
        /// <para>
        /// Gets the parameters.
        /// </para>
        /// <para>
        /// Values to pass to your template at runtime (when you create or update a stack). You can refer to parameters from the Resources and Outputs sections of the template.
        /// </para>
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        IEnumerable<IParameter> Parameters { get; }

        /// <summary>
        /// <para>
        /// Gets the resources.
        /// </para>
        /// <para>
        /// Specifies the stack resources and their properties, such as an Amazon Elastic Compute Cloud instance or an Amazon Simple Storage Service bucket.
        /// You can refer to resources in the <c>Resources</c> and <c>Outputs</c> sections of the template.
        /// </para>
        /// </summary>
        /// <value>
        /// The resources.
        /// </value>
        IEnumerable<IResource> Resources { get; }

        /// <summary>
        /// <para>
        /// Gets or sets the rules.
        /// </para>
        /// <para>
        /// Validates a parameter or a combination of parameters passed to a template during a stack creation or stack update.
        /// </para>
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        RuleSection? Rules { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the transformations.
        /// </para>
        /// <para>
        /// For serverless applications (also referred to as Lambda-based applications),
        /// specifies the version of the AWS Serverless Application Model (AWS SAM) to use. When you specify a transform,
        /// you can use AWS SAM syntax to declare resources in your template.
        /// The model defines the syntax that you can use and how it's processed.
        /// </para>
        /// <para>
        /// You can also use AWS::Include transforms to work with template snippets that are stored separately from the main AWS CloudFormation template.
        /// You can store your snippet files in an Amazon S3 bucket and then reuse the functions across multiple templates.
        /// </para>
        /// </summary>
        /// <value>
        /// The transformations which will be <c>null</c> if the template did not provide this property.
        /// </value>
        object? Transform { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a Serverless Application Model template.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a SAM template; otherwise, <c>false</c>.
        /// </value>
        // ReSharper disable once StyleCop.SA1650
        // ReSharper disable once UnusedMemberInSuper.Global
        // ReSharper disable once InconsistentNaming
        bool IsSAMTemplate { get; }

        /// <summary>
        /// Gets any values for parameters set up from <see cref="IDeserializerSettings"/>
        /// </summary>
        /// <value>
        /// The parameter values.
        /// </value>
        IDictionary<string, object> UserParameterValues { get; }

        /// <summary>
        /// Gets any pseudo parameters discovered during template deserialization.
        /// </summary>
        /// <value>
        /// The pseudo parameters.
        /// </value>
        public List<IParameter> PseudoParameters { get; }
    }
}