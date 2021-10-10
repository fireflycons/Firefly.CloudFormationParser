namespace Firefly.CloudFormationParser.Intrinsics.Abstractions
{
    /// <summary>
    /// Marker interface for intrinsics that may reference other objects in the template
    /// </summary>
    internal interface IReferenceIntrinsic : IIntrinsic
    {
    }
}