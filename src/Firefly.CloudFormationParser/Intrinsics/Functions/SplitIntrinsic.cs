namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization.Serializers;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-split.html">Fn::Split</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class SplitIntrinsic : AbstractArrayIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Split";

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitIntrinsic"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global - used in TagRepository
        public SplitIntrinsic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public SplitIntrinsic(object value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useLongForm">If set to <c>true</c>, emit long form of intrinsic when serializing.</param>
        public SplitIntrinsic(object value, bool useLongForm)
            : base(value, useLongForm)
        {
        }

        /// <inheritdoc />
        public override IntrinsicType Type => IntrinsicType.Split;

        /// <summary>
        /// Gets the delimiter on which to split.
        /// </summary>
        /// <value>
        /// The delimiter.
        /// </value>
        public string Delimiter { get; private set; } = string.Empty;

        /// <inheritdoc />
        public override string LongName => "Fn::Split";

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public object Source { get; set; } = new object();

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new SequenceEmitterTrait();

        /// <inheritdoc />
        internal override int MaxValues => 2;

        /// <inheritdoc />
        internal override int MinValues => 2;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            string source;

            if (this.Source is IIntrinsic intrinsic)
            {
                var value = intrinsic.Evaluate(template);

                if (value is string s)
                {
                    source = s;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Fn::Split: Cannot split result of {intrinsic.LongName} as the result is not a single string");
                }
            }
            else
            {
                source = this.Source.ToString();
            }

            return source.Split(new[] { this.Delimiter }, StringSplitOptions.None);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            if (this.Source is IIntrinsic intrinsic)
            {
                return intrinsic.GetReferencedObjects(template);
            }

            return new List<string>();
        }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            if (this.Source is IDictionary)
            {
                return new List<UnresolvedTagProperty>
                           {
                               new UnresolvedTagProperty
                                   {
                                       Intrinsic = this, Property = this.GetType().GetProperty(nameof(this.Source))
                                   }
                           };
            }

            return new List<UnresolvedTagProperty>();
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.Delimiter, this.Source });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.Delimiter, this.Source });
        }

        /// <inheritdoc />
        protected override void SetValue(IEnumerable<object> values)
        {
            var list = values.ToList().Select(this.UnpackIntrinsic).ToList();

            this.ValidateValues(this.MinValues, this.MaxValues, list);
            this.Delimiter = (string)list[0];
            this.Source = list[1];
        }
    }
}