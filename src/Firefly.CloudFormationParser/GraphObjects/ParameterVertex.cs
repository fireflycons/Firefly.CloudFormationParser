namespace Firefly.CloudFormationParser.GraphObjects
{
    using Firefly.CloudFormationParser.TemplateObjects;

    using QuikGraph.Graphviz.Dot;

    /// <summary>
    /// Graph vertex that represents a template parameter
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.GraphObjects.AbstractVertex" />
    public class ParameterVertex : AbstractVertex, IParameterVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterVertex"/> class.
        /// </summary>
        /// <param name="templateObject">The template object.</param>
        public ParameterVertex(ITemplateObject templateObject)
            : base(templateObject)
        {
        }

        /// <inheritdoc />
        public override GraphvizVertexShape Shape => GraphvizVertexShape.Diamond;
    }
}