namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-ref.html">Ref</see> intrinsic.
    /// </summary>
    /// <seealso cref="AbstractScalarIntrinsic" />
    public class RefIntrinsic : AbstractScalarIntrinsic, IReferenceIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Ref";

        /// <summary>
        /// Initializes a new instance of the <see cref="RefIntrinsic"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global - used in TagRepository
        public RefIntrinsic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public RefIntrinsic(object value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useLongForm">If set to <c>true</c>, emit long form of intrinsic when serializing.</param>
        public RefIntrinsic(object value, bool useLongForm)
            : base(value, useLongForm)
        {
        }

        /// <inheritdoc />
        public override IntrinsicType Type => IntrinsicType.Ref;

        /// <inheritdoc />
        public override string LongName => "Ref";

        /// <summary>
        /// Gets name of the resource or parameter being referenced.
        /// </summary>
        /// <value>
        /// The reference.
        /// </value>
        public string Reference =>
            this.Value != null
                ? (string)this.Value
                : throw new InvalidOperationException("!Ref: Referenced object cannot be null");

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            var param = template.Parameters.Concat(template.PseudoParameters)
                .FirstOrDefault(p => p.Name == this.Reference);

            // Note that we can't dereference the physical ID of a resource here so we must return the logical name.
            return param == null ? this.Reference : param.GetCurrentValue();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            return new List<string> { this.Reference };
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"!Ref {this.Reference}";
        }

        /// <inheritdoc />
        public string ReferencedObject(ITemplate template)
        {
            return this.Value.ToString();
        }
    }
}