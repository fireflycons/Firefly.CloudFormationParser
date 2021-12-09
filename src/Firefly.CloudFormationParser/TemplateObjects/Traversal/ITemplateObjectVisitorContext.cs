namespace Firefly.CloudFormationParser.TemplateObjects.Traversal
{
    /// <summary>
    /// This interface defines methods for altering the context as the object graph is traversed.
    /// <br/>
    /// It is up to the implementation to choose if that changes a state in the context or if it returns a new context.
    /// </summary>
    /// <typeparam name="TContext">The actual type of the context itself, this is to guide in using the right context type at all times.</typeparam>
    public interface ITemplateObjectVisitorContext<out TContext>
    {
        /// <summary>
        /// Gets the next context for an item in a list.
        /// </summary>
        /// <param name="index">Index in current list</param>
        /// <returns>Current or new context.</returns>
        TContext Next(int index);

        /// <summary>
        /// Gets the next context for an entry in a dictionary 
        /// </summary>
        /// <param name="name">Name of property.</param>
        /// <returns>Current or new context.</returns>
        TContext Next(string name);
    }
}