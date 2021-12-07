namespace Firefly.CloudFormationParser.Utils
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents a path to a given property within an object graph such as an item within a template.
    /// </summary>
    [DebuggerDisplay("{Path}")]
    public class PropertyPath : Stack<string>
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
        /// Initializes a new instance of the <see cref="PropertyPath"/> class.
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
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path => string.Join(".", this.Reverse());

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A new <see cref="PropertyPath"/> with the current stack of elements</returns>
        public PropertyPath Clone()
        {
            return new PropertyPath(this);
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