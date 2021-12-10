namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
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
        /// Initializes a new instance of the <see cref="ImportValueIntrinsic"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global - used in TagRepository
        public ImportValueIntrinsic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportValueIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        // ReSharper disable once UnusedMember.Global
        public ImportValueIntrinsic(object value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportValueIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useLongForm">If set to <c>true</c>, emit long form of intrinsic when serializing.</param>
        public ImportValueIntrinsic(object value, bool useLongForm)
            : base(value, useLongForm)
        {
        }

        /// <inheritdoc />
        public override IntrinsicType Type => IntrinsicType.ImportValue;

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
            return this.ExportName is IIntrinsic intrinsic ? intrinsic.Evaluate(template) : this.ExportName.ToString();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            return this.ExportName is IIntrinsic intrinsic
                       ? intrinsic.GetReferencedObjects(template)
                       : new List<string>();
        }
    }
}