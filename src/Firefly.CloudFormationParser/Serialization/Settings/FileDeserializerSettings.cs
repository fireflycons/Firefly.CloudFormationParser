namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System.IO;

    /// <summary>
    /// <para>
    /// Implementation that provides the mechanism to deserialize a CloudFormation template contained in a file.
    /// </para>
    /// </summary>
    /// <example>
    /// How to deserialize a template given a file containing a template.
    /// <code>
    /// public async Task&lt;ITemplate&gt; ReadFromFile(string path)
    /// {
    ///     using var settings = new FileDeserializerSettings(path);
    ///     return await Template.Deserialize(settings); 
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Firefly.CloudFormationParser.Serialization.Settings.IDeserializerSettings" />
    public class FileDeserializerSettings : StreamDeserializerSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDeserializerSettings"/> class.
        /// </summary>
        /// <param name="pathToFile">The path to a file containing template to deserialize.</param>
        public FileDeserializerSettings(string pathToFile)
            : base(File.Open(pathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }
    }
}