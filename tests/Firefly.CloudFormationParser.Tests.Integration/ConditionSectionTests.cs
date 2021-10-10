namespace Firefly.CloudFormationParser.Tests.Integration
{
    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.EmbeddedResourceLoader;

    using Xunit;

    public class ConditionSectionTests : AutoResourceLoader
    {
#pragma warning disable 649
        [EmbeddedResource("condition-section-tests.yaml")]
        private string templateContent;
#pragma warning restore

        [Fact]
        public async void ShouldParseCondtionSection()
        {
            var template = await Template.Deserialize(new StringDeserializerSettings(this.templateContent));
        }
    }
}