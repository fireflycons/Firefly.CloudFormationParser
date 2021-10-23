namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    /// <summary>
    /// Extensions for <c>YamlDoNet</c> emitter interface
    /// </summary>
    /// <remarks>
    /// Dirty hack awaiting a better solution for <see href="https://github.com/aaubry/YamlDotNet/issues/644"/>
    /// </remarks>
    internal static class EmitterExtensions
    {
        /// <summary>
        /// Accesses the private events field on the emitter
        /// </summary>
        private static readonly FieldInfo EventsField = typeof(Emitter).GetField(
            "events",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Accesses the private state field on the emitter
        /// </summary>
        private static readonly FieldInfo StateField = typeof(Emitter).GetField(
            "state",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Determines whether the emitter is about to emit a key.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <returns>
        ///   <c>true</c> if the next token emitted will be a key; else <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">emitter is <c>null</c></exception>
        public static bool IsEmittingKey(this IEmitter? emitter)
        {
            if (emitter == null)
            {
                throw new ArgumentNullException(nameof(emitter));
            }

            var state = GetState(emitter);

            if (state == EmitterState.BlockMappingFirstKey || state == EmitterState.BlockMappingKey)
            {
                // State indicates it will emit a key next.
                return true;
            }

            // If the next event in the queue is a mapping start, then a key will be emitted next.
            return GetEventQueue(emitter).FirstOrDefault()?.GetType() == typeof(MappingStart);
        }

        /// <summary>
        /// Gets the event queue.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <returns>Event queue</returns>
        private static Queue<ParsingEvent> GetEventQueue(IEmitter emitter)
        {
            return (Queue<ParsingEvent>)EventsField.GetValue(emitter);
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <returns>Current state.</returns>
        private static EmitterState GetState(IEmitter emitter)
        {
            return (EmitterState)StateField.GetValue(emitter);
        }
    }
}