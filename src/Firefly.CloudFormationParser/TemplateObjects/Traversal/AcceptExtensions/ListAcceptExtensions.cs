namespace Firefly.CloudFormationParser.TemplateObjects.Traversal.AcceptExtensions
{
    using System.Collections.Generic;

    /// <summary>
    /// Adds accept methods to <see cref="IList{T}"/>
    /// </summary>
    internal static class ListAcceptExtensions
    {
        /// <summary>
        /// Performs dispatch for the current object using the visitor's DoAccept method.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the list</typeparam>
        /// <typeparam name="TVisitor">The type of the visitor.</typeparam>
        /// <param name="self">The object accepting the visitor.</param>
        /// <param name="visitor">The visitor.</param>
        /// <returns>This dictionary</returns>
        public static IList<TItem> Accept<TItem, TVisitor>(this IList<TItem> self, TVisitor visitor)
            where TVisitor : ITemplateObjectVisitor<NullTemplateObjectVisitorContext>
        {
            visitor.DoAccept(self, new NullTemplateObjectVisitorContext());
            return self;
        }

        /// <summary>
        /// Performs dispatch for the current object using the visitor's DoAccept method.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the list</typeparam>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="self">The object accepting the visitor.</param>
        /// <param name="visitor">The visitor.</param>
        /// <param name="context">The context.</param>
        /// <returns>This JToken</returns>
        public static IList<TItem> Accept<TItem, TContext>(
            this IList<TItem> self,
            ITemplateObjectVisitor<TContext> visitor,
            TContext context)
            where TContext : ITemplateObjectVisitorContext<TContext>
        {
            visitor.DoAccept(self, context);
            return self;
        }
    }
}