namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Serialization.Deserializers;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-condition.html">Condition</see> intrinsic.
    /// </summary>
    /// <seealso cref="AbstractScalarIntrinsic" />
    [DebuggerDisplay("{TagName} {Value}")]
    public class ConditionIntrinsic : AbstractScalarIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Condition";

        /// <inheritdoc />
        public override string LongName => "Condition";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            var conditionName = (string)this.Value;

            if (template.EvaluatedConditions.ContainsKey(conditionName))
            {
                return template.EvaluatedConditions[conditionName];
            }

            // Will occur if an attempt to evaluate this condition is made before they have been processed byt the template processor
            throw new InvalidOperationException($"Condition '{conditionName}' has not been evaluated yet.");
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            return new List<string>();
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            var condition = this.Value.ToString();

            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(this.LongName));
            emitter.Emit(
                new Scalar(
                    AnchorName.Empty,
                    YamlDotNet.Core.TagName.Empty,
                    condition,
                    condition.Any(c => c == '\r' || c == '\n') ? ScalarStyle.Literal : ScalarStyle.Any,
                    true,
                    false));
            emitter.Emit(new MappingEnd());
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            var condition = this.Value.ToString();

            var t = new Scalar(
                AnchorName.Empty,
                new TagName(this.TagName),
                condition,
                condition.Any(c => c == '\r' || c == '\n') ? ScalarStyle.Literal : ScalarStyle.Any,
                false,
                false);
            emitter.Emit(t);
        }

        /// <inheritdoc />
        /// <remarks>
        /// When the parser is currently in the Resources section, do not deserialize a key named Condition as an intrinsic.
        /// </remarks>
        /// <seealso cref="IntrinsicFunctionNodeTypeResolver.DeserializationContextChanged"/>
        internal override bool ShouldDeserialize(DeserializationContext context)
        {
            return context != DeserializationContext.Resources;
        }
    }
}