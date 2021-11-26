namespace Firefly.CloudFormationParser.TemplateObjects
{
    /// <summary>
    /// Interface for template objects that marks them as able to be visited.
    /// </summary>
    public interface IVisitable
    {
        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="templateObjectVisitor">The visitor.</param>
        void Accept(ITemplateObjectVisitor templateObjectVisitor);
    }
}