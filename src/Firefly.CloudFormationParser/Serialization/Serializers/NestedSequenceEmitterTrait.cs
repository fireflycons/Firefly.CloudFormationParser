namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// <para>
    /// Concrete trait for intrinsics that have a nested list, e.g. !Select and !Join
    /// </para>
    /// <para>
    /// The nested list may be the evaluation of another intrinsic, or a list of scalar values
    /// </para>
    /// </summary>
    /// <seealso cref="IEmitterTrait" />
    internal class NestedSequenceEmitterTrait : IEmitterTrait
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
            var items = operands.ToArray();
            var fixedArgument = items[0];
            var listArguments = ((IEnumerable<object>)items[1]).ToList();

            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(intrinsic.LongName));
            emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, false, SequenceStyle.Block));
            emitter.Emit(
                new Scalar(AnchorName.Empty, TagName.Empty, fixedArgument.ToString(), ScalarStyle.Any, false, true));

            if (listArguments.Count == 1)
            {
                if (listArguments.First() is AbstractIntrinsic nestedIntrinsic)
                {
                    // Second sequence item is an intrinsic that should evaluate to a list
                    nestedIntrinsic.WriteYaml(emitter, nestedValueSerializer);
                }
                else
                {
                    throw new ArgumentException(
                        $"{intrinsic.LongName}: When only one element in list of objects, this must be an intrinsic returning a list.");
                }
            }
            else
            {
                emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, false, SequenceStyle.Block));

                foreach (var operand in listArguments)
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
                                new Scalar(
                                    AnchorName.Empty,
                                    TagName.Empty,
                                    operand.ToString(),
                                    ScalarStyle.Any,
                                    false,
                                    true));
                            break;
                    }
                }

                emitter.Emit(new SequenceEnd());
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
            var items = operands.ToArray();
            var fixedArgument = items[0];
            var listArguments = ((IEnumerable<object>)items[1]).ToList();

            emitter.Emit(
                new SequenceStart(AnchorName.Empty, new TagName(intrinsic.TagName), false, SequenceStyle.Block));
            emitter.Emit(
                new Scalar(AnchorName.Empty, TagName.Empty, fixedArgument.ToString(), ScalarStyle.Any, false, true));

            if (listArguments.Count == 1)
            {
                if (listArguments.First() is AbstractIntrinsic nestedIntrinsic)
                {
                    // Second sequence item is an intrinsic that should evaluate to a list
                    nestedIntrinsic.WriteYaml(emitter, nestedValueSerializer);
                }
                else
                {
                    throw new ArgumentException(
                        $"{intrinsic.LongName}: When only one element in list of objects, this must be an intrinsic returning a list.");
                }
            }
            else
            {
                emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, false, SequenceStyle.Block));

                foreach (var operand in listArguments)
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

                            emitter.Emit(new Scalar(operand.ToString()));
                            break;
                    }
                }

                emitter.Emit(new SequenceEnd());
            }

            emitter.Emit(new SequenceEnd());
        }
    }
}