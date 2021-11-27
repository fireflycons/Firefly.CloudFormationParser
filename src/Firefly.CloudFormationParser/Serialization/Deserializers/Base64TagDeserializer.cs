namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    /// <summary>
    /// Deserializer !Base64 tag which requires its own special logic for dealing with nested intrinsics
    /// One or other intrinsic must be in long form for legal YAML 
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.INodeDeserializer" />
    internal class Base64TagDeserializer : AbstractTagDeserializer
    {
        /// <summary>
        /// Deserializes the tag identified by the type parameter.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="nestedObjectDeserializer">The nested object deserializer.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the tag was parsed; else <c>false</c></returns>
        /// <exception cref="YamlDotNet.Core.YamlException">
        /// Expected scalar map key
        /// or
        /// Unsupported function '{key.Value}' for '{tag.TagName}'
        /// </exception>
        public override bool Deserialize(
            IParser parser,
            Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer,
            out object? value)
        {
            value = null;

            if (expectedType != typeof(Base64Intrinsic))
            {
                return false;
            }

            object nestedObject;
            Base64Intrinsic tag;

            if (parser.Accept<Scalar>(out var scalar))
            {
                // Next event is scalar. Could be one of
                // - A literal value
                // - An intrinsic function tag with a scalar value
                if (scalar.Tag.IsEmpty)
                {
                    tag = new Base64Intrinsic(scalar.Value);
                    parser.MoveNext();
                }
                else
                {
                    // Tagged scalar
                    if (scalar.Value == string.Empty || (!scalar.Tag.IsEmpty && scalar.Tag.Value == TagRepository.GetIntrinsicByType(expectedType).TagName))
                    {
                        nestedObject = this.SafeNestedObjectDeserializer(
                            ParsingEventBuffer.FromNestedScalar(parser),
                            nestedObjectDeserializer,
                            typeof(object));

                            tag = new Base64Intrinsic(nestedObject);
                    }
                    else
                    {
                        tag = new Base64Intrinsic(
                            this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(IIntrinsic)));
                    }
                }

                value = tag;
                return true;
            }

            var useLongForm = false;

            if (parser.Accept<MappingStart>(out var mapping))
            {
                // Buffer the map that follows, removing any tag from MappingStart, else we will infinitely recurse.
                nestedObject = this.SafeNestedObjectDeserializer(
                    ParsingEventBuffer.FromNestedMapping(parser),
                    nestedObjectDeserializer,
                    typeof(Dictionary<object, object>));

                if (nestedObject is AbstractIntrinsic && !mapping.Tag.IsEmpty)
                {
                    // Parent must be emitted long form
                    useLongForm = true;
                }

                value = new Base64Intrinsic(nestedObject, useLongForm);
                return true;
            }

            if (parser.Accept<SequenceStart>(out var seq))
            {
                // Found a tagged sequence, like e.g. !Join
                nestedObject = this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(object));

                if (nestedObject is AbstractIntrinsic && !seq.Tag.IsEmpty)
                {
                    // Parent must be an intrinsic and must be emitted long form
                    useLongForm = true;
                }

                value = new Base64Intrinsic(nestedObject, useLongForm);
                return true;
            }

            return false;
        }
    }
}