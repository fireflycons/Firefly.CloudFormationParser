namespace Firefly.CloudFormationParser.GraphObjects
{
    using System;

    /// <summary>
    /// Object describing the details of an inter-CloudFormation object relationship
    /// </summary>
    public class EdgeDetail : IEquatable<EdgeDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDetail"/> class.
        /// </summary>
        /// <param name="attributeName">Name of the attribute referenced from the source vertex.</param>
        public EdgeDetail(string attributeName)
        {
            this.ReferenceType = ReferenceType.AttributeReference;
            this.AttributeName = attributeName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDetail"/> class.
        /// </summary>
        /// <param name="referenceType">Type of the reference.</param>
        public EdgeDetail(ReferenceType referenceType)
        {
            if (referenceType == ReferenceType.AttributeReference)
            {
                throw new ArgumentException("Please use EdgeDetail(string) constructor with the attribute name to reference.");
            }

            this.ReferenceType = referenceType;

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            this.AttributeName = referenceType switch
                {
                    ReferenceType.DependsOn => "#Explicit",
                    ReferenceType.DirectReference => "id",
                    ReferenceType.ParameterReference => "#Param",
                    _ => throw new ArgumentException("Invalid reference type in this context.", nameof(referenceType)),
                };
        }

        /// <summary>
        /// <para>
        /// Gets the name of the resource attribute in the source vertex that is referenced by the target vertex. This can have the following values
        /// </para>
        /// <para>
        /// <list type="bullet">
        /// <item>
        /// <term><c>id</c> - </term>
        /// <description>The relationship is due to a direct <c>!Ref</c></description>
        /// </item>
        /// <item>
        /// <term><c>#Explicit</c> - </term>
        /// <description>The relationship is due to a <c>DependsOn</c></description>
        /// </item>
        /// <item>
        /// <term><c>#Param</c> - </term>
        /// <description>The relationship is due to a <c>!Ref</c> to a parameter or pseudo parameter</description>
        /// </item>
        /// <item>
        /// <term><i>string value</i> - </term>
        /// <description>The relationship is due to a <c>!GetAtt</c> where the property name of the source resource is <i>string value</i></description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        public string AttributeName { get; }

        /// <summary>
        /// Gets the type of the reference.
        /// </summary>
        /// <value>
        /// The type of the reference.
        /// </value>
        public ReferenceType ReferenceType { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(EdgeDetail? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.AttributeName == other.AttributeName && this.ReferenceType == other.ReferenceType;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == this.GetType() && this.Equals((EdgeDetail)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
             return (this.AttributeName.GetHashCode() * 397) ^ (int)this.ReferenceType;
        }
    }
}