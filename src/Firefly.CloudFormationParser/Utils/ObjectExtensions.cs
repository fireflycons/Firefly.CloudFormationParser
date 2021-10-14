namespace Firefly.CloudFormationParser.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Numerics;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Extensions for <see cref="object"/>
    /// </summary>
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Converts the given object to template resource schema, i.e. a list/dictionary graph
        /// </summary>
        /// <param name="self">The object.</param>
        /// <returns>An object that can be inserted to the resource schema</returns>
        public static object? ToResourceSchema(this object? self)
        {
            if (self == null)
            {
                return null;
            }

            if (IsScalar(self))
            {
                return self;
            }

            var serializer = new SerializerBuilder().Build();
            var deserializer = new DeserializerBuilder().Build();

            var yaml = serializer.Serialize(self);

            if (self is IList)
            {
                return deserializer.Deserialize<List<object>>(yaml);
            }
            else
            {
                return deserializer.Deserialize<Dictionary<string, object>>(yaml);
            }
        }

        /// <summary>
        /// Determines whether the specified value is scalar.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is scalar; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsScalar(object value)
        {
            return value is byte || value is short || value is int || value is long || value is sbyte || value is ushort
                   || value is uint || value is ulong || value is BigInteger || value is decimal || value is double
                   || value is float || value is string;
        }
    }
}