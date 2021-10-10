namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Serialization.Deserializers;

    /// <summary>
    /// Container to hold the <c>Outputs:</c> section during template deserialization
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.TemplateObjects.ITemplateSection" />
    public class OutputSection : Dictionary<string, Output>, ITemplateSection
    {
        /// <inheritdoc cref="ITemplateSection.Context"/>
        public DeserializationContext Context => DeserializationContext.Outputs;
    }
}