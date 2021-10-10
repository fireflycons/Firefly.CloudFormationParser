namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.TemplateObjects;

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
            var param = template.Parameters.FirstOrDefault(p => p.Name == this.Reference);

            return param == null ? this.Reference : param.GetCurrentValue();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            if (this.Reference.StartsWith("AWS::"))
            {
                // Pseudo parameter reference
                var t = (Template)template;
                t.AddPseudoParameter(PseudoParameter.Create(this.Reference));
            }

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
    }
}