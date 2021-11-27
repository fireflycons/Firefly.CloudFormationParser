namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-conditions.html#intrinsic-function-reference-conditions-equals">Fn::Equals</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class EqualsIntrinsic : AbstractLogicalIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Equals";

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsIntrinsic"/> class.
        /// </summary>
        public EqualsIntrinsic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public EqualsIntrinsic(object value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useLongForm">If set to <c>true</c>, emit long form of intrinsic when serializing.</param>
        public EqualsIntrinsic(object value, bool useLongForm)
            : base(value, useLongForm)
        {
        }

        /// <inheritdoc />
        public override string LongName => "Fn::Equals";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override int MaxValues => 2;

        /// <inheritdoc />
        internal override int MinValues => 2;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            var evaluations = this.ChangeType(
                this.Operands.Select(
                    v =>
                        {
                            if (v is AbstractIntrinsic tag)
                            {
                                return tag.Evaluate(template);
                            }

                            return v;
                        })).ToArray();

            return evaluations[0].Equals(evaluations[1]);
        }
    }
}