namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-conditions.html#intrinsic-function-reference-conditions-and">Fn::And</see> intrinsic.
    /// </summary>
    /// <seealso cref="AbstractLogicalIntrinsic" />
    public class AndIntrinsic : AbstractLogicalIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!And";

        /// <inheritdoc />
        public override string LongName => "Fn::And";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override int MaxValues => 10;

        /// <inheritdoc />
        internal override int MinValues => 2;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            // Avoid multiple enumeration
            foreach (var operand in this.Operands)
            {
                // If any operand evaluates to false, the result is false
                if (operand is AbstractIntrinsic tag)
                {
                    if (!(bool)tag.Evaluate(template))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Convert.ToBoolean(operand))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}