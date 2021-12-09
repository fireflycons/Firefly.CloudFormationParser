namespace Firefly.CloudFormationParser.TemplateObjects.Traversal.AcceptExtensions
{
    /// <summary>
    /// Adds accept methods to <see cref="IResource"/> to visit the resource's properties.
    /// </summary>
    public static class ResourceAcceptExtensions
    {
        /// <summary>
        /// Performs dispatch for the current object using the visitor's DoAccept method.
        /// </summary>
        /// <typeparam name="TVisitor">The type of the visitor.</typeparam>
        /// <param name="self">The object accepting the visitor.</param>
        /// <param name="visitor">The visitor.</param>
        /// <returns>This dictionary</returns>
        public static IResource Accept<TVisitor>(this IResource self, TVisitor visitor)
            where TVisitor : ITemplateObjectVisitor<NullTemplateObjectVisitorContext>
        {
            if (self.Properties != null)
            {
                visitor.DoAccept(self.Properties, new NullTemplateObjectVisitorContext());
            }

            return self;
        }

        /// <summary>
        /// Performs dispatch for the current object using the visitor's DoAccept method.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="self">The object accepting the visitor.</param>
        /// <param name="visitor">The visitor.</param>
        /// <param name="context">The context.</param>
        /// <returns>This JToken</returns>
        public static IResource Accept<TContext>(
            this IResource self,
            ITemplateObjectVisitor<TContext> visitor,
            TContext context)
            where TContext : ITemplateObjectVisitorContext<TContext>
        {
            if (self.Properties != null)
            {
                visitor.DoAccept(self.Properties, context);
            }

            return self;
        }
    }
}