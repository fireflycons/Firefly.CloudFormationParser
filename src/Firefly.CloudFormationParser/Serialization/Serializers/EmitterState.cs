﻿namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    internal enum EmitterState
    {
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
    }
}