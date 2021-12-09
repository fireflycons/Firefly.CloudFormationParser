﻿namespace Firefly.CloudFormationParser.TemplateObjects.Traversal.AcceptExtensions
{
    /// <summary>
    /// Adds accept methods to <see cref="string"/>
    /// </summary>
    internal static class StringAcceptExtentsions
    {
        /// <summary>
        /// Performs dispatch for the current object using the visitor's DoAccept method.
        /// </summary>
        /// <typeparam name="TVisitor">The type of the visitor.</typeparam>
        /// <param name="self">The object accepting the visitor.</param>
        /// <param name="visitor">The visitor.</param>
        /// <returns>This dictionary</returns>
        public static string Accept<TVisitor>(this string self, TVisitor visitor)
            where TVisitor : ITemplateObjectVisitor<NullTemplateObjectVisitorContext>
        {
            visitor.DoAccept(self, new NullTemplateObjectVisitorContext());
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
        public static string Accept<TContext>(
            this string self,
            ITemplateObjectVisitor<TContext> visitor,
            TContext context)
            where TContext : ITemplateObjectVisitorContext<TContext>
        {
            visitor.DoAccept(self, context);
            return self;
        }
    }
}