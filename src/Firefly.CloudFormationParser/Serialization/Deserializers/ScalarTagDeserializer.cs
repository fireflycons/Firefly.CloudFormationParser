namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    /// <summary>
    /// Deserializer for tags that have a scalar value, or in long form possibly a mapping that contains another intrinsic
    /// </summary>
    /// <typeparam name="T">Tag type to deserialize</typeparam>
    /// <seealso cref="YamlDotNet.Serialization.INodeDeserializer" />
    internal class ScalarTagDeserializer<T> : AbstractTagDeserializer
        where T : IIntrinsic, new()
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

            var haveScalar = parser.Accept<Scalar>(out var scalar);
            var haveMapping = parser.Accept<MappingStart>(out _);

            if (!(haveMapping || haveScalar))
            {
                return false;
            }

            var tag = new T();

            if (expectedType != typeof(T))
            {
                return false;
            }                                                        

            if (scalar is { Tag: { IsEmpty: false } })
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

            // Long form, with a literal as the value.
            tag.SetValue(this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(object)));
            value = tag;
            return true;
        }
    }
}
