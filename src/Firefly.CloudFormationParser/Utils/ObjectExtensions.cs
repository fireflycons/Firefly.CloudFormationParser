namespace Firefly.CloudFormationParser.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    using Firefly.CloudFormationParser.Intrinsics;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Extensions for <see cref="object"/>
    /// </summary>
    internal static class ObjectExtensions
    {
        /// <summary>
        /// The clone method
        /// </summary>
        // ReSharper disable once AssignNullToNotNullAttribute
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod(
            "MemberwiseClone",
            BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Deep copy an object graph, evaluating any intrinsics found within.
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <param name="template">The template.</param>
        /// <returns>Copy of the original object with intrinsics replaced by their evaluations.</returns>
        public static object? CopyAndEvaluateIntrinsics(this object? originalObject, ITemplate? template)
        {
            return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()), template);
        }

        /// <summary>
        /// Walk object graph finding intrinsics and evaluating their references.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="template">The template.</param>
        /// <returns>List of references.</returns>
        public static List<string> GetNestedReferences(this object obj, ITemplate template)
        {
            var objectDict = new Dictionary<object, object>(new ReferenceEqualityComparer());

            InternalCopy(obj, objectDict, template);

            var references = new List<string>();

            foreach (var intrinsic in objectDict.Keys.Where(k => k is IIntrinsic).Cast<IIntrinsic>())
            {
                references.AddRange(intrinsic.GetReferencedObjects(template));
            }

            return references;
        }

        /// <summary>
        /// Deep copy an object graph
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <returns>Copy of the original object.</returns>
        public static object? Copy(this object? originalObject)
        {
            return originalObject.CopyAndEvaluateIntrinsics(null);
        }

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

            return deserializer.Deserialize<Dictionary<string, object>>(yaml);
        }

        /// <summary>
        /// Recursively copies the fields of the given object.
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <param name="visited">Dictionary of objects already processed.</param>
        /// <param name="cloneObject">The clone object.</param>
        /// <param name="typeToReflect">The type to reflect.</param>
        /// <param name="template">The template.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="filter">The filter.</param>
        private static void CopyFields(
            object originalObject,
            IDictionary<object, object> visited,
            object cloneObject,
            IReflect typeToReflect,
            ITemplate? template,
            BindingFlags bindingFlags =
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy,
            Func<FieldInfo, bool>? filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false)
                {
                    continue;
                }

                if (IsPrimitive(fieldInfo.FieldType))
                {
                    continue;
                }

                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited, template);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        /// <summary>
        /// Recursive method to copy objects.
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <param name="visited">Dictionary of objects already processed.</param>
        /// <param name="template">The template.</param>
        /// <returns>Copied object at current point in the object graph.</returns>
        private static object? InternalCopy(object? originalObject, IDictionary<object, object> visited, ITemplate? template)
        {
            if (originalObject == null)
            {
                return null;
            }

            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect))
            {
                return originalObject;
            }

            if (visited.ContainsKey(originalObject))
            {
                return visited[originalObject];
            }

            if (typeof(Delegate).IsAssignableFrom(typeToReflect))
            {
                return null;
            }

            object? cloneObject;

            if (template != null && originalObject is IIntrinsic intrinsic)
            {
                cloneObject = intrinsic.Evaluate(template);
                visited.Add(originalObject, cloneObject);
                return cloneObject;
            }

            cloneObject = CloneMethod.Invoke(originalObject, null);

            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach(
                        (array, indices) => array.SetValue(
                            InternalCopy(clonedArray.GetValue(indices), visited, template),
                            indices));
                }
            }

            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect, template);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect, template);
            return cloneObject;
        }

        /// <summary>
        /// Determines whether this instance is primitive (a built in primitive, string or value type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is primitive; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }

            return type.IsValueType & type.IsPrimitive;
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

        /// <summary>
        /// Recursively copy base type private fields.
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <param name="visited">Dictionary of objects already processed.</param>
        /// <param name="cloneObject">The clone object.</param>
        /// <param name="typeToReflect">The type to reflect.</param>
        /// <param name="template">The template.</param>
        private static void RecursiveCopyBaseTypePrivateFields(
            object originalObject,
            IDictionary<object, object> visited,
            object cloneObject,
            Type typeToReflect,
            ITemplate? template)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType, template);
                CopyFields(
                    originalObject,
                    visited,
                    cloneObject,
                    typeToReflect.BaseType,
                    template,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    info => info.IsPrivate);
            }
        }

        /// <summary>
        /// Equality comparer that compares whether two references point to the same object.
        /// </summary>
        /// <seealso cref="object" />
        private class ReferenceEqualityComparer : EqualityComparer<object>
        {
            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="x">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <param name="y">The y.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public override bool Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode(object? obj)
            {
                return obj == null ? 0 : obj.GetHashCode();
            }
        }
    }
}