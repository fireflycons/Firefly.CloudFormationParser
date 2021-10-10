namespace Firefly.CloudFormationParser.GraphObjects
{
    using Firefly.CloudFormationParser.TemplateObjects;

    using QuikGraph.Graphviz.Dot;

    /// <summary>
    /// Interface describing a graph vertex for the template dependency graph
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// Gets the template object associated with this vertex.
        /// </summary>
        /// <value>
        /// The template object.
        /// </value>
        ITemplateObject TemplateObject { get; }

        /// <summary>
        /// Gets the name of the vertex, which is the name assigned to the referenced template object.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the shape with which to draw the vertex.
        /// </summary>
        /// <value>
        /// The shape.
        /// </value>
        GraphvizVertexShape Shape { get; }
    }
}