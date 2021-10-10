namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Serialization.Serializers;

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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            var refs = new List<string>();

            foreach (var item in this.Items)
            {
                switch (item)
                {
                    case AbstractIntrinsic intrinsic:

                        refs.AddRange(intrinsic.GetReferencedObjects(template));
                        break;
                }
            }

            return refs;
        }

        /// <inheritdoc />
        public override void SetValue(IEnumerable<object> values)
        {
            var list = (List<object>)values;

            this.ValidateValues(this.MinValues, this.MaxValues, list);
            this.Separator = (string)list[0];

            if (list[1].GetType() == typeof(List<object>))
            {
                this.Items = (List<object>)list[1];
            }
            else
            {
                this.Items = new List<object> { list[1] };
            }
        }

        ///// <inheritdoc />
        // internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        // {
        // var unresolved = new List<UnresolvedTagProperty>();
        // var property = this.GetType().GetProperty(nameof(this.Items));

        // foreach (var (item, index) in this.Items.WithIndex())
        // {
        // switch (item)
        // {
        // case IDictionary _:

        // unresolved.Add(
        // new UnresolvedTagProperty { Property = property, Index = index, Intrinsic = this });
        // break;

        // case AbstractIntrinsic tag:

        // unresolved.AddRange(tag.GetUnresolvedDictionaryProperties());
        // break;
        // }
        // }

        // return unresolved;
        // }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(this, emitter, nestedValueSerializer, new[] { this.Separator, (object)this.Items });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(this, emitter, nestedValueSerializer, new[] { this.Separator, (object)this.Items });
        }
    }
}