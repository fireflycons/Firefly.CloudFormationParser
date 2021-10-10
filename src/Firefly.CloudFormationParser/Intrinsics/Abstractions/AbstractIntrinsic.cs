namespace Firefly.CloudFormationParser.Intrinsics.Abstractions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization.Deserializers;
    using Firefly.CloudFormationParser.Serialization.Serializers;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Abstract base class for CloudFormation intrinsics
    /// </summary>
    /// <seealso cref="CloudFormationParser.Intrinsics.IIntrinsic" />
    public abstract class AbstractIntrinsic : IIntrinsic
    {
        /// <inheritdoc />
        public abstract string LongName { get; }

        /// <inheritdoc />
        public abstract string TagName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the long form when serializing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if long form should be used; otherwise, <c>false</c>.
        /// </value>
        public bool UseLongForm { get; set; } = false;

        /// <summary>
        /// Gets the emitter trait for this intrinsic.
        /// </summary>
        /// <value>
        /// The emitter trait.
        /// </value>
        internal abstract IEmitterTrait EmitterTrait { get; }

        /// <summary>
        /// Evaluates the result of the intrinsic function.
        /// </summary>
        /// <param name="template">Reference to the template being processed</param>
        /// <returns>
        /// The result.
        /// </returns>
        public abstract object Evaluate(ITemplate template);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<object> GetEnumerator()
        {
            yield return this;
        }

        /// <inheritdoc />
        public abstract IEnumerable<string> GetReferencedObjects(ITemplate template);

        /// <inheritdoc />
        public abstract void SetValue(IEnumerable<object> values);

        /// <inheritdoc />
        public void SetValue(object value)
        {
            if (value is IEnumerable<object> enumerable)
            {
                this.SetValue(enumerable);
            }
            else
            {
                this.SetValue(new[] { value });
            }
        }

        /// <summary>
        /// Writes this intrinsic to a YAML emitter.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer">An <see cref="IValueSerializer"/> with which to write nested objects such as dictionaries.</param>
        public void WriteYaml(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            if (this.UseLongForm)
            {
                this.WriteLongForm(emitter, nestedValueSerializer);
            }
            else
            {
                this.WriteShortForm(emitter, nestedValueSerializer);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the unresolved dictionary properties of the tag so they may be walked to resolve nested tags.
        /// </summary>
        /// <returns>List of tag properties to walk.</returns>
        internal abstract IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties();

        /// <summary>
        /// Called to write the long form of this intrinsic to YAML
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer">An <see cref="IValueSerializer"/> with which to write nested objects such as dictionaries.</param>
        internal abstract void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer);

        /// <summary>
        /// Called to write the short form of this intrinsic to YAML
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer">An <see cref="IValueSerializer"/> with which to write nested objects such as dictionaries.</param>
        internal abstract void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer);

        /// <summary>
        /// <para>
        /// Indicates to <see cref="IntrinsicFunctionNodeTypeResolver"/> and to the post processing phase whether a key matching an intrinsic function name
        /// should actually be deserialized as an intrinsic.
        /// </para>
        /// <para>
        /// This is useful for e.g. policy documents where 'Condition' may occur as a key, but is a policy condition, not an intrinsic condition.
        /// </para>
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if the key value pair should be deserialized as an intrinsic; else <c>false</c>.</returns>
        internal virtual bool ShouldDeserialize(DeserializationContext context)
        {
            return true;
        }

        /// <summary>
        /// Performs some validation on the number of values being set on this intrinsic.
        /// </summary>
        /// <param name="minValues">The minimum values.</param>
        /// <param name="maxValues">The maximum values.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="System.ArgumentException">
        /// Number of values being assigned is outside the min and max constraints,
        /// </exception>
        protected void ValidateValues(int minValues, int maxValues, IList<object> values)
        {
            if (values.Count < minValues || values.Count > maxValues)
            {
                if (minValues == maxValues)
                {
                    throw new ArgumentException(
                        $"{this.LongName}: Expected {minValues} values. Got {values.Count}.",
                        nameof(values));
                }

                throw new ArgumentException(
                    $"{this.LongName}: Expected between {minValues} and {maxValues} values. Got {values.Count}.",
                    nameof(values));
            }
        }
    }
}