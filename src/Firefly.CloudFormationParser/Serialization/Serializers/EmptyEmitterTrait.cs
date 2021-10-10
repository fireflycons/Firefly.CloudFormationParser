namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Empty emitter trait used for initialization of nun null property
    /// </summary>
    /// <seealso cref="IEmitterTrait" />
    internal class EmptyEmitterTrait : IEmitterTrait
    {
        /// <summary>
        /// Writes the long form of the intrinsic.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer"></param>
        /// <param name="operands">The operands.</param>
        public void WriteLongForm(
            IIntrinsic intrinsic,
            IEmitter emitter,
            IValueSerializer nestedValueSerializer,
            IEnumerable<object> operands)
        {
        }

        /// <summary>
        /// Writes the short form of the intrinsic, using tags.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer"></param>
        /// <param name="operands">The operands.</param>
        public void WriteShortForm(
            IIntrinsic intrinsic,
            IEmitter emitter,
            IValueSerializer nestedValueSerializer,
            IEnumerable<object> operands)
        {
        }
    }
}