namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.TemplateObjects;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Abstract base class for intrinsic tag node deserializer
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.INodeDeserializer" />
    internal abstract class AbstractTagDeserializer : INodeDeserializer
    {
        /// <summary>
        /// Attempts to deserialize an intrinsic function from the current parser position.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="nestedObjectDeserializer">The nested object deserializer.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if an intrinsic function was deserialized, else <c>false</c>.</returns>
        public abstract bool Deserialize(
            IParser parser,
            Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer,
            out object? value);

        /// <summary>
        /// Calls the nested object deserializer and addresses:
        /// <list type="bullet">
        /// <item>
        /// <description>If a dictionary is returned where value is an <see cref="IIntrinsic"/> and the key is the long name of that intrinsic, then return the intrinsic.</description>
        /// </item>
        /// <item>
        /// <description>If the value is null (as in <c>!GetAtt</c> with no argument, return special value <see cref="NullNestedObject"/></description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="nestedObjectDeserializer">The nested object deserializer.</param>
        /// <param name="requiredType">Type of the required.</param>
        /// <returns>Deserialized nested object</returns>
        /// <exception cref="YamlDotNet.Core.YamlException">Null value not expected.</exception>
        protected object SafeNestedObjectDeserializer(
            IParser parser,
            Func<IParser, Type, object?> nestedObjectDeserializer,
            Type requiredType)
        {
            // Note that nestedObjectDeserializer will always MoveNext off whatever it deserializes
            // Callers of this method must not therefore call MoveNext
            var obj = nestedObjectDeserializer(parser, requiredType);

            return obj != null ? TryConvertToIntrinsic(obj) : new NullNestedObject();
        }

        /// <summary>
        /// Performs some validation on the number of values being set on this intrinsic.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="intrinsicName">Name of intrinsic being deserialized.</param>
        /// <param name="minValues">The minimum values.</param>
        /// <param name="maxValues">The maximum values.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="System.ArgumentException">
        /// Number of values being assigned is outside the min and max constraints,
        /// </exception>
        protected void ValidateValues(IParser parser, string intrinsicName, int minValues, int maxValues, IList<object> values)
        {
            var @event = parser.Current;

            if (values.Count < minValues || values.Count > maxValues)
            {
                if (minValues == maxValues)
                {
                    throw new YamlException(
                        @event!.Start,
                        @event.End,
                        $"{intrinsicName}: Expected {minValues} values. Got {values.Count}.");
                }

                throw new YamlException(
                    @event!.Start,
                    @event.End,
                    $"{intrinsicName}: Expected between {minValues} and {maxValues} values. Got {values.Count}.");
            }
        }

        /// <summary>
        /// Unpack an intrinsic from a long form dictionary entry e.g. <c>{ "Fn::Sub", SubIntrinsic }</c>
        /// </summary>
        /// <param name="value">The value to unpack.</param>
        /// <returns>If <paramref name="value"/> is a long form intrinsic, then the intrinsic else the original object.</returns>
        protected object UnpackIntrinsic(object value)
        {
            if (value is Dictionary<object, object> dict && dict.Any() && dict.First().Value is IIntrinsic intrinsic && dict.First().Key.ToString() == intrinsic.LongName)
            {
                return intrinsic;
            }

            return value;
        }

        /// <summary>
        /// <para>
        /// Check if the parsed object is a dictionary with one entry, and that entry is e.g. <c>{ "Fn::GetAtt", GetAttIntrinsic }</c>.
        /// If so, return the intrinsic; else the original object.
        /// </para>
        /// <para>
        /// Note that when parsing JSON or a long-form in YAML this won't be hit,
        /// and we need to rely on the second pass in <see cref="Template.PostProcess"/>
        /// </para>
        /// </summary>
        /// <param name="parsedObject">The parsed object.</param>
        /// <returns>Intrinsic, or original object if the condition is not met.</returns>
        private static object TryConvertToIntrinsic(object parsedObject)
        {
            if (parsedObject is IDictionary<object, object> { Count: 1 } dict)
            {
                var kv = dict.First();

                if (kv.Value is IIntrinsic intrinsic && kv.Key.ToString() == intrinsic.LongName)
                {
                    return intrinsic;
                }
            }

            return parsedObject;
        }

        /// <summary>
        /// Creates an intrinsic instance given the type and value to set.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected static IIntrinsic CreateIntrinsic(Type type, object value)
        {
            return (IIntrinsic)Activator.CreateInstance(type, value);
        }
    }
}
