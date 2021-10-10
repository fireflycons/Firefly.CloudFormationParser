namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Interface describing YAML emitter traits
    /// </summary>
    internal interface IEmitterTrait
    {
        /// <summary>
        /// Writes the long form of the intrinsic.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer">An <see cref="IValueSerializer"/> with which to write nested objects such as dictionaries.</param>
        /// <param name="operands">The operands.</param>
        void WriteLongForm(
            IIntrinsic intrinsic,
            IEmitter emitter,
            IValueSerializer nestedValueSerializer,
            IEnumerable<object> operands);

        /// <summary>
        /// Writes the short form of the intrinsic, using tags.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer">An <see cref="IValueSerializer"/> with which to write nested objects such as dictionaries.</param>
        /// <param name="operands">The operands.</param>
        void WriteShortForm(
            IIntrinsic intrinsic,
            IEmitter emitter,
            IValueSerializer nestedValueSerializer,
            IEnumerable<object> operands);
    }
}