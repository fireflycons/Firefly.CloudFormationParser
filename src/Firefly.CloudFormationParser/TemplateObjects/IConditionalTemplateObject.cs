namespace Firefly.CloudFormationParser.TemplateObjects
{
    /// <summary>
    /// Interface for template objects that support Condition property (resources, outputs)
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.TemplateObjects.ITemplateObject" />
    public interface IConditionalTemplateObject : ITemplateObject
    {
        /// <summary>
        /// Gets the condition.
        /// </summary>
        /// <value>
        /// The condition.
        /// </value>
        string? Condition { get; }
    }
}