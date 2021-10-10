namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

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