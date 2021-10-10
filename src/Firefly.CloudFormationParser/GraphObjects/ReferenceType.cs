namespace Firefly.CloudFormationParser.GraphObjects
{
    /// <summary>
    /// Describes the relationship between two graph vertices.
    /// </summary>
    public enum ReferenceType
    {
        /// <summary>
        /// Direct reference via <c>!Ref</c> to a resource
        /// </summary>
        DirectReference,

        /// <summary>
        /// Direct reference via <c>!Ref</c> to a parameter
        /// </summary>
        ParameterReference,

        /// <summary>
        /// Attribute reference via <c>!GetAtt</c>
        /// </summary>
        AttributeReference,

        /// <summary>
        /// Explicit inter-resource reference
        /// </summary>
        DependsOn
    }
}