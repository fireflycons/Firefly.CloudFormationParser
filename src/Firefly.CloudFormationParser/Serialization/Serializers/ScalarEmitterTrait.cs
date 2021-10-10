namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Concrete trait for intrinsics that have a scalar operand
    /// </summary>
    /// <seealso cref="IEmitterTrait" />
    internal class ScalarEmitterTrait : IEmitterTrait
    {
        /// <summary>
        /// Writes the long form of the intrinsic.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer">An <see cref="IValueSerializer"/> with which to write nested objects such as dictionaries.</param>
        /// <param name="operands">The operands.</param>
        public void WriteLongForm(
            IIntrinsic intrinsic,
            IEmitter emitter,
            IValueSerializer nestedValueSerializer,
            IEnumerable<object> operands)
        {
            var operand = operands.First();

            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(intrinsic.LongName));

            if (operand is AbstractIntrinsic nestedIntrinsic)
            {
                nestedIntrinsic.WriteYaml(emitter, nestedValueSerializer);
            }
            else
            {
                var value = operand.ToString();

                emitter.Emit(
                    new Scalar(
                        AnchorName.Empty,
                        TagName.Empty,
                        value,
                        value.Any(c => c == '\r' || c == '\n') ? ScalarStyle.Literal : ScalarStyle.Any,
                        true,
                        false));
            }

            emitter.Emit(new MappingEnd());
        }

        /// <summary>
        /// Writes the short form of the intrinsic, using tags.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer">An <see cref="IValueSerializer"/> with which to write nested objects such as dictionaries.</param>
        /// <param name="operands">The operands.</param>
        public void WriteShortForm(
            IIntrinsic intrinsic,
            IEmitter emitter,
            IValueSerializer nestedValueSerializer,
            IEnumerable<object> operands)
        {
            var enumerable = operands as object[] ?? operands.ToArray();
            var operand = enumerable.First();

            if (operand is AbstractIntrinsic)
            {
                // This intrinsic must be emitted long form
                this.WriteLongForm(intrinsic, emitter, nestedValueSerializer, enumerable);
                return;
            }

            var value = operand.ToString();

            var t = new Scalar(
                AnchorName.Empty,
                new TagName(intrinsic.TagName),
                value,
                value.Any(c => c == '\r' || c == '\n') ? ScalarStyle.Literal : ScalarStyle.Any,
                false,
                false);
            emitter.Emit(t);
        }
    }
}