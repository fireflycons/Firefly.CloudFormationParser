namespace Firefly.CloudFormationParser.Intrinsics.Abstractions
{
    /// <summary>
    /// Marker interface for intrinsics that may reference other objects in the template
    /// </summary>
    public interface IReferenceIntrinsic : IIntrinsic
    {
        /// <summary>
        /// Gets the name of the referenced object, e.g. parameter name or resource.property (GetAtt)
        /// </summary>
        /// <value>
        /// The referenced object.
        /// </value>
        string ReferencedObject(ITemplate template);
    }
}