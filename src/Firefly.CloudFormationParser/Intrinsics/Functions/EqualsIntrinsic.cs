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