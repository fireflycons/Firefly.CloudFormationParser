namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    public class MockParser : IParser
    {
        /// <summary>
        /// Buffer of <see cref="ParsingEvent"/> read from the input stream.
        /// </summary>
        private readonly LinkedList<ParsingEvent> buffer = new LinkedList<ParsingEvent>();

        /// <summary>
        /// The current node in the buffer list
        /// </summary>
        private LinkedListNode<ParsingEvent> current;

        /// <summary>
        /// Gets the current event. Returns null before the first call to <see cref="M:YamlDotNet.Core.IParser.MoveNext" />,
        /// and also after <see cref="M:YamlDotNet.Core.IParser.MoveNext" /> returns false.
        /// </summary>
        public ParsingEvent Current => this.current.Value;

        /// <summary>
        /// Moves to the next event.
        /// </summary>
        /// <returns>
        /// Returns true if there are more events available, otherwise returns false.
        /// </returns>
        public bool MoveNext()
        {
            this.current = this.current.Next;
            return this.current != null;
        }

        /// <summary>
        /// Pushes the event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <returns>This object for chaining calls</returns>
        public MockParser PushEvent(ParsingEvent @event)
        {
            this.buffer.AddLast(@event);
            return this;
        }

        /// <summary>
        /// Resets the buffer, i.e. point to the first event.
        /// </summary>
        /// <returns>Reference to this instance.</returns>
        public MockParser Reset()
        {
            this.current = this.buffer.First;
            return this;
        }
    }
}