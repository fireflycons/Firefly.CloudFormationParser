namespace Firefly.CloudFormationParser.GraphObjects
{
    using Firefly.CloudFormationParser.TemplateObjects;

    using QuikGraph.Graphviz.Dot;

    /// <summary>
    /// Graph vertex that represents an AWS pseudo parameter
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.GraphObjects.AbstractVertex" />
    public class PseudoParameterVertex : AbstractVertex, IParameterVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PseudoParameterVertex"/> class.
        /// </summary>
        /// <param name="templateObject">The template object.</param>
        public PseudoParameterVertex(ITemplateObject templateObject)
            : base(templateObject)
        {
        }

        /// <inheritdoc />
        public override GraphvizVertexShape Shape => GraphvizVertexShape.Diamond;
    }
}