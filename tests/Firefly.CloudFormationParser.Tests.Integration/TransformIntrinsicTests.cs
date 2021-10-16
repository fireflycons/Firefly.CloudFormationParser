namespace Firefly.CloudFormationParser.Tests.Integration
{
    using System.Threading.Tasks;

    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.EmbeddedResourceLoader;

    using Xunit;

    public class TransformIntrinsicTests : AutoResourceLoader
    {
#pragma warning disable 649
        [EmbeddedResource("transform.yaml")]
        private string templateContent;
#pragma warning restore 649

        [Fact]
        public async Task ShouldDeserializeTransformAtTopLevel()
        {
            var template = await Template.Deserialize(new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var yaml = Template.Serialize(template);
        }
    }
}