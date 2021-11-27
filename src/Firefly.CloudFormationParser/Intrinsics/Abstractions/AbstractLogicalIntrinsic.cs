namespace Firefly.CloudFormationParser.Intrinsics.Abstractions
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization.Serializers;
    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Base class for tags that evaluate to a boolean result, i.e. those that can be used in conditions.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public abstract class AbstractLogicalIntrinsic : AbstractArrayIntrinsic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractLogicalIntrinsic"/> class.
        /// </summary>
        protected AbstractLogicalIntrinsic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractLogicalIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        protected AbstractLogicalIntrinsic(object value)
        : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractLogicalIntrinsic"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useLongForm">If set to <c>true</c>, emit long form of intrinsic when serializing.</param>
        protected AbstractLogicalIntrinsic(object value, bool useLongForm)
            : base(value, useLongForm)
        {
        }

        /// <summary>
        /// Parses a boolean value as per the YAML specification.
        /// </summary>
        private static readonly Regex YamlBoolean = new Regex(
            @"^((?<true>y|Y|yes|Yes|YES|true|True|TRUE|on|On|ON)|(?<false>n|N|no|No|NO|false|False|FALSE|off|Off|OFF))$");

        /// <summary>
        /// Gets or sets the operands.
        /// </summary>
        /// <value>
        /// The operands.
        /// </value>
        public IEnumerable<object> Operands => this.Items;

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new SequenceEmitterTrait();

        /// <summary>
        /// Returns a list of all condition intrinsics as operands in this and child intrinsics.
        /// </summary>
        /// <returns>A list of condition intrinsics</returns>
        public IList<ConditionIntrinsic> GetConditionIntrinsics()
        {
            var conditionIntrinsics = new List<ConditionIntrinsic>();

            foreach (var operand in this.Items)
            {
                switch (operand)
                {
                    case ConditionIntrinsic cond:

                        conditionIntrinsics.Add(cond);
                        break;

                    case AbstractLogicalIntrinsic logical:

                        conditionIntrinsics.AddRange(logical.GetConditionIntrinsics());
                        break;
                }
            }

            return conditionIntrinsics;
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
        protected override void SetValue(IEnumerable<object> values)
        {
            this.Items = values.Select(this.UnpackIntrinsic).ToList();
            this.ValidateValues(this.MinValues, this.MaxValues, this.Items);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.TagName} [ {string.Join(", ", this.Items.Select(o => o.ToString()))} ]";
        }

        /// <summary>
        /// Changes the type.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <returns>new enumeration with member types changed</returns>
        internal IEnumerable<object> ChangeType(IEnumerable<object> inputs)
        {
            foreach (var input in inputs)
            {
                if (input is string s)
                {
                    if (double.TryParse(s, out var d))
                    {
                        yield return d;
                    }
                    else
                    {
                        var m = YamlBoolean.Match(s);

                        if (m.Success)
                        {
                            yield return !string.IsNullOrEmpty(m.Groups["true"].Value);
                        }
                        else
                        {
                            yield return s;
                        }
                    }
                }
                else
                {
                    yield return input;
                }
            }
        }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            var unresolved = new List<UnresolvedTagProperty>();
            var property = this.GetType().GetProperty(nameof(this.Items));

            foreach (var (item, index) in this.Items.WithIndex())
            {
                switch (item)
                {
                    case IDictionary _:

                        unresolved.Add(
                            new UnresolvedTagProperty { Property = property, Index = index, Intrinsic = this });
                        break;

                    case AbstractIntrinsic tag:

                        unresolved.AddRange(tag.GetUnresolvedDictionaryProperties());
                        break;
                }
            }

            return unresolved;
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(this, emitter, nestedValueSerializer, this.Items);
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(this, emitter, nestedValueSerializer, this.Items);
        }
    }
}