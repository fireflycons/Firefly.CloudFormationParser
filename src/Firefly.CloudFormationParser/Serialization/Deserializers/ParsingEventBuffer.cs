namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System.Collections.Generic;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    /// <summary>
    /// A buffer used to perform a read-ahead on the YAML stream so that we can modify/replay parsing events.
    /// </summary>
    /// <seealso cref="YamlDotNet.Core.IParser" />
    /// <seealso href="https://gist.github.com/atruskie/bfb7e9ee3df954a29cbc17bdf12405f9"/>
    internal class ParsingEventBuffer : IParser
    {
        /// <summary>
        /// Buffer of <see cref="ParsingEvent"/> read from the input stream.
        /// </summary>
        private readonly LinkedList<ParsingEvent> buffer;

        /// <summary>
        /// The current node in the buffer list
        /// </summary>
        private LinkedListNode<ParsingEvent>? current;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingEventBuffer"/> class.
        /// </summary>
        /// <param name="events">The events.</param>
        public ParsingEventBuffer(LinkedList<ParsingEvent> events)
        {
            this.buffer = events;
            this.current = events.First;
        }

        /// <summary>
        /// Gets the current event. Returns null before the first call to <see cref="M:YamlDotNet.Core.IParser.MoveNext" />,
        /// and also after <see cref="M:YamlDotNet.Core.IParser.MoveNext" /> returns false.
        /// </summary>
        public ParsingEvent? Current => this.current?.Value;

        /// <summary>
        /// Read a nested mapping ensuring that if it's a tagged mapping,
        /// we remove the tag else we will get indefinite recursion and a stack overflow.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns>List of events for nested object deserializer</returns>
        public static ParsingEventBuffer FromNestedMapping(IParser parser)
        {
            var parsingEvents = new LinkedList<ParsingEvent>();
            var mapping = parser.Consume<MappingStart>();

            // Kill tag to prevent recursion
            var newMapping = new MappingStart(
                AnchorName.Empty,
                TagName.Empty,
                mapping.IsImplicit,
                mapping.Style,
                mapping.Start,
                mapping.End);

            parsingEvents.AddLast(newMapping);
            var depth = 0;
            do
            {
                var next = parser.Consume<ParsingEvent>();
                depth += next.NestingIncrease;
                parsingEvents.AddLast(next);
            }
            while (depth >= 0);

            return new ParsingEventBuffer(parsingEvents).Reset();
        }

        /// <summary>
        /// Read a nested sequence ensuring that if it's a tagged sequence,
        /// we remove the tag else we will get indefinite recursion and a stack overflow.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns>List of events for nested object deserializer</returns>
        public static ParsingEventBuffer FromNestedSequence(IParser parser)
        {
            var parsingEvents = new LinkedList<ParsingEvent>();
            var sequence = parser.Consume<SequenceStart>();

            // Kill tag to prevent recursion
            var newSequence = new SequenceStart(
                AnchorName.Empty,
                TagName.Empty,
                sequence.IsImplicit,
                sequence.Style,
                sequence.Start,
                sequence.End);

            parsingEvents.AddLast(newSequence);
            var depth = 0;
            do
            {
                var next = parser.Consume<ParsingEvent>();
                depth += next.NestingIncrease;
                parsingEvents.AddLast(next);
            }
            while (depth >= 0);

            return new ParsingEventBuffer(parsingEvents).Reset();
        }

        /// <summary>
        /// Read a nested scalar tag.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns>List of events for nested object deserializer</returns>
        public static ParsingEventBuffer FromNestedScalar(IParser parser)
        {
            var scalar = parser.Consume<Scalar>();

            // Buffer this scalar and remove the tag, and the following event as nestedObjectDeserializer will call MoveNext
            LinkedList<ParsingEvent> events = new LinkedList<ParsingEvent>();
            events.AddLast(
                new Scalar(
                    scalar.Anchor,
                    TagName.Empty,
                    scalar.Value,
                    scalar.Style,
                    scalar.IsPlainImplicit,
                    scalar.IsQuotedImplicit,
                    scalar.Start,
                    scalar.End));

            return new ParsingEventBuffer(events).Reset();
        }

        /// <summary>
        /// Moves to the next event.
        /// </summary>
        /// <returns>
        /// Returns true if there are more events available, otherwise returns false.
        /// </returns>
        public bool MoveNext()
        {
            this.current = this.current?.Next;
            return this.current != null;
        }

        /// <summary>
        /// Resets the buffer, i.e. point to the first event.
        /// </summary>
        /// <returns>Reference to this instance.</returns>
        public ParsingEventBuffer Reset()
        {
            this.current = this.buffer.First;
            return this;
        }
    }
}