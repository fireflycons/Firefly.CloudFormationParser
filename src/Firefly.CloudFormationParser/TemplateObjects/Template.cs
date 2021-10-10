﻿namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;
    using System.Linq;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents an entire CloudFormation Template
    /// </summary>
    public partial class Template : ITemplate
    {
        /// <inheritdoc/>
        [YamlMember(Order = 0)]
        public string? AWSTemplateFormatVersion { get; set; }

        /// <inheritdoc/>
        [YamlMember(Order = 8)]
        public GlobalSection? Globals { get; set; }

        /// <inheritdoc/>
        [YamlMember(Order = 1)]
        public object? Transform { get; set; }

        /// <inheritdoc/>
        [YamlMember(Order = 2)]
        public string? Description { get; set; }

        /// <inheritdoc/>
        [YamlMember(Order = 6)]
        public ConditionSection? Conditions { get; set; }

        /// <inheritdoc/>
        [YamlMember(Order = 5)]
        public RuleSection? Rules { get; set; }

        /// <inheritdoc/>
        [YamlMember(Order = 7)]
        public MappingSection? Mappings { get; set; }

        /// <inheritdoc/>
        [YamlMember(Order = 3)]
        public MetadataSection? Metadata { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the parsed outputs.
        /// </para>
        /// <para>
        /// Use the convenience property <see cref="Outputs"/> to access the outputs.
        /// </para>
        /// </summary>
        /// <value>
        /// The parsed outputs.
        /// </value>
        [YamlMember(Alias = "Outputs", Order = 10)]
        public OutputSection? ParsedOutputs { get; set; }

        /// <inheritdoc/>
        [YamlIgnore]
        public IEnumerable<IOutput> Outputs =>
            this.ParsedOutputs == null
                ? (IEnumerable<IOutput>)new List<IOutput>()
                : (IEnumerable<Output>)this.ParsedOutputs.Select(kv => kv.Value);

        /// <inheritdoc/>
        [YamlIgnore]
        public IEnumerable<IParameter> Parameters =>
            this.ParsedParameters == null
                ? new List<IParameter>()
                : (IEnumerable<IParameter>)this.ParsedParameters.Select(kv => kv.Value);

        /// <summary>
        /// <para>
        /// Gets or sets the parsed parameters.
        /// </para>
        /// <para>
        /// Use the convenience property <see cref="Parameters"/> to access the parameters.
        /// </para>
        /// </summary>
        /// <value>
        /// The parsed parameters.
        /// </value>
        [YamlMember(Alias = "Parameters", Order = 4)]
        public ParameterSection? ParsedParameters { get; set; }

        /// <inheritdoc/>
        [YamlIgnore]
        public IEnumerable<IResource> Resources => this.ParsedResources.Select(kv => kv.Value);

        /// <summary>
        /// <para>
        /// Gets or sets the parsed resources.
        /// </para>
        /// <para>
        /// Use the convenience property <see cref="Resources"/> to access the resources.
        /// </para>
        /// </summary>
        /// <value>
        /// The parsed resources.
        /// </value>
        /// <remarks>
        /// Not null as templates must have resources.
        /// </remarks>
        [YamlMember(Alias = "Resources", Order = 9)]
        public ResourceSection ParsedResources { get; set; } = new ResourceSection();
    }
}