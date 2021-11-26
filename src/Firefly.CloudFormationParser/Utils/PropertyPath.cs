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
        /// Gets the current path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path => string.Join(".", this.Reverse());
    }
}