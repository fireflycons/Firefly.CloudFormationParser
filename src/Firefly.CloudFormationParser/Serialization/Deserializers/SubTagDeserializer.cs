namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    using Scalar = YamlDotNet.Core.Events.Scalar;

    /// <summary>
    /// Deserializer for !Sub, which may be scalar or a binary array.
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.INodeDeserializer" />
    internal class SubTagDeserializer : AbstractTagDeserializer
    {
        /// <summary>
        /// Deserializes a !Sub tag.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="nestedObjectDeserializer">The nested object deserializer.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the tag was parsed; else <c>false</c></returns>
        public override bool Deserialize(
            IParser parser,
            Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer,
            out object? value)
        {
            value = null;

            if (expectedType != typeof(SubIntrinsic))
            {
                return false;
            }

            var tag = new SubIntrinsic();
            var haveScalar = parser.Accept<Scalar>(out var scalar);
            var haveSequence = parser.Accept<SequenceStart>(out _);

            if (!(haveScalar || haveSequence))
            {
                return false;
            }

            if (haveScalar && !scalar!.Tag.IsEmpty)
            {
                if (scalar.Tag.Value == tag.TagName)
                {
                    // Short form
                    tag.SetValue(new[] { scalar.Value });
                    value = tag;

                    // Move off the tag we just read
                    parser.MoveNext();
                    return true;
                }

                // Long form, but with a short form intrinsic as the value.
                var nestedTag = TagRepository.GetTagByName(scalar.Tag.Value);
                tag.SetValue(this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, nestedTag.GetType()));
                value = tag;
                return true;
            }

            if (haveSequence)
            {
                tag.SetValue(this.SafeNestedObjectDeserializer(ParsingEventBuffer.FromNestedSequence(parser), nestedObjectDeserializer, typeof(List<object>)));
                value = tag;
                return true;
            }


            // Long form, with a literal as the value.
            tag.SetValue(this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(object)));
            value = tag;
            return true;
        }
    }
}