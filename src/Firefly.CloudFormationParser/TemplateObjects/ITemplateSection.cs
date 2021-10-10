namespace Firefly.CloudFormationParser.TemplateObjects
{
    using Firefly.CloudFormationParser.Serialization.Deserializers;

    /// <summary>
    /// Interface describing a section of the template (Resources, Conditions etc.)
    /// </summary>
    public interface ITemplateSection
    {
        /// <summary>
        /// Gets the deserialization context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        DeserializationContext Context { get; }
    }
}