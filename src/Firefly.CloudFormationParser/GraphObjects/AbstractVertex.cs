namespace Firefly.CloudFormationParser.GraphObjects
{
    using Firefly.CloudFormationParser.TemplateObjects;

    using QuikGraph.Graphviz.Dot;

    /// <summary>
    /// Abstract base class for graph vertices
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.GraphObjects.IVertex" />
    public abstract class AbstractVertex : IVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractVertex"/> class.
        /// </summary>
        /// <param name="templateObject">The template object.</param>
        protected AbstractVertex(ITemplateObject templateObject)
        {
            this.TemplateObject = templateObject;
        }

        /// <inheritdoc />
        public string Name => this.TemplateObject.Name;

        /// <inheritdoc />
        public abstract GraphvizVertexShape Shape { get; }

        /// <inheritdoc />
        public ITemplateObject TemplateObject { get; }
    }
}