namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Serialization.Serializers;
    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    ///  Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-select.html">Fn::Select</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class SelectIntrinsic : AbstractArrayIntrinsic, IBranchableIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Select";

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectIntrinsic"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global - used in TagRepository
        public SelectIntrinsic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public SelectIntrinsic(object value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useLongForm">If set to <c>true</c>, emit long form of intrinsic when serializing.</param>
        public SelectIntrinsic(object value, bool useLongForm)
            : base(value, useLongForm)
        {
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; private set; }

        /// <inheritdoc />
        public override string LongName => "Fn::Select";

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
            if (this.Items.Count == 1 && this.Items[0] is IIntrinsic intrinsic)
            {
                var results = intrinsic.Evaluate(template);

                if (results is IEnumerable enumerable)
                {
                    return enumerable.ToList()[this.Index];
                }
            }

            return this.Items[this.Index];
        }

        /// <summary>
        /// Gets the item indicated by the selection index.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>
        /// Selected item based on template or intrinsic conditions
        /// </returns>
        public object GetBranch(ITemplate? template)
        {
            return this.Items[this.Index];
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            var refs = new List<string>();

            foreach (var intrinsic in this.Items.Where(i => i is IIntrinsic).Cast<IIntrinsic>())
            {
                refs.AddRange(intrinsic.GetReferencedObjects(template));
            }

            return refs;
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.Index, (object)this.Items });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.Index, (object)this.Items });
        }

        /// <inheritdoc />
        protected override void SetValue(IEnumerable<object> values)
        {
            var list = values.ToList();

            this.ValidateValues(this.MinValues, this.MaxValues, list);

            if (list[0] is int ind)
            {
                this.Index = ind;
            }
            else
            {
                this.Index = int.Parse((string)list[0]);
            }

            if (list[1] is IEnumerable enumerable)
            {
                this.Items = enumerable.ToList().Select(this.UnpackIntrinsic).ToList();
            }
            else
            {
                this.Items = new List<object> { list[1] };
            }
        }
    }
}