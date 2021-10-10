namespace Firefly.CloudFormationParser.Intrinsics
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface that represents a CloudFormation Intrinsic function.
    /// </summary>
    public interface IIntrinsic : IEnumerable<object>
    {
        /// <summary>
        /// Gets the long name of the intrinsic.
        /// </summary>
        /// <value>
        /// The long name.
        /// </value>
        string LongName { get; }

        /// <summary>
        /// Gets the YAML tag name.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        string TagName { get; }

        /// <summary>
        /// Evaluates the result of the intrinsic function.
        /// </summary>
        /// <param name="template">Reference to the template being processed</param>
        /// <returns>The result.</returns>
        object Evaluate(ITemplate template);

        /// <summary>
        /// Gets a list of referenced resources and/or parameters in this expression
        /// </summary>
        /// <param name="template">Reference to the template being processed</param>
        /// <returns>List of references</returns>
        IEnumerable<string> GetReferencedObjects(ITemplate template);

        /// <summary>
        /// <para>
        /// Sets the values for the intrinsic.
        /// </para>
        /// <para>
        /// This is a list of objects, whose length depends on the number of properties (usually list elements) supported by the intrinsic.
        /// Each element depending on the intrinsic, may be a scalar value, another intrinsic or another list of items e.g. for <c>!Join</c> or <c>!Select</c> 
        /// </para>
        /// </summary>
        /// <param name="values">Values to assign to the intrinsic</param>
        void SetValue(IEnumerable<object> values);

        /// <summary>
        /// <para>
        /// Sets the values for the intrinsic.
        /// </para>
        /// <para>
        /// This is a single scalar or intrinsic object or a list of objects. This is a convenience method which wraps a scalar and then calls <see cref="SetValue(System.Collections.Generic.IEnumerable{object})"/>
        /// </para>
        /// </summary>
        /// <param name="value">Value to assign to the intrinsic</param>
        void SetValue(object value);
    }
}