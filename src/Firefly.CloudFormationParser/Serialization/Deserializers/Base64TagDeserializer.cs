﻿namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Functions;

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
            var tag = new Base64Intrinsic();

            if (parser.Accept<Scalar>(out var scalar))
            {
                // Next event is scalar. Could be one of
                // - A literal value
                // - An intrinsic function tag with a scalar value
                if (scalar.Tag.IsEmpty)
                {
                    tag.SetValue(new[] { scalar.Value });
                    parser.MoveNext();
                }
                else
                {
                    // Tagged scalar
                    if (scalar.Value == string.Empty || (!scalar.Tag.IsEmpty && scalar.Tag.Value == tag.TagName))
                    {
                        nestedObject = this.SafeNestedObjectDeserializer(
                            ParsingEventBuffer.FromNestedScalar(parser),
                            nestedObjectDeserializer,
                            typeof(object));

                        // Kludgy
                        if (nestedObject.GetType() == typeof(NullNestedObject))
                        {
                            if (tag.GetType() == typeof(GetAZsIntrinsic))
                            {
                                tag.SetValue(string.Empty);
                            }
                            else
                            {
                                throw new YamlException(
                                    parser.Current!.Start,
                                    parser.Current!.End,
                                    $"{tag.LongName}: Null value unexpected.");
                            }
                        }
                        else
                        {
                            tag.SetValue(nestedObject);
                        }
                    }
                    else
                    {
                        tag.SetValue(
                            this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(IIntrinsic)));
                    }
                }

                value = tag;
                return true;
            }

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
                    tag.UseLongForm = true;
                }

                tag.SetValue(new[] { nestedObject });
                value = tag;
                return true;
            }

            if (parser.Accept<SequenceStart>(out var seq))
            {
                // Found a tagged sequence, like e.g. !Join
                nestedObject = this.SafeNestedObjectDeserializer(parser, nestedObjectDeserializer, typeof(object));

                if (nestedObject is AbstractIntrinsic && !seq.Tag.IsEmpty)
                {
                    // Parent must be an intrinsic and must be emitted long form
                    tag.UseLongForm = true;
                }

                tag.SetValue(new[] { nestedObject });
                value = tag;
                return true;
            }

            return false;
        }
    }
}