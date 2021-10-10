namespace Firefly.CloudFormationParser.S3.Tests.Unit
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Amazon.S3;
    using Amazon.S3.Model;

    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.EmbeddedResourceLoader;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class S3TemplateTests : AutoResourceLoader
    {
        private const string BadBucketMessage = "The specified bucket does not exist";

        private const string BadKeyMessage = "The specified key does not exist";

        private const string BucketName = "my-bucket";

        private const string Key = "my-template.yaml";

#pragma warning disable 649
        [EmbeddedResource("test-stack.yaml", "Firefly.CloudFormationParser.Tests.Common")]
        private string templateContent;
#pragma warning restore 649

        [Fact]
        public async void ShouldReadTemplateWhenBucketAndKeyAreValid()
        {
            var mockClient = this.CreateMockS3Client();

            using var settings = new S3DeserializerSettings(mockClient.Object, BucketName, Key);

            var template = await Template.Deserialize(settings);

            template.Resources.Count().Should().Be(2);
        }

        [Fact]
        public async void ShouldReadTemplateWhenHttpUriIsValid()
        {
            var mockClient = this.CreateMockS3Client();

            using var settings = new S3DeserializerSettings(
                mockClient.Object,
                new Uri($"https://{BucketName}.s3.eu-west-1.amazonaws.com/{Key}"));

            var template = await Template.Deserialize(settings);

            template.Resources.Count().Should().Be(2);
        }

        [Fact]
        public async void ShouldReadTemplateWhenS3UriIsValid()
        {
            var mockClient = this.CreateMockS3Client();

            using var settings = new S3DeserializerSettings(mockClient.Object, new Uri($"s3://{BucketName}/{Key}"));

            var template = await Template.Deserialize(settings);

            template.Resources.Count().Should().Be(2);
        }

        [Theory]
        [InlineData("s3", "bad-bucket", Key, BadBucketMessage)]
        [InlineData("s3", BucketName, "bad-key", BadKeyMessage)]
        [InlineData("https", "bad-bucket", Key, BadBucketMessage)]
        [InlineData("https", BucketName, "bad-key", BadKeyMessage)]
        public async void ShouldThowWhenURIReferencesInvalidObject(
            string scheme,
            string bucket,
            string key,
            string expectedMessage)
        {
            Uri uri;

            uri = scheme == "s3"
                      ? new Uri($"s3://{bucket}/{key}")
                      : new Uri($"https://{bucket}.s3.eu-west-1.amazonaws.com/{key}");

            var mockClient = this.CreateMockS3Client();

            using var settings = new S3DeserializerSettings(mockClient.Object, uri);

            Func<Task<TextReader>> action = async () => await settings.GetContentAsync();

            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage(expectedMessage);
        }

        [Fact]
        public async void ShouldThrowWhenBucketNameIsInvalid()
        {
            var mockClient = this.CreateMockS3Client();

            using var settings = new S3DeserializerSettings(mockClient.Object, "bad-bucket", Key);

            Func<Task<TextReader>> action = async () => await settings.GetContentAsync();

            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage(BadBucketMessage);
        }

        [Fact]
        public async void ShouldThrowWhenKeytNameIsInvalid()
        {
            var mockClient = this.CreateMockS3Client();

            using var settings = new S3DeserializerSettings(mockClient.Object, BucketName, BadKeyMessage);

            Func<Task<TextReader>> action = async () => await settings.GetContentAsync();

            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage(BadKeyMessage);
        }

        private Mock<IAmazonS3> CreateMockS3Client()
        {
            var client = new Mock<IAmazonS3>();

            client.Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), default)).ReturnsAsync(
                (GetObjectRequest r, CancellationToken c) =>
                    {
                        if (r.BucketName != BucketName)
                        {
                            throw new InvalidOperationException(BadBucketMessage);
                        }

                        if (r.Key != Key)
                        {
                            throw new InvalidOperationException(BadKeyMessage);
                        }

                        return new GetObjectResponse
                                   {
                                       ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(this.templateContent))
                                   };
                    });

            return client;
        }
    }
}