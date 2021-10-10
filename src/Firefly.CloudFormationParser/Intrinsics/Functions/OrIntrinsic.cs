﻿namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-conditions.html#intrinsic-function-reference-conditions-or">Fn::Or</see> intrinsic.
    /// </summary>
    /// <seealso cref="AbstractLogicalIntrinsic" />
    public class OrIntrinsic : AbstractLogicalIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Or";

        /// <inheritdoc />
        public override string LongName => "Fn::Or";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override int MaxValues => 10;

        /// <inheritdoc />
        internal override int MinValues => 2;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            foreach (var operand in this.Operands)
            {
                // If any operand evaluates to true, the result is true
                if (operand is AbstractIntrinsic tag)
                {
                    if ((bool)tag.Evaluate(template))
                    {
                        return true;
                    }
                }
                else
                {
                    if (Convert.ToBoolean(operand))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}