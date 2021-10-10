namespace Firefly.CloudFormationParser
{
    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.TemplateObjects;

    /// <summary>
    /// Interface describing a CloudFormation Output.
    /// </summary>
    public interface IOutput : ITemplateObject
    {
        /// <summary>
        /// <para>
        /// Gets or sets the output's description.
        /// </para>
        /// <para>
        /// A string type that describes the output value. The value for the description declaration must be a literal string that's between 0 and 1024 bytes in length.
        /// </para> 
        /// </summary>
        /// <value>
        /// The output's description which will be <c>null</c> if the template did not provide this property.
        /// </value>
        string? Description { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the output's condition.
        /// </para>
        /// <para>
        /// When present, associates this output with a condition defined in the <c>Conditions</c> section of the template.
        /// </para>
        /// </summary>
        /// <value>
        /// The CloudFormation condition which will be <c>null</c> if the template did not provide this property.
        /// </value>
        string? Condition { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the output's value.
        /// </para>
        /// <para>
        /// The value of the property returned by the <c>aws cloudformation describe-stacks</c> command.
        /// The value of an output can include literals, parameter references, pseudo-parameters, a mapping value, or intrinsic functions.
        /// </para>
        /// <seealso cref="IIntrinsic"/>
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        object Value { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource output to be exported for a cross-stack reference.
        /// </summary>
        /// <value>
        /// The export name which will be <c>null</c> if the template did not provide this property.
        /// </value>
        object? Export { get; set; }
    }
}