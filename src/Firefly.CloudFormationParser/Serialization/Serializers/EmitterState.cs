namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    /// <summary>
    /// Copy of the one in <c>YamlDotNet</c>
    /// </summary>
    internal enum EmitterState
    {
        // ReSharper disable StyleCop.SA1602
        // ReSharper disable UnusedMember.Global - All members must be present to preserve integer values
        StreamStart,

        StreamEnd,

        FirstDocumentStart,

        DocumentStart,

        DocumentContent,

        DocumentEnd,

        FlowSequenceFirstItem,

        FlowSequenceItem,

        FlowMappingFirstKey,

        FlowMappingKey,

        FlowMappingSimpleValue,

        FlowMappingValue,

        BlockSequenceFirstItem,

        BlockSequenceItem,

        BlockMappingFirstKey,

        BlockMappingKey,

        BlockMappingSimpleValue,

        BlockMappingValue
        // ReSharper restore UnusedMember.Global
        // ReSharper restore StyleCop.SA1602
    }
}