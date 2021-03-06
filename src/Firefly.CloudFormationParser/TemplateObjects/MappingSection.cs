namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Serialization.Deserializers;

    /// <summary>
    /// Container to hold the <c>Mappings:</c> section during template deserialization
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.TemplateObjects.ITemplateSection" />
    public class MappingSection : Dictionary<string, object>, ITemplateSection, ITemplateObject
    {
        /// <inheritdoc cref="ITemplateSection.Context"/>
        public DeserializationContext Context => DeserializationContext.Mappings;

        /// <inheritdoc />
        public string Name { get; set; } = "Mappings";

        /// <inheritdoc />
        public ITemplate? Template { get; set; }
    }
}