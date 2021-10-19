namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;


    /// <summary>
    /// C# 8 detail - used to provide a default for <see cref="AbstractIntrinsicFunctionTypeConverter.ValueSerializer"/>
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.IValueSerializer" />
    internal class NotImpementedValueSerializer : IValueSerializer
    {
        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.NotImplementedException">Not implemented.</exception>
        public void SerializeValue(IEmitter emitter, object? value, Type? type)
        {
            throw new NotImplementedException();
        }
    }
}