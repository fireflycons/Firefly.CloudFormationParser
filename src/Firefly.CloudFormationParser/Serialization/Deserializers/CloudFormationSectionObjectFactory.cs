namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;

    using Firefly.CloudFormationParser.TemplateObjects;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Replacement for the default object factory.
    /// Since there are no interface types as members of the custom objects we will deserialize, that logic is not necessary here.
    /// An event is fired when an <see cref="ITemplateObject"/> is being created to indicate to <see cref="IntrinsicFunctionNodeTypeResolver"/>
    /// where in the template we are, so that the deserialization rules may be adjusted.
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.IObjectFactory" />
    internal class CloudFormationSectionObjectFactory : IObjectFactory
    {
        /// <summary>
        /// Occurs when [deserialization context changing].
        /// </summary>
        public event EventHandler<DeserializationContextChangingEventArgs>? DeserializationContextChanging;

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to create</param>
        /// <returns>A new instance of the type.</returns>
        /// <exception cref="System.InvalidOperationException">Failed to create an instance of type '<i>type name</i>'</exception>
        public object Create(Type type)
        {
            try
            {
                var instance =  Activator.CreateInstance(type)!;

                if (instance is ITemplateSection section)
                {
                    this.DeserializationContextChanging?.Invoke(this, new DeserializationContextChangingEventArgs { CurrentContext = section.Context });
                }

                return instance;
            }
            catch (Exception err)
            {
                var message = $"Failed to create an instance of type '{type.FullName}'.";
                throw new InvalidOperationException(message, err);
            }
        }
    }
}