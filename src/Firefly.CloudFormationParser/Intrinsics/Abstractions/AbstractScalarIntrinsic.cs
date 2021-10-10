namespace Firefly.CloudFormationParser.Intrinsics.Abstractions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization.Serializers;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Base class for tags that have a scalar value
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public abstract class AbstractScalarIntrinsic : AbstractIntrinsic
    {
        /// <summary>
        /// The value of this intrinsic
        /// </summary>
        private object? intrinsicValue;

        /// <summary>
        /// Gets or sets the item list for the intrinsic
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public List<object> Items { get; protected set; } = new List<object>();

        /// <summary>
        /// Gets or sets the scalar value for the tag.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value
        {
            get
            {
                if (this.intrinsicValue == null)
                {
                    throw new InvalidOperationException($"{this.LongName}: Illegal null value.");
                }

                return this.intrinsicValue;
            }

            set => this.intrinsicValue = value;
        }

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new ScalarEmitterTrait();

        /// <inheritdoc />
        public override void SetValue(IEnumerable<object> values)
        {
            var list = values.ToList();

            this.ValidateValues(1, 1, list);
            this.Value = list.First();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.TagName} {this.Value}";
        }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            if (this.Value is IDictionary)
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

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            if (this.Value == null)
            {
                throw new InvalidOperationException($"{this.LongName}: Missing value.");
            }

            this.EmitterTrait.WriteLongForm(this, emitter, nestedValueSerializer, new[] { this.Value });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            if (this.Value == null)
            {
                throw new InvalidOperationException($"{this.LongName}: Missing value.");
            }

            this.EmitterTrait.WriteShortForm(this, emitter, nestedValueSerializer, new[] { this.Value });
        }
    }
}