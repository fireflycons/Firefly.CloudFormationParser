namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    /// <summary>
    /// Deserializer for tags that have exactly two array items
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.INodeDeserializer" />
    internal class GetAttTagDeserializer : AbstractTagDeserializer
    {
        /// <summary>
        /// Deserializes the tag identified by the type parameter.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="nestedObjectDeserializer">The nested object deserializer.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns><c>true</c> if the tag was parsed; else <c>false</c></returns>
        public override bool Deserialize(
            IParser parser,
            Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer,
            out object? value)
        {
            value = null;

            if (expectedType != typeof(GetAttIntrinsic))
            {
                return false;
            }

            ParsingEvent @event;

            if (parser.Accept<Scalar>(out var scalar))
            {
                // Scalar version
                var props = scalar.Value.Split('.').Cast<object>().ToList();

                if (props.Count < 2)
                {
                    @event = parser.Current!;

                    if (@event == null)
                    {
                        throw new YamlException("Expected parsing event, got nothing");
                    }

                    throw new YamlException(@event.Start, @event.End, "Fn::GetAtt: Expected LogicalID.Attribute.");
                }

                var logicalId = props.First().ToString();
                var attribute = string.Join(".", props.Skip(1).Cast<string>());

                parser.MoveNext();
                value = new GetAttIntrinsic(logicalId, attribute);
                return true;
            }

            if (!parser.Accept<SequenceStart>(out _))
            {
                return false;
            }

            // Array version
            var values = new List<object>();

            @event = parser.Current!;
            parser.MoveNext();
            
            while (!parser.TryConsume<SequenceEnd>(out _))
            {
                values.Add(this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(object)));
            }

            if (values.Count != 2)
            {
                throw new YamlException(
                    @event.Start,
                    @event.End,
                    $"{GetAttIntrinsic.Tag}: Incorrect number of values {values.Count}. Expected 2.");
            }
            
            value = new GetAttIntrinsic(values[0].ToString(), values[1]);
            return true;
        }
    }
}