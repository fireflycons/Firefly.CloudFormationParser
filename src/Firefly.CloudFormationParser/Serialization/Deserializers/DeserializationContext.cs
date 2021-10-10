namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    /// <summary>
    /// Indicates to the node type resolver which section of the template is currently being parsed
    /// such that a decision on how to deserialize certain keys (e.g. <c>Condition:</c>) can be made.
    /// </summary>
    public enum DeserializationContext
    {
        /// <summary>
        /// No context yet
        /// </summary>
        None,

        /// <summary>
        /// In <c>Metadata</c> (top level) block.
        /// </summary>
        Metadata,

        /// <summary>
        /// In <c>Parameters</c> block.
        /// </summary>
        Parameters,

        /// <summary>
        /// In <c>Globals</c> block.
        /// </summary>
        Globals,

        /// <summary>
        /// In <c>Conditions</c> block.
        /// </summary>
        Conditions,

        /// <summary>
        /// In <c>Mappings</c> block.
        /// </summary>
        Mappings,

        /// <summary>
        /// In <c>Rules</c> block.
        /// </summary>
        Rules,

        /// <summary>
        /// In <c>Resources</c> block.
        /// </summary>
        Resources,

        /// <summary>
        /// In <c>Outputs</c> block.
        /// </summary>
        Outputs
    }
}