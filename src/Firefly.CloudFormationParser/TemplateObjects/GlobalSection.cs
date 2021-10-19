namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Serialization.Deserializers;

    /// <summary>
    /// Container to hold the <c>Globals:</c> section during template deserialization.
    /// This section is unique to AWS SAM.
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.TemplateObjects.ITemplateSection" />
    /// <seealso href="https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/sam-specification-template-anatomy-globals.html" />
    public class GlobalSection : Dictionary<string, object>, ITemplateSection
    {
        /// <inheritdoc cref="ITemplateSection.Context"/>
        public DeserializationContext Context => DeserializationContext.Globals;

        /// <summary>
        /// <para>
        /// Gets a named section from the global declarations map.
        /// </para>
        /// <para>
        /// Section can be one of <c>Function</c>, <c>Api</c>, <c>HttpApi</c> or <c>SimpleTable</c>.
        /// </para>
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <returns>The section if declared, else <c>null</c></returns>
        public Dictionary<string, object>? GetSection(string sectionName)
        {
            return !this.ContainsKey(sectionName)
                       ? null
                       : ((Dictionary<object, object>)this[sectionName]).ToDictionary(
                           kv => kv.Key.ToString(),
                           kv => kv.Value);
        }
    }
}