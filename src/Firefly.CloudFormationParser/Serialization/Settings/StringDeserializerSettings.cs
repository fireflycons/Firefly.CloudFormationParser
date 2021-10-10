namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// <para>
    /// Implementation that provides the mechanism to deserialize a CloudFormation template contained in a YAML or JSON string.
    /// </para>
    /// </summary>
    /// <example>
    /// How to deserialize a template given a string containing a template body.
    /// <code>
    /// public async Task&lt;ITemplate&gt; ReadFromString(string templateBody)
    /// {
    ///     using var settings = new StringDeserializerSettings(templateBody);
    ///     return await Template.Deserialize(settings); 
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Firefly.CloudFormationParser.Serialization.Settings.IDeserializerSettings" />
    public class StringDeserializerSettings : StreamDeserializerSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringDeserializerSettings"/> class.
        /// </summary>
        /// <param name="templateContent">Content of the template as a YAML or JSON string.</param>
        public StringDeserializerSettings(string templateContent)
            : base(new MemoryStream(Encoding.UTF8.GetBytes(templateContent)))
        {
        }
    }
}