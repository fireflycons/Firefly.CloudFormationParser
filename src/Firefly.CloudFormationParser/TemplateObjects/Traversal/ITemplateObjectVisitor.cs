namespace Firefly.CloudFormationParser.TemplateObjects.Traversal
{
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    /// <summary>
    /// Basic interface for implementing a CloudFormation object graph visitor. The visitor provides a context implementation.
    /// A visitor context must as minimum implement the <see cref="ITemplateObjectVisitorContext{TContext}"/> which provides basic traversal methods.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface ITemplateObjectVisitor<in TContext>
        where TContext : ITemplateObjectVisitorContext<TContext>
    {
        /// <summary>
        /// <para>
        /// Gets a reference to the current parsed template.
        /// </para>
        /// <para>
        /// This is needed when visiting child objects of a <see cref="IBranchableIntrinsic"/> such that the appropriate
        /// sub-properties of the intrinsic are visited according to the prevailing conditions.
        /// </para>
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        ITemplate Template { get; }

        /// <summary>
        /// Dispatches the Accept calls added as extension methods to the types of objects encountered in the deserialized YAML object graph.
        /// </summary>
        /// <param name="objectInGraph">The object.</param>
        /// <param name="context">The context.</param>
        void DoAccept(object objectInGraph, TContext context);
    }
}