namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Utils;

    /// <summary>
    /// Empty implementation of the interface with overridable methods.
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.TemplateObjects.ITemplateObjectVisitor" />
    public abstract class TemplateObjectVisitor : ITemplateObjectVisitor
    {
        /// <inheritdoc />
        public virtual void AfterVisitList<T>(ITemplateObject templateObject, PropertyPath path, IList<T> item)
        {
        }

        /// <inheritdoc />
        public virtual void AfterVisitObject<T>(
            ITemplateObject templateObject,
            PropertyPath path,
            IDictionary<T, object> item)
        {
        }

        /// <inheritdoc />
        public virtual void BeforeVisitList<T>(ITemplateObject templateObject, PropertyPath path, IList<T> item)
        {
        }

        /// <inheritdoc />
        public virtual void BeforeVisitObject<T>(
            ITemplateObject templateObject,
            PropertyPath path,
            IDictionary<T, object> item)
        {
        }

        /// <inheritdoc />
        public virtual bool VisitIntrinsic(ITemplateObject templateObject, PropertyPath path, IIntrinsic intrinsic)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual void VisitListItem(ITemplateObject templateObject, PropertyPath path, object item)
        {
        }

        /// <inheritdoc />
        public virtual void VisitProperty<T>(
            ITemplateObject templateObject,
            PropertyPath path,
            KeyValuePair<T, object> item)
        {
        }
    }
}