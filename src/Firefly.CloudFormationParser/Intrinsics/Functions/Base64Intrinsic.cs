namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-base64.html">Fn::Base64</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class Base64Intrinsic : AbstractScalarIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Base64";

        /// <summary>
        /// Initializes a new instance of the <see cref="Base64Intrinsic"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global - used in TagRepository
        public Base64Intrinsic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Base64Intrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Base64Intrinsic(object value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Base64Intrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useLongForm">If set to <c>true</c>, emit long form of intrinsic when serializing.</param>
        public Base64Intrinsic(object value, bool useLongForm)
            : base(value, useLongForm)
        {
        }

        /// <inheritdoc />
        public override string LongName => "Fn::Base64";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <summary>
        /// Gets or sets the value to encode.
        /// </summary>
        /// <value>
        /// The value to encode.
        /// </value>
        public object ValueToEncode => this.Value is IEnumerable<object> @enum ? @enum.First() : this.Value;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            object value = this.ValueToEncode;

            if (this.ValueToEncode is AbstractIntrinsic intrinsic)
            {
                value = intrinsic.Evaluate(template);
            }

            if (value is string s)
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
            }

            throw new ArgumentException($"Cannot evaluate base64 on type {value.GetType().Name}");
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            if (this.ValueToEncode is AbstractIntrinsic intrinsic)
            {
                return intrinsic.GetReferencedObjects(template);
            }

            return new List<string>();
        }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            if (this.ValueToEncode is IDictionary)
            {
                return new List<UnresolvedTagProperty>
                           {
                               new UnresolvedTagProperty
                                   {
                                       Intrinsic = this, Property = this.GetType().GetProperty(nameof(this.Value))
                                   }
                           };
            }

            return new List<UnresolvedTagProperty>();
        }
    }
}