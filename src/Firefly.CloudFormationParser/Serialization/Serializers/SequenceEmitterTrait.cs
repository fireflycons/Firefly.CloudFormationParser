namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System.Collections;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Concrete trait for intrinsics that have a sequence as operands
    /// </summary>
    /// <seealso cref="IEmitterTrait" />
    internal class SequenceEmitterTrait : IEmitterTrait
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
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(intrinsic.LongName));
            emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, false, SequenceStyle.Block));

            foreach (var operand in operands)
            {
                switch (operand)
                {
                    case AbstractIntrinsic abstractIntrinsic:

                        abstractIntrinsic.WriteYaml(emitter, nestedValueSerializer);
                        break;

                    case IDictionary _:
                    case IList _:

                        nestedValueSerializer.SerializeValue(emitter, operand, operand.GetType());
                        break;

                    default:

                        emitter.Emit(
                            new Scalar(AnchorName.Empty, TagName.Empty, operand.ToString(), ScalarStyle.Any, true, false));
                        break;
                }
            }

            emitter.Emit(new SequenceEnd());
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
            emitter.Emit(
                new SequenceStart(AnchorName.Empty, new TagName(intrinsic.TagName), false, SequenceStyle.Block));

            foreach (var operand in operands)
            {
                switch (operand)
                {
                    case AbstractIntrinsic nestedIntrinsic:
                    
                        nestedIntrinsic.WriteYaml(emitter, nestedValueSerializer);
                        break;

                    case IDictionary _:
                    case IList _:
                        
                        nestedValueSerializer.SerializeValue(emitter, operand, operand.GetType());
                        break;

                    default:
                        
                        emitter.Emit(
                            new Scalar(AnchorName.Empty, TagName.Empty, operand.ToString(), ScalarStyle.Any, true, false));
                        break;
                }
            }

            emitter.Emit(new SequenceEnd());
        }
    }
}