namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    /// <summary>
    /// Special case for nested object deserialization where the value can be null, e.g. !GetAZs with no argument
    /// </summary>
    internal class NullNestedObject
    {
    }
}