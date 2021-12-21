namespace Firefly.CloudFormationParser.Intrinsics
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface that represents a CloudFormation Intrinsic function.
    /// </summary>
    public interface IIntrinsic : IEnumerable<object>
    {
        /// <summary>
        /// Gets or sets the extra data.
        /// </summary>
        /// <value>
        /// The extra data that may be stored against an intrinsic instance by client applications.
        /// </value>
        object? ExtraData { get; set; }

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
        /// Gets the intrinsic type for this intrinsic.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        IntrinsicType Type { get; }

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
    }
}