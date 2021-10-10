namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-conditions.html#intrinsic-function-reference-conditions-not">Fn::Not</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class NotIntrinsic : AbstractLogicalIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Not";

        /// <inheritdoc />
        public override string LongName => "Fn::Not";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override int MaxValues => 1;

        /// <inheritdoc />
        internal override int MinValues => 1;

        /// <summary>
        /// Gets the expression to negate.
        /// </summary>
        /// <value>
        /// The expression.
        /// </value>
        private object Expression =>
            this.Operands.Any()
                ? this.Operands.First()
                : throw new InvalidOperationException("!Not: Cannot have this intrinsic without a value");

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            if (this.Expression is AbstractIntrinsic tag)
            {
                return !(bool)tag.Evaluate(template);
            }

            return !Convert.ToBoolean(this.ChangeType(new[] { this.Expression }).First());
        }
    }
}