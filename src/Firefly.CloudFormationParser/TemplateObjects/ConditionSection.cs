namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Serialization.Deserializers;

    /// <summary>
    /// Container to hold the <c>Conditions:</c> section during template deserialization
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.TemplateObjects.ITemplateSection" />
    public class ConditionSection : Dictionary<string, object>, ITemplateSection
    {
        /// <inheritdoc cref="ITemplateSection.Context"/>
        public DeserializationContext Context => DeserializationContext.Conditions;
    }
}