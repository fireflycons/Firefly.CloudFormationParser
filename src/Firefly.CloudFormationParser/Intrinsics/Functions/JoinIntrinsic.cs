namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Serialization.Serializers;
    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-join.html">Fn::Join</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class JoinIntrinsic : AbstractArrayIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Join";

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinIntrinsic"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global - used in TagRepository
        public JoinIntrinsic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public JoinIntrinsic(object value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useLongForm">If set to <c>true</c>, emit long form of intrinsic when serializing.</param>
        public JoinIntrinsic(object value, bool useLongForm)
            : base(value, useLongForm)
        {
        }

        /// <inheritdoc />
        public override string LongName => "Fn::Join";

        /// <summary>
        /// Gets the separator.
        /// </summary>
        /// <value>
        /// The separator.
        /// </value>
        public string Separator { get; private set; } = string.Empty;

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new NestedSequenceEmitterTrait();

        /// <inheritdoc />
        internal override int MaxValues => 2;

        /// <inheritdoc />
        internal override int MinValues => 2;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            var evaluations = new List<string>();

            foreach (var item in this.Items)
            {
                if (item is IIntrinsic intrinsic)
                {
                    evaluations.Add(intrinsic.Evaluate(template).ToString());
                }
                else
                {
                    evaluations.Add(item.ToString());
                }
            }

            return string.Join(this.Separator, evaluations);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            var refs = new List<string>();

            foreach (var item in this.Items.Where(i => i is AbstractIntrinsic).Cast<AbstractIntrinsic>())
            {
                refs.AddRange(item.GetReferencedObjects(template));
            }

            return refs.Distinct();
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.Separator, (object)this.Items });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.Separator, (object)this.Items });
        }

        /// <inheritdoc />
        protected override void SetValue(IEnumerable<object> values)
        {
            var list = EnumerableExtensions.ToList(values);
            this.Items = new List<object>();

            this.ValidateValues(this.MinValues, this.MaxValues, list);
            this.Separator = (string)list[0];

            if (list[1] is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    this.Items.Add(this.UnpackIntrinsic(item));
                }
            }
            else
            {
                this.Items = new List<object> { list[1] };
            }
        }
    }
}