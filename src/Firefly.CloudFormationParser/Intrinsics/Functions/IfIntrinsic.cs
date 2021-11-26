namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Serialization.Serializers;
    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-conditions.html#intrinsic-function-reference-conditions-if">Fn::If</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class IfIntrinsic : AbstractArrayIntrinsic, IBranchableIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!If";

        /// <summary>
        /// Gets or sets the condition.
        /// </summary>
        /// <value>
        /// The condition.
        /// </value>
        public string Condition { get; set; } = string.Empty;

        /// <inheritdoc />
        public override string LongName => "Fn::If";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <summary>
        /// Gets or sets the value if false.
        /// </summary>
        /// <value>
        /// The value if false.
        /// </value>
        public object ValueIfFalse => this.Items.Count == this.MaxValues - 1 ? this.Items[1] : "##UNSET";

        /// <summary>
        /// Gets or sets the value if true.
        /// </summary>
        /// <value>
        /// The value if true.
        /// </value>
        public object ValueIfTrue => this.Items.Count == this.MaxValues - 1 ? this.Items[0] : "##UNSET";

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new SequenceEmitterTrait();

        /// <inheritdoc />
        internal override int MaxValues => 3;

        /// <inheritdoc />
        internal override int MinValues => 3;

        /// <summary>
        /// Gets the branch based on condition evaluation.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>Either <see cref="ValueIfTrue" /> or <see cref="ValueIfFalse" /> based on conditions.</returns>
        /// <exception cref="Amazon.CloudFormation.Model.InvalidOperationException">Condition '{this.Condition} not found in Conditions section of template.</exception>
        public object GetBranch(ITemplate? template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template), "Template cannot be null to call GetBranch");
            }

            if (template.EvaluatedConditions.ContainsKey(this.Condition))
            {
                return template.EvaluatedConditions[this.Condition] ? this.ValueIfTrue : this.ValueIfFalse;
            }

            throw new InvalidOperationException(
                $"Condition '{this.Condition} not found in Conditions section of template.");
        }

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            var evaluation = this.GetBranch(template);

            if (evaluation is IIntrinsic intrinsic)
            {
                return intrinsic.Evaluate(template);
            }

            // Walk this object evaluating any nested intrinsics
            return evaluation.CopyAndEvaluateIntrinsics(template)!;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            if (template.EvaluatedConditions.ContainsKey(this.Condition))
            {
                var evaluation = template.EvaluatedConditions[this.Condition] ? this.ValueIfTrue : this.ValueIfFalse;

                if (evaluation is IIntrinsic intrinsic)
                {
                    return intrinsic.GetReferencedObjects(template);
                }

                // Walk this object evaluating any nested intrinsic references
                return evaluation.GetNestedReferences(template);
            }

            throw new InvalidOperationException(
                $"Condition '{this.Condition} not found in Conditions section of template.");
        }

        /// <inheritdoc />
        public override void SetValue(IEnumerable<object> values)
        {
            var list = values.ToList();

            this.ValidateValues(this.MinValues, this.MaxValues, list);
            this.Condition = (string)list[0];
            this.Items = list.Skip(1).Select(this.UnpackIntrinsic).ToList();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"!f [ {this.Condition}, {this.ValueIfTrue}, {this.ValueIfFalse} ]";
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.Condition, this.ValueIfTrue, this.ValueIfFalse });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.Condition, this.ValueIfTrue, this.ValueIfFalse });
        }
    }
}