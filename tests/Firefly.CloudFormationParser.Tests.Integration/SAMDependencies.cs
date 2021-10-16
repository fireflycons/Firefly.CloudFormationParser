namespace Firefly.CloudFormationParser.Tests.Integration
{
    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.EmbeddedResourceLoader;

    using Xunit;

    public class SAMDependencies : AutoResourceLoader
    {
#pragma warning disable 649
        [EmbeddedResource("sam-dependencies.yaml")]
        private string templateContent;
#pragma warning restore 649

        [Fact]
        public async void DependencyOnImpliedSAMResourceShoudBeIgnored()
        {
            var template = await Template.Deserialize(new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
        }
    }
}