namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    /// <summary>
    /// Deserializer for intrinsic tags that have array values.
    /// </summary>
    /// <typeparam name="T">Type of tag accepted by this deserializer.</typeparam>
    /// <seealso cref="AbstractTagDeserializer" />
    internal class ArrayTagDeserializer<T> : AbstractTagDeserializer
        where T : AbstractArrayIntrinsic, new()
    {
        /// <summary>
        /// Deserializes the specified tag.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="nestedObjectDeserializer">The nested object deserializer.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the tag was parsed; else <c>false</c></returns>
        /// <exception cref="YamlException">
        /// Incorrect number of values.
        /// </exception>
        public override bool Deserialize(
            IParser parser,
            Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer,
            out object? value)
        {
            value = null;

            if (expectedType != typeof(T))
            {
                return false;
            }

            var tag = new T();

            if (!parser.Accept<SequenceStart>(out _))
            {
                return false;
            }

            var values = new List<object>();
            var @event = parser.Current;

            parser.MoveNext();

            while (!parser.TryConsume<SequenceEnd>(out _))
            {
                values.Add(this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(object)));
            }

            if (values.Count < tag.MinValues || values.Count > tag.MaxValues)
            {
                if (tag.MinValues == tag.MaxValues)
                {
                    throw new YamlException(
                        @event!.Start,
                        @event.End,
                        $"{tag.LongName}: Incorrect number of values {values.Count}. Expected {tag.MaxValues}.");
                }
                else
                {
                    throw new YamlException(
                        @event!.Start,
                        @event.End,
                        $"{tag.LongName}: Incorrect number of values {values.Count}. Expected between {tag.MinValues} and {tag.MaxValues}.");
                }
            }

            tag.SetValue(values);
            value = tag;
            return true;
        }
    }
}