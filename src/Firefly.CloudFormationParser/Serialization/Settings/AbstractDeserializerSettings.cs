namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Amazon.S3;

    /// <summary>
    /// Abstract base class for deserializer settings types
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.Serialization.Settings.IDeserializerSettings" />
    internal abstract class AbstractDeserializerSettings : IDeserializerSettings
    {
        /// <inheritdoc />
        public bool ExcludeConditionalResources { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> ParameterValues { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public virtual IAmazonS3? S3Client { get; set; }

        /// <inheritdoc />
        public abstract void Dispose();

        /// <inheritdoc />
        public abstract Task<TextReader> GetContentAsync();
    }
}