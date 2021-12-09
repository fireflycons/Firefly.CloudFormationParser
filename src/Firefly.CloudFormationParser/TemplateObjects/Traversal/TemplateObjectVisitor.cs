namespace Firefly.CloudFormationParser.TemplateObjects.Traversal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.TemplateObjects.Traversal.AcceptExtensions;
    using Firefly.CloudFormationParser.Utils;

    /// <summary>
    /// An object visitor
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="ITemplateObjectVisitor{TContext}" />
    public abstract class TemplateObjectVisitor<TContext> : ITemplateObjectVisitor<TContext>
        where TContext : ITemplateObjectVisitorContext<TContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateObjectVisitor{TContext}"/> class.
        /// </summary>
        /// <param name="template">The parsed CloudFormation template.</param>
        protected TemplateObjectVisitor(ITemplate template)
        {
            this.Template = template;
        }

        /// <inheritdoc />
        public void DoAccept(object objectInGraph, TContext context)
        {
            switch (objectInGraph)
            {
                case IIntrinsic intrinsic:

                    this.Visit(intrinsic, context);
                    break;

                case IDictionary<string, object> dict:

                    this.Visit(dict, context);
                    break;

                case IDictionary<object, object> dict:

                    this.Visit(dict, context);
                    break;

                case IList<string> list:

                    this.Visit(list, context);
                    break;

                case IList<object> list:

                    this.Visit(list, context);
                    break;

                case KeyValuePair<string, object> kvp:

                    this.Visit(kvp, context);
                    break;

                case KeyValuePair<object, object> kvp:

                    this.Visit(kvp, context);
                    break;

                case string str:

                    this.Visit(str, context);
                    break;

                case int i:

                    this.Visit(i, context);
                    break;

                case double d:

                    this.Visit(d, context);
                    break;

                case bool b:

                    this.Visit(b, context);
                    break;

                default:

                    throw new InvalidOperationException(
                        $"Object of type {objectInGraph.GetType().FullName} cannot be visited by {this.GetType().FullName}");
            }
        }

        /// <inheritdoc />
        public ITemplate Template { get; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public PropertyPath Path { get; } = new PropertyPath();

        /// <summary>
        /// Visits the specified dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key. This should be either string or object(string).</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="context">The context.</param>
        protected virtual void Visit<TKey>(IDictionary<TKey, object> dict, TContext context)
        {
            foreach (var kvp in dict)
            {
                this.Path.Push(kvp.Key!.ToString());
                this.Visit(kvp, context);
                this.Path.Pop();
            }
        }

        /// <summary>
        /// Visits the specified list.
        /// </summary>
        /// <typeparam name="TItem">The type of the list item. This should be dictionary, list, intrinsic or any acceptable value type for CloudFormation.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="context">The context.</param>
        protected virtual void Visit<TItem>(IList<TItem> list, TContext context)
        {
            foreach (var (item, index) in list.Where(item => item != null).WithIndex())
            {
                this.Path.Push(index.ToString());
                this.ItemAccept(item, context.Next(index));
                this.Path.Pop();
            }
        }

        /// <summary>
        /// Visits the specified intrinsic and dispatches to visit handler for each distinct subclass of <see cref="IIntrinsic"/>.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="context">The context.</param>
        protected virtual void Visit(IIntrinsic intrinsic, TContext context)
        {
            this.Path.Push(intrinsic.TagName);

            switch (intrinsic)
            {
                case IBranchableIntrinsic branchableIntrinsic:

                    this.VisitBranchableIntrinsic(branchableIntrinsic, context);
                    break;

                case AbstractArrayIntrinsic arrayIntrinsic:

                    this.VisitAbstractArrayIntrinsic(arrayIntrinsic, context);
                    break;

                case AbstractScalarIntrinsic scalarIntrinsic:

                    this.VisitAbstractScalarIntrinsic(scalarIntrinsic, context);
                    break;

                case SubIntrinsic subIntrinsic:

                    this.VisitSubIntrinsic(subIntrinsic, context);
                    break;

                default:

                    // Should not get here unless intrinsic inheritance gets refactored
                    throw new InvalidOperationException(
                        $"Cannot visit intrinsic of type {intrinsic.GetType().FullName}");
            }

            this.Path.Pop();
        }

        /// <summary>
        /// Visits any derivative of <see cref="AbstractArrayIntrinsic"/>.
        /// Each item in the <c>Items</c> array of the intrinsic is visited.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="context">The context.</param>
        protected virtual void VisitAbstractArrayIntrinsic(AbstractArrayIntrinsic intrinsic, TContext context)
        {
            intrinsic.Items.Accept(this, context);
        }

        /// <summary>
        /// Visits any derivative of <see cref="AbstractScalarIntrinsic"/>.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="context">The context.</param>
        protected virtual void VisitAbstractScalarIntrinsic(AbstractScalarIntrinsic intrinsic, TContext context)
        {
        }

        /// <summary>
        /// Visits <c>!Sub</c> intrinsic which is somewhat a special case,
        /// having both implicit and possibly additional explicit references as a dictionary.
        /// Each of these are visited.
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="context">The context.</param>
        protected virtual void VisitSubIntrinsic(SubIntrinsic intrinsic, TContext context)
        {
            // !Sub is a special case, having both implicit and possibly additional explicit references as a dictionary
            intrinsic.ImplicitReferences.Cast<object>().ToList().Accept(this, context);
            intrinsic.Substitutions.Accept(this, context);
        }

        /// <summary>
        /// <para>
        /// Visits any derivatives of <see cref="IBranchableIntrinsic"/>, i.e. <c>!If</c> and <c>!Select</c>
        /// where only one branch of the intrinsic should be walked base don prevailing conditions.
        /// </para>
        /// <para>
        /// Whilst both of these intrinsic functions are also inherited from <see cref="AbstractScalarIntrinsic"/>,
        /// this visitor takes precedence for these functions.
        /// </para>
        /// </summary>
        /// <param name="intrinsic">The intrinsic.</param>
        /// <param name="context">The context.</param>
        protected virtual void VisitBranchableIntrinsic(IBranchableIntrinsic intrinsic, TContext context)
        {
            this.DoAccept(intrinsic.GetBranch(this.Template), context);
        }

        /// <summary>
        /// Visits the specified property, i.e. <see cref="KeyValuePair{TKey,TValue}"/> of a dictionary object.
        /// </summary>
        /// <typeparam name="TKey">The type of the key. This should be either string or object(string).</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="context">The context.</param>
        protected virtual void Visit<TKey>(KeyValuePair<TKey, object> property, TContext context)
        {
            this.ItemAccept(property.Value, context.Next(property.Key!.ToString()));
        }

        /// <summary>
        /// Visits the specified string value.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        /// <param name="context">The context.</param>
        protected virtual void Visit(string stringValue, TContext context)
        {
        }

        /// <summary>
        /// Visits the specified integer value.
        /// </summary>
        /// <param name="integerValue">The integer value.</param>
        /// <param name="context">The context.</param>
        protected virtual void Visit(int integerValue, TContext context)
        {
        }

        /// <summary>
        /// Visits the specified boolean value.
        /// </summary>
        /// <param name="booleanValue">if set to <c>true</c> [boolean value].</param>
        /// <param name="context">The context.</param>
        protected virtual void Visit(bool booleanValue, TContext context)
        {
        }

        /// <summary>
        /// Visits the specified double value.
        /// </summary>
        /// <param name="doubleValue">The double value.</param>
        /// <param name="context">The context.</param>
        protected virtual void Visit(double doubleValue, TContext context)
        {
        }

        private void ItemAccept<TItem>(TItem item, TContext context)
        {
            switch (item)
            {
                case IBranchableIntrinsic intrinsic:

                    intrinsic.Accept(this, context);
                    break;

                case AbstractArrayIntrinsic intrinsic:

                    intrinsic.Accept(this, context);
                    break;

                case AbstractScalarIntrinsic intrinsic:

                    intrinsic.Accept(this, context);
                    break;

                case SubIntrinsic intrinsic:

                    intrinsic.Accept(this, context);
                    break;

                case IDictionary<string, object> dict:

                    dict.Accept(this, context);
                    break;

                case IDictionary<object, object> dict:

                    dict.Accept(this, context);
                    break;

                case IList<string> nestedList:

                    nestedList.Accept(this, context);
                    break;

                case IList<object> nestedList:

                    nestedList.Accept(this, context);
                    break;

                case KeyValuePair<string, object> kvp:

                    kvp.Accept(this, context);
                    break;

                case KeyValuePair<object, object> kvp:

                    kvp.Accept(this, context);
                    break;

                case string str:

                    str.Accept(this, context);
                    break;

                case int i:

                    i.Accept(this, context);
                    break;

                case double d:

                    d.Accept(this, context);
                    break;

                case bool b:

                    b.Accept(this, context);
                    break;

                case null:

                    break;

                default:

                    throw new InvalidOperationException(
                        $"Object of type {item.GetType().FullName} cannot be visited by {this.GetType().FullName}");
            }
        }
    }
}