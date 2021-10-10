namespace Firefly.CloudFormationParser.GraphObjects
{
    /// <summary>
    /// Graph vertex representing a Condition in the template
    /// These are topologically sorted so that conditions with
    /// dependencies on other conditions are evaluated in the
    /// correct order.
    /// </summary>
    internal class ConditionVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionVertex"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ConditionVertex(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the condition.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }
    }
}