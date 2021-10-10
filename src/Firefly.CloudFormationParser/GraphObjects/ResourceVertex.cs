namespace Firefly.CloudFormationParser.GraphObjects
{
    using Firefly.CloudFormationParser.TemplateObjects;

    using QuikGraph.Graphviz.Dot;

    /// <summary>
    /// Graph vertex that represents an AWS resource
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.GraphObjects.AbstractVertex" />
    public class ResourceVertex : AbstractVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceVertex"/> class.
        /// </summary>
        /// <param name="templateObject">The template object.</param>
        public ResourceVertex(ITemplateObject templateObject)
            : base(templateObject)
        {
        }

        /// <inheritdoc />
        public override GraphvizVertexShape Shape => GraphvizVertexShape.Box;
    }
}