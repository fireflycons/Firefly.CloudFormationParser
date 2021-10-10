namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.TemplateObjects;

    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Node type resolver for CloudFormation Intrinsic functions
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.INodeTypeResolver" />
    internal class IntrinsicFunctionNodeTypeResolver : INodeTypeResolver
    {
        /// <summary>
        /// Map of tag name to intrinsic function type
        /// </summary>
        private readonly Dictionary<string, Type> intrinsicTagNameToType =
            TagRepository.AllTags.ToDictionary(t => t.TagName, t => t.GetType());

        /// <summary>
        /// Instantiation of all intrinsic types
        /// </summary>
        private readonly List<IIntrinsic> allIntrinsics = TagRepository.AllTags.ToList();

        /// <summary>
        /// The previous parsing event, so we can see if it was a long-form intrinsic function name.
        /// </summary>
        private ParsingEvent? previousEvent;

        /// <summary>
        /// Gets the current context.
        /// </summary>
        /// <value>
        /// The current context.
        /// </value>
        internal DeserializationContext CurrentContext { get; private set; }

        /// <summary>
        /// Determines the type of the specified node.
        /// </summary>
        /// <param name="nodeEvent">The node to be deserialized.</param>
        /// <param name="currentType">The type that has been determined so far.</param>
        /// <returns>
        /// true if <paramref name="currentType" /> has been resolved completely;
        /// false if the next type <see cref="T:YamlDotNet.Serialization.INodeTypeResolver" /> should be invoked.
        /// </returns>
        public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
        {
            var tp = currentType;

            try
            {
                if (nodeEvent == null)
                {
                    return false;
                }

                // At this point, look at the previously parsed event. 
                // If that contains the long name of an intrinsic function and the current context
                // indicates it should be parsed as such, then return the type of the intrinsic.
                if (this.previousEvent is Scalar scalar)
                {
                    if (this.allIntrinsics.FirstOrDefault(t => t.LongName == scalar.Value) is AbstractIntrinsic intrinsic && intrinsic.ShouldDeserialize(this.CurrentContext))
                    {
                        currentType = intrinsic.GetType();
                        return true;
                    }
                }

                if (nodeEvent.Tag.IsEmpty)
                {
                    currentType = tp;
                    return false;
                }

                if (!string.IsNullOrEmpty(nodeEvent.Tag.Value) && this.intrinsicTagNameToType.ContainsKey(nodeEvent.Tag.Value))
                {
                    currentType = this.intrinsicTagNameToType[nodeEvent.Tag.Value];
                    return true;
                }

                return false;
            }
            finally
            {
                // Ensure the previous event is always recorded.
                this.previousEvent = nodeEvent;
            }
        }

        /// <summary>
        /// <para>
        /// Event handler for context change in the deserializer.
        /// </para>
        /// <para>
        /// This is triggered upon creation of anything inherited from <see cref="ITemplateObject"/>
        /// by <see cref="CloudFormationSectionObjectFactory"/>.
        /// </para>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="DeserializationContextChangingEventArgs"/> instance containing the event data.</param>
        internal void DeserializationContextChanged(object sender, DeserializationContextChangingEventArgs args)
        {
            this.CurrentContext = args.CurrentContext;
        }
    }
}