namespace Firefly.CloudFormationParser.Intrinsics.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Used when walking through intrinsic function hierarchies.
    /// </summary>
    internal class UnresolvedTagProperty
    {
        /// <summary>
        /// Gets or sets the index where the property is a list or array type.
        /// </summary>
        /// <value>
        /// The index. If value is -1 then property is not indexed
        /// </value>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Gets or sets the tag to operate on.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public IIntrinsic? Intrinsic { get; set; }

        /// <summary>
        /// Gets or sets the property within the given <see cref="Intrinsic"/> to modify.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public PropertyInfo? Property { get; set; }

        public object? Reference { get; set; }

        /// <summary>
        /// Gets the dictionary value of the given property.
        /// </summary>
        /// <returns>Property value as a dictionary</returns>
        public Dictionary<object, object> GetDictionaryValue()
        {
            if (this.Property == null)
            {
                if (this.Reference != null)
                {
                    return (Dictionary<object, object>)this.Reference;
                }

                return new Dictionary<object, object>();
            }

            var value = this.Property.GetValue(this.Intrinsic);

            return value switch
                {
                    IList list => (Dictionary<object, object>)list[this.Index],
                    Dictionary<object, object> dict => dict,
                    _ => throw new InvalidCastException(
                             $"Unexpected type {value.GetType().Name} on property {this.Property.Name} for {this.Intrinsic?.LongName}")
                };
        }

        /// <summary>
        /// Sets the value of the referenced property to the given tag value.
        /// </summary>
        /// <param name="intrinsic">The tag.</param>
        public void SetValue(IIntrinsic? intrinsic)
        {
            if (this.Property != null)
            {
                var value = this.Property.GetValue(this.Intrinsic);

                if (value is IList list)
                {
                    list[this.Index] = intrinsic;
                }
                else
                {
                    this.Property.SetValue(
                        this.Intrinsic,
                        intrinsic,
                        this.Index == -1 ? null : new[] { (object)this.Index });
                }
            }

            if (this.Reference != null)
            {
                this.Reference = intrinsic;
            }
        }
    }
}