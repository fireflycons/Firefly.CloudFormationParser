namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Abstract base class for intrinsic type converters.
    /// Holds the value serializer used for compound properties of the intrinsic.
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.IYamlTypeConverter" />
    internal abstract class AbstractIntrinsicFunctionTypeConverter : IYamlTypeConverter
    {
        /// <summary>
        /// Gets or sets the value serializer.
        /// Unfortunately the API does not provide this in the <see cref="WriteYaml"/>
        /// method, so we are forced to set it after creation.
        /// </summary>
        /// <value>
        /// The value serializer.
        /// </value>
        internal IValueSerializer ValueSerializer { get; set; } = new NotImpementedValueSerializer();

        /// <summary>
        /// Gets a value indicating whether the current converter supports converting the specified type.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns><c>true</c> if this instance can handle the given type.</returns>
        public abstract bool Accepts(Type type);

        /// <summary>
        /// Reads an object's state from a YAML parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="type">Type of intrinsic to deserialize</param>
        /// <returns>Object that was read.</returns>
        /// <exception cref="System.NotImplementedException">Use ScalarTagDeserializer to read {type.Name}</exception>
        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException($"Use Deserializer classes to read {type.Name}");
        }

        /// <summary>
        /// Writes the specified object's state to a YAML emitter.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="value">The intrinsic to serialize,</param>
        /// <param name="type">Type of intrinsic to serialize.</param>
        public abstract void WriteYaml(IEmitter emitter, object? value, Type type);
    }
}