namespace Firefly.CloudFormationParser.Tests.Common
{
    using System;
    using System.Collections;

    using QuikGraph.Graphviz;
    using QuikGraph.Graphviz.Dot;

    /// <summary>
    /// Helper methods for test assertions
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Determines whether a given object graph rooted on <see cref="IDictionary"/> contains an object of the specified type.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [contains object of type] [the specified graph]; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsObjectOfType(IDictionary graph, Type type)
        {
            return graph != null && WalkDict(graph, type);
        }

        /// <summary>
        /// Determines whether a given object graph rooted on <see cref="IList"/> contains an object of the specified type.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [contains object of type] [the specified graph]; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsObjectOfType(IList graph, Type type)
        {
            return graph != null && WalkList(graph, type);
        }

        /// <summary>
        /// Generates a dot graph from a template for use in comparison with another template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>Object graph in DOT format</returns>
        public static string GenerateDotGraph(ITemplate template)
        {
            return template.DependencyGraph.ToGraphviz(
                algorithm =>
                    {
                        var font = new GraphvizFont("Arial", 9);

                        algorithm.CommonVertexFormat.Font = font;
                        algorithm.CommonEdgeFormat.Font = font;
                        algorithm.GraphFormat.RankDirection = GraphvizRankDirection.LR;
                        algorithm.FormatVertex += (sender, args) =>
                            {
                                args.VertexFormat.Shape = args.Vertex.Shape;
                                args.VertexFormat.Label = args.Vertex.Name;
                            };

                        algorithm.FormatEdge += (sender, args) =>
                            {
                                args.EdgeFormat.Label.Value = args.Edge.Tag?.AttributeName;
                            };
                    });
        }

        private static bool WalkDict(IDictionary dict, Type type)
        {
            foreach (DictionaryEntry kv in dict)
            {
                var key = kv.Key;

                switch (kv.Value)
                {
                    case IDictionary dict1:

                        if (WalkDict(dict1, type))
                        {
                            return true;
                        }

                        break;

                    case IList list:

                        if (WalkList(list, type))
                        {
                            return true;
                        }

                        break;

                    default:

                        if (type.IsInstanceOfType(kv.Value))
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        private static bool WalkList(IList list, Type type)
        {
            foreach (var obj in list)
            {
                switch (obj)
                {
                    case IDictionary dict1:

                        if (WalkDict(dict1, type))
                        {
                            return true;
                        }

                        break;

                    case IList list1:

                        if (WalkList(list1, type))
                        {
                            return true;
                        }

                        break;

                    default:

                        if (type.IsInstanceOfType(obj))
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }
    }
}