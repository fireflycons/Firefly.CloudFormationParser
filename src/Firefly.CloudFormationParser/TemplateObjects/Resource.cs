namespace Firefly.CloudFormationParser.TemplateObjects
{
#pragma warning disable 1574
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents a template resource
    /// </summary>
    public class Resource : IConditionalTemplateObject, IResource
    {
        /// <inheritdoc cref="IResource.Condition"/>
        [YamlMember(Order = 0)]
        public string? Condition { get; set; }

        /// <inheritdoc cref="IResource.CreationPolicy"/>
        [YamlMember(Order = 4)]
        public Dictionary<string, object>? CreationPolicy { get; set; }

        /// <inheritdoc cref="IResource.DeletionPolicy"/>
        [YamlMember(Order = 5)]
        public string? DeletionPolicy { get; set; }

        /// <inheritdoc cref="IResource.DependsOn"/>
        [YamlMember(Order = 1)]
        public object? DependsOn { get; set; }

        /// <inheritdoc cref="IResource.Description"/>
        [YamlMember(Order = 3)]
        public object? Description { get; set; }

        /// <inheritdoc cref="IResource.ExplicitDependencies"/>
        [YamlIgnore]
        public IEnumerable<string> ExplicitDependencies =>
            this.DependsOn switch
                {
                    null => new List<string>(),
                    string s => new List<string> { s },
                    List<object> l => l.Select(d => (string)d),
                    _ => throw new InvalidCastException(
                             $"Unexpected type {this.DependsOn.GetType().Name} for DependsOn in resource {this.Name}")
                };

        /// <inheritdoc cref="IResource.Metadata"/>
        [YamlMember(Order = 9)]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <inheritdoc cref="IResource.Name"/>
        [YamlIgnore]
        public string Name { get; set; } = "Unresolved";

        /// <inheritdoc cref="IResource.Properties"/>
        [YamlMember(Order = 10)]
        public Dictionary<string, object>? Properties { get; set; }

        /// <inheritdoc cref="IResource.Type"/>
        [YamlMember(Order = 2)]
        public string? Type { get; set; }

        /// <inheritdoc cref="IResource.UpdatePolicy"/>
        [YamlMember(Order = 6)]
        public Dictionary<string, object>? UpdatePolicy { get; set; }

        /// <inheritdoc cref="IResource.UpdateReplacePolicy"/>
        [YamlMember(Order = 7)]
        public string? UpdateReplacePolicy { get; set; }

        /// <inheritdoc cref="IResource.Version"/>
        [YamlMember(Order = 8)]
        public string? Version { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Resource {this.Name}";
        }
    }
}