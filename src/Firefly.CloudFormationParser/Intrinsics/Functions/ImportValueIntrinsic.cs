namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-importvalue.html">Fn::ImportValue</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class ImportValueIntrinsic : AbstractScalarIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!ImportValue";

        /// <summary>
        /// Gets or sets the name of the export.
        /// </summary>
        /// <value>
        /// The name of the export.
        /// </value>
        public object ExportName => this.Value;

        /// <inheritdoc />
        public override string LongName => "Fn::ImportValue";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            return new List<string>();
        }
    }
}