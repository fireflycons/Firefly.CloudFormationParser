namespace Firefly.CloudFormationParser.Intrinsics.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A "repository" of known intrinsic function tags
    /// </summary>
    internal static class TagRepository
    {
        /// <summary>
        /// All known intrinsic function tags
        /// </summary>
        private static readonly List<IIntrinsic> KnownTags = Assembly.GetCallingAssembly().GetTypes()
            .Where(ty => !ty.IsAbstract && ty.GetInterfaces().Any(i => i == typeof(IIntrinsic)))
            .Select(tt => (IIntrinsic)Activator.CreateInstance(tt)).ToList();

        /// <summary>
        /// Gets all known intrinsic function tags.
        /// </summary>
        /// <value>
        /// All known intrinsic function tags.
        /// </value>
        public static IEnumerable<IIntrinsic> AllTags => KnownTags;

        /// <summary>
        /// Gets a tag instance by YAML tag name
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>Instance of tag type identified by <paramref name="tagName"/></returns>
        public static IIntrinsic GetTagByName(string tagName)
        {
            var tag = KnownTags.FirstOrDefault(t => t.TagName == tagName);

            if (tag == null)
            {
                throw new InvalidOperationException($"Unknown or unsupported intrinsic '{tagName}");
            }

            return tag;
        }
    }
}