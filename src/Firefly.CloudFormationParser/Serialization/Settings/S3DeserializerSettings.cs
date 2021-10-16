namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Amazon.S3;
    using Amazon.S3.Model;
    using Amazon.S3.Util;

    /// <summary>
    /// <para>
    /// Deserializer settings for reading a stack template directly from a location in an S3 bucket.
    /// </para>
    /// </summary>
    /// <example>
    /// How to deserialize a template given a bucket name and key
    /// <code>
    /// public async Task&lt;ITemplate&gt; ReadFromS3(IAmazonS3 client, string bucket, string key)
    /// {
    ///     using var settings = new S3DeserializerSettings(client, bucket, key);
    ///     return await Template.Deserialize(settings); 
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IDeserializerSettings" />
    internal class S3DeserializerSettings : AbstractDeserializerSettings
    {
        /// <summary>
        /// The bucket name
        /// </summary>
        private readonly string bucketName;

        /// <summary>
        /// The client
        /// </summary>
        private readonly IAmazonS3 client;

        /// <summary>
        /// The key
        /// </summary>
        private readonly string key;

        /// <summary>
        /// The reader on the stream which we return from <see cref="GetContentAsync"/>
        /// </summary>
        private StreamReader? reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="S3DeserializerSettings"/> class.
        /// </summary>
        /// <param name="client">An AWS S3 client with which to read the given stack template.</param>
        /// <param name="bucketName">Name of the bucket containing the template file.</param>
        /// <param name="key">The key that refers to the template file.</param>
        public S3DeserializerSettings(IAmazonS3 client, string bucketName, string key)
        {
            this.key = key;
            this.bucketName = bucketName;
            this.client = client;
        }

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="S3DeserializerSettings"/> class.
        /// </para>
        /// <para>
        /// This can take object URIs in both HTTPS and S3 schemas. S3 URIs always imply the current region.
        /// </para>
        /// <para>
        /// Since the S3 client passed to the constructor is already set up, HTTPS URIs referring to regions other than
        /// that which the client is set up for will cause <see cref="GetContentAsync"/> to fail.
        /// </para>
        /// </summary>
        /// <param name="client">An AWS S3 client with which to read the given stack template.</param>
        /// <param name="objectUri">A URI that identifies the location of the template file in S3.</param>
        public S3DeserializerSettings(IAmazonS3 client, Uri objectUri)
        {
            this.client = client;

            if (objectUri.Scheme == "s3")
            {
                // AmazonS3Uri only parses HTTPS URIs
                this.bucketName = objectUri.Host;
                this.key = objectUri.AbsolutePath.TrimStart('/');
                return;
            }

            // ReSharper disable once StyleCop.SA1305
            var s3Uri = new AmazonS3Uri(objectUri);

            this.bucketName = s3Uri.Bucket;
            this.key = s3Uri.Key;
        }

        /// <inheritdoc />
        public override IAmazonS3? S3Client
        {
            get => this.client;

            set
            {
                // ignore
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this.reader?.Dispose();
        }

        /// <inheritdoc />
        public override async Task<TextReader> GetContentAsync()
        {
            var response =
                await this.client.GetObjectAsync(new GetObjectRequest { BucketName = this.bucketName, Key = this.key });

            this.reader = new StreamReader(response.ResponseStream);
            return this.reader;
        }
    }
}