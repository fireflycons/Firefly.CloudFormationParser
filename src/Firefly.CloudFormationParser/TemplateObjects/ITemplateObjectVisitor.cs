namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Utils;

    /// <summary>
    /// Visitor interface for template objects
    /// </summary>
    public interface ITemplateObjectVisitor
    {
        /// <summary>
        /// Called when an object (dictionary) is about to be visited.
        /// </summary>
        /// <typeparam name="T">Type of item in list. Should be string or object.</typeparam>
        /// <param name="templateObject">The template object being visited</param>
        /// <param name="path">The current path in the property walk.</param>
        /// <param name="item">The dictionary being visited.</param>
        void BeforeVisitObject<T>(ITemplateObject templateObject, PropertyPath path, IDictionary<T, object> item);

        /// <summary>
        /// Called when a dictionary item is visited.
        /// </summary>
        /// <typeparam name="T">Type of item in list. Should be string or object.</typeparam>
        /// <param name="templateObject">The template object.</param>
        /// <param name="path">The path.</param>
        /// <param name="item">The dictionary item being visited.</param>
        void VisitProperty<T>(ITemplateObject templateObject, PropertyPath path, KeyValuePair<T, object> item);

        /// <summary>
        /// Called at the end of the enumeration of a on object (dictionary( item ).
        /// </summary>
        /// <typeparam name="T">Type of item in list. Should be string or object.</typeparam>
        /// <param name="templateObject">The template object.</param>
        /// <param name="path">The path.</param>
        /// <param name="item">The dictionary item being visited.</param>
        void AfterVisitObject<T>(ITemplateObject templateObject, PropertyPath path, IDictionary<T, object> item);

        /// <summary>
        /// Called when a list is about to be visited (before list traversal).
        /// </summary>
        /// <typeparam name="T">Type of item in list. Should be string or object.</typeparam>
        /// <param name="templateObject">The template object being visited</param>
        /// <param name="path">The current path in the property walk.</param>
        /// <param name="item">The list being visited.</param>
        void BeforeVisitList<T>(ITemplateObject templateObject, PropertyPath path, IList<T> item);

        /// <summary>
        /// Called at the end of the enumeration of a list.
        /// </summary>
        /// <typeparam name="T">Type of item in list. Should be string or object.</typeparam>
        /// <param name="templateObject">The template object being visited</param>
        /// <param name="path">The current path in the property walk.</param>
        /// <param name="item">The list being visited.</param>
        void AfterVisitList<T>(ITemplateObject templateObject, PropertyPath path, IList<T> item);

        /// <summary>
        /// Called when a scalar list item is being visited.
        /// </summary>
        /// <param name="templateObject">The template object being visited</param>
        /// <param name="path">The current path in the property walk.</param>
        /// <param name="item">The list item being visited.</param>
        void VisitListItem(ITemplateObject templateObject, PropertyPath path, object item);

        /// <summary>
        /// Called when an intrinsic is being visited.
        /// </summary>
        /// <param name="templateObject">The template object being visited</param>
        /// <param name="path">The current path in the property walk.</param>
        /// <param name="intrinsic">The intrinsic being visited.</param>
        void VisitIntrinsic(ITemplateObject templateObject, PropertyPath path, IIntrinsic intrinsic);
    }
}