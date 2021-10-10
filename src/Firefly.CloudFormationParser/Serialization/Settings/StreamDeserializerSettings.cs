namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// <para>
    /// Implementation that provides the mechanism to deserialize a CloudFormation template from an open stream. The stream will be closed on disposal.
    /// </para>
    /// </summary>
    /// <example>
    /// How to deserialize a template given an open stream.
    /// <code>
    /// public async Task&lt;ITemplate&gt; ReadFromStream(Stream stream)
    /// {
    ///     using var settings = new StreamDeserializerSettings(stream);
    ///     return await Template.Deserialize(settings); 
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Firefly.CloudFormationParser.Serialization.Settings.IDeserializerSettings" />
    public class StreamDeserializerSettings : IDeserializerSettings
    {
        /// <summary>
        /// The stream to read
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// The reader on the stream which we return from <see cref="GetContentAsync"/>
        /// </summary>
        private StreamReader? reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDeserializerSettings"/> class.
        /// </summary>
        /// <param name="stream">The stream to read template from.</param>
        public StreamDeserializerSettings(Stream stream)
        {
            this.stream = stream;
        }

        /// <inheritdoc />
        public bool ExcludeConditionalResources { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object>? ParameterValues { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.reader?.Dispose();
            this.stream.Dispose();
        }

        /// <inheritdoc />
        public async Task<TextReader> GetContentAsync()
        {
            this.reader = new StreamReader(this.stream);

            return await Task.Run(() => this.reader);
        }
    }
}