namespace Firefly.CloudFormationParser.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents a path to a given property within an object graph such as an item within a template.
    /// </summary>
    [DebuggerDisplay("{Path}")]
    public class PropertyPath : Stack<string>, IEquatable<PropertyPath>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPath"/> class.
        /// </summary>
        public PropertyPath()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPath"/> class.
        /// </summary>
        /// <param name="pathComponents">The path components.</param>
        public PropertyPath(IEnumerable<string> pathComponents)
        {
            foreach (var c in pathComponents)
            {
                this.Push(c);
            }
        }

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="PropertyPath"/> class.
        /// </para>
        /// <para>
        /// This is effectively a clone mechanism so that a point in time capture of the current path can be taken.
        /// </para>
        /// </summary>
        /// <param name="other">Another property path.</param>
        private PropertyPath(PropertyPath other)
        {
            foreach (var prop in other.Reverse())
            {
                this.Push(prop);
            }
        }

        /// <summary>
        /// Gets the path depth (number of path components on the stack).
        /// </summary>
        /// <value>
        /// The depth.
        /// </value>
        public int Depth => this.Count;

        /// <summary>
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path => string.Join(".", this.Reverse());

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(PropertyPath? left, PropertyPath? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(PropertyPath? left, PropertyPath? right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A new <see cref="PropertyPath"/> with the current stack of elements</returns>
        public PropertyPath Clone()
        {
            return new PropertyPath(this);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(PropertyPath? other)
        {
            return other != null && this.SequenceEqual(other);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((PropertyPath)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Converts this path to a JSON path suitable for use with SelectToken
        /// </summary>
        /// <returns>JSON path</returns>
        /// <seealso href="https://www.newtonsoft.com/json/help/html/SelectToken.htm">Querying JSON with SelectToken</seealso>
        public string ToJsonPath()
        {
            if (!this.Any())
            {
                return string.Empty;
            }

            return this.Reverse().Select(p => p.All(char.IsDigit) ? $"[{p}]" : p)
                .Aggregate((jp, next) => jp + (next.StartsWith("[") ? next : $".{next}"));
        }
    }
}