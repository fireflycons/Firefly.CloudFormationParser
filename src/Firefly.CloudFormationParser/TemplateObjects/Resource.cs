namespace Firefly.CloudFormationParser.TemplateObjects
{
#pragma warning disable 1574
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents a template resource
    /// </summary>
    public class Resource : IConditionalTemplateObject, IResource
    {
        private object? transform;

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

        /// <summary>
        /// Gets or sets the function transform.
        /// </summary>
        /// <value>
        /// The function transform.
        /// </value>
        [YamlMember(Order = 11, Alias = "Fn::Transform", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public object? FnTransform
        {
            get => this.transform;

            set => this.transform = value;
        }

        /// <inheritdoc cref="IResource.Metadata"/>
        [YamlMember(Order = 9)]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <inheritdoc cref="IResource.Name"/>
        [YamlIgnore]
        public string Name { get; set; } = "Unresolved";

        /// <inheritdoc cref="IResource.Properties"/>
        [YamlMember(Order = 10)]
        public Dictionary<string, object>? Properties { get; set; }

        /// <summary>
        /// Gets or sets the transform.
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        [YamlMember(Order = 12)]
        public object? Transform
        {
            get => null;

            set => this.transform = value;
        }

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

        [YamlIgnore]
        internal ITemplate? Template { get; set; }

        /// <inheritdoc cref="IResource.GetResourcePropertyValue"/>
        public object? GetResourcePropertyValue(string propertyPath)
        {
            var prop = this.GetPropertyAtPath(propertyPath);
            var propValue = prop?.GetValue();

            if (!(propValue is IIntrinsic intrinsic))
            {
                // Value is scalar, list or dictionary
                return propValue;
            }

            if (this.Template == null)
            {
                // Should only get this for incorrectly set up tests.
                throw new InvalidOperationException(
                    $"Resource {this.Name}: Cannot evaluate intrinsic when template property is null");
            }

            // Evaluate the intrinsic and return its result
            return intrinsic.Evaluate(this.Template);

        }

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

        /// <inheritdoc cref="IResource.UpdateResourceProperty"/>
        public void UpdateResourceProperty(string propertyPath, object newValue)
        {
            var prop = this.GetPropertyAtPath(propertyPath);

            // TODO - If null, create property
            if (prop == null)
            {
                throw new InvalidOperationException($"Resource {this.Name}: No property to set at {propertyPath}");
            }

            prop.SetValue(newValue);
        }

        /// <summary>
        /// Gets an object that references a property value within the properties hierarchy.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>Object that can be used to get or set the value of the property within the resource.</returns>
        private ResourceProperty? GetPropertyAtPath(string propertyPath)
        {
            // TODO: Handle Serverless Globals, which when setting will have to create new properties on target object. Will need ref to parent template.
            if (propertyPath == null)
            {
                throw new ArgumentNullException(nameof(propertyPath), "Argument cannot be null");
            }

            var pathSegments = propertyPath.Split('.');

            if (this.Properties == null)
            {
                throw new FormatException($"Cannot find resource property {propertyPath} in resource {this.Name}");
            }

            object current = this.Properties;
            object parent = this.Properties;

            string finalSegment = string.Empty;
            string key = string.Empty;
            bool finished = false;

            foreach (var segment in pathSegments)
            {
                finalSegment = segment;
                parent = current;
                key = segment;

                switch (current)
                {
                    case IDictionary dict:

                        if (dict.Contains(segment))
                        {
                            current = dict[segment];
                            break;
                        }

                        return null;

                    case IList<object> list:

                        if (int.TryParse(segment, out var index))
                        {
                            current = list[index];
                            break;
                        }

                        return null;

                    default:

                        finished = true;
                        break;
                }

                if (finished)
                {
                    break;
                }
            }

            if (finalSegment == pathSegments.Last())
            {
                return new ResourceProperty(parent, key);
            }

            return null;
        }

        /// <summary>
        /// Object to manipulate the value at given location within resource properties
        /// </summary>
        private class ResourceProperty
        {
            public ResourceProperty(object container, string key)
            {
                this.Container = container;
                this.Key = key;
            }

            /// <summary>
            /// Gets the container within the property tree. Either a list or a dictionary
            /// </summary>
            /// <value>
            /// The container.
            /// </value>
            private object Container { get; }

            /// <summary>
            /// Gets the key to the container - key name for dict, index for list.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            private string Key { get; }

            /// <summary>
            /// Gets the value at the selected container and key.
            /// </summary>
            /// <returns>The value</returns>
            public object GetValue()
            {
                return this.Container switch
                    {
                        IDictionary dict => dict[this.Key],
                        IList list => list[int.Parse(this.Key)],
                        _ => throw new InvalidOperationException(
                                 $"Unexpected type {this.Container.GetType().Name} in resource properties.")
                    };
            }

            /// <summary>
            /// Sets the value at the selected container and key.
            /// </summary>
            /// <param name="value">The value.</param>
            public void SetValue(object? value)
            {
                switch (this.Container)
                {
                    case IDictionary dict:

                        dict[this.Key] = value.ToResourceSchema();
                        break;

                    case IList list:

                        list[int.Parse(this.Key)] = value.ToResourceSchema();
                        break;

                    default:

                        throw new InvalidOperationException(
                            $"Unexpected type {this.Container.GetType().Name} in resource properties.");
                }
            }
        }
    }
}