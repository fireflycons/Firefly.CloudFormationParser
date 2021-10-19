namespace Firefly.CloudFormationParser.Intrinsics.Abstractions
{
    using System.Collections;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Utils;

    /// <summary>
    /// Abstract base class for intrinsics that have an array of items
    /// </summary>
    /// <seealso cref="AbstractIntrinsic" />
    public abstract class AbstractArrayIntrinsic : AbstractIntrinsic
    {
        /// <summary>
        /// Gets or sets the item list for the intrinsic
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public List<object> Items { get; protected set; } = new List<object>();

        /// <summary>
        /// Gets the maximum number of values for this intrinsic.
        /// </summary>
        /// <value>
        /// The maximum values.
        /// </value>
        internal abstract int MaxValues { get; }

        /// <summary>
        /// Gets the minimum number of values for this intrinsic.
        /// </summary>
        /// <value>
        /// The minimum values.
        /// </value>
        internal abstract int MinValues { get; }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            var unresolved = new List<UnresolvedTagProperty>();
            var itemsProperty = this.GetType().GetProperty(nameof(AbstractScalarIntrinsic.Items));

            foreach (var (item, index) in this.Items.WithIndex())
            {
                switch (item)
                {
                    case IDictionary _:

                        unresolved.Add(
                            new UnresolvedTagProperty
                                {
                                    Property = itemsProperty, Index = index, Intrinsic = this
                                });
                        break;

                    case AbstractIntrinsic tag:

                        unresolved.AddRange(tag.GetUnresolvedDictionaryProperties());
                        break;
                }
            }

            return unresolved;
        }
    }
}