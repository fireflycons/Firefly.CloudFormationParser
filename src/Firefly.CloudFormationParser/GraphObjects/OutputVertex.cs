namespace Firefly.CloudFormationParser.GraphObjects
{
    using Firefly.CloudFormationParser.TemplateObjects;

    using QuikGraph.Graphviz.Dot;

    /// <summary>
    /// Graph vertex that represents a template output.
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.GraphObjects.AbstractVertex" />
    public class OutputVertex : AbstractVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputVertex"/> class.
        /// </summary>
        /// <param name="templateObject">The template object.</param>
        public OutputVertex(ITemplateObject templateObject)
            : base(templateObject)
        {
        }

        /// <inheritdoc />
        public override GraphvizVertexShape Shape => GraphvizVertexShape.Circle;
    }
}