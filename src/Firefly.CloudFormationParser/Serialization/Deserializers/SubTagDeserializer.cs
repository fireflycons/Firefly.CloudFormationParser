namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Functions;

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

            var haveScalar = parser.Accept<Scalar>(out var scalar);
            var haveSequence = parser.Accept<SequenceStart>(out _);

            if (!(haveScalar || haveSequence))
            {
                return false;
            }

            if (haveScalar && !scalar!.Tag.IsEmpty)
            {
                if (scalar.Tag.Value == SubIntrinsic.Tag)
                {
                    // Short form
                    value = new SubIntrinsic(scalar.Value);

                    // Move off the tag we just read
                    parser.MoveNext();
                    return true;
                }

                // Long form, but with a short form intrinsic as the value.
                // Shouldn't get here.
                throw new YamlException(
                    parser.Current!.Start,
                    parser.Current.End,
                    $"{SubIntrinsic.Tag}: Unexpected intrinsic as substitution expression.");
            }

            if (haveSequence)
            {
                var values = (List<object>)this.SafeNestedObjectDeserializer(
                    ParsingEventBuffer.FromNestedSequence(parser),
                    nestedObjectDeserializer,
                    typeof(List<object>));

                this.ValidateValues(parser, SubIntrinsic.Tag, 1, 2, values);
                value = new SubIntrinsic(
                    values[0].ToString(),
                    ((Dictionary<object, object>)values[1]).ToDictionary(
                        kv => kv.Key.ToString(),
                        kv => this.UnpackIntrinsic(kv.Value)));

                return true;
            }


            // Long form, with a string literal as the value.
            value = new SubIntrinsic(
                (string)this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(string)));
            return true;
        }
    }
}