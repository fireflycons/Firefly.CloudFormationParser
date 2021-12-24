namespace Firefly.CloudFormationParser.TemplateObjects
{
#pragma warning disable 1574
    using System.Diagnostics;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents a template output
    /// </summary>
    [DebuggerDisplay("Output {Name}")]
    public class Output : IConditionalTemplateObject, IOutput
    {
        /// <inheritdoc cref="IOutput.Condition"/>
        [YamlMember(Order = 0)]
        public string? Condition { get; set; }

        /// <inheritdoc cref="IOutput.Description"/>
        [YamlMember(Order = 1)]
        public string? Description { get; set; }

        /// <inheritdoc cref="IOutput.Export"/>
        [YamlMember(Order = 3)]
        public object? Export { get; set; }

        /// <inheritdoc cref="IOutput.Name"/>
        [YamlIgnore]
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc />
        [YamlIgnore]
        public ITemplate? Template { get; set; }

        /// <inheritdoc cref="IOutput.Value"/>
        [YamlMember(Order = 2)]
        public object Value { get; set; } = new object();
    }
}