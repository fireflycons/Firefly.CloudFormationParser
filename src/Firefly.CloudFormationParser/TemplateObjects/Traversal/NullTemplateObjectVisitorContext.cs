namespace Firefly.CloudFormationParser.TemplateObjects.Traversal
{
    /// <summary>
    /// A empty implementation of the <see cref="ITemplateObjectVisitorContext{TContext}"/>.
    /// This just returns it self as the object graph is traversed and otherwise does nothing.
    /// </summary>
    public class NullTemplateObjectVisitorContext : ITemplateObjectVisitorContext<NullTemplateObjectVisitorContext>
    {
        /// <inheritdoc />
        public NullTemplateObjectVisitorContext Next(int index) => this;

        /// <inheritdoc />
        public NullTemplateObjectVisitorContext Next(string name) => this;
    }
}