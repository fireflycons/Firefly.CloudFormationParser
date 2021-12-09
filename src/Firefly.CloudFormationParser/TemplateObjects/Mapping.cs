namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Top level mapping in the mappings section
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.TemplateObjects.ITemplateObject" />
    public class Mapping : Dictionary<string, object>, ITemplateObject
    {
        /// <inheritdoc />
        [YamlIgnore]
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc />
        [YamlIgnore]
        public ITemplate? Template { get; set;  }
    }
}