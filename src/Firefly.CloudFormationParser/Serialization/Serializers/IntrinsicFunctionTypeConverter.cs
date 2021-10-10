namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    using YamlDotNet.Core;

    /// <summary>
    /// Base class for intrinsic function serialization.
    /// </summary>
    /// <typeparam name="T">Type of intrinsic to serialize</typeparam>
    internal class IntrinsicFunctionTypeConverter<T> : AbstractIntrinsicFunctionTypeConverter
        where T : AbstractIntrinsic
    {
        /// <summary>
        /// Gets a value indicating whether the current converter supports converting the specified type.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns><c>true</c> if this instance can handle the given type.</returns>
        public override bool Accepts(Type type)
        {
            return type == typeof(T);
        }

        /// <summary>
        /// Writes the specified object's state to a YAML emitter.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="value">The intrinsic to serialize,</param>
        /// <param name="type">Type of intrinsic to serialize.</param>
        public override void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value == null)
            {
                return;
            }

            var intrinsic = (T)value;
            intrinsic.WriteYaml(emitter, this.ValueSerializer);
        }
    }
}