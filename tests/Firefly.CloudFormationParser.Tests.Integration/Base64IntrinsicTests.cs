namespace Firefly.CloudFormationParser.Tests.Integration
{
    using System.Linq;

    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.CloudFormationParser.Tests.Common;
    using Firefly.EmbeddedResourceLoader;

    using FluentAssertions;

    using Xunit;

    public class Base64IntrinsicTests : AutoResourceLoader
    {
#pragma warning disable 649
        [EmbeddedResource("base64-tests.yaml")]
        private string templateContent;
#pragma warning restore 649

        [Theory]
        [InlineData("Param1", "Resource1")]
        [InlineData("Param1", "Resource2")]
        [InlineData("Param1", "Resource3")]
        [InlineData("Param1", "Resource4")]
        public async void ParameterShouldBeReferencedByResource(string parameterName, string resourceName)
        {
            var template = await Template.Deserialize(new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var dotGraph = TestHelpers.GenerateDotGraph(template);
            var parameterVertex = template.DependencyGraph.Vertices.First(v => v.Name == parameterName);
            var resourceVertex = template.DependencyGraph.Vertices.First(v => v.Name == resourceName);

            var edge = template.DependencyGraph.Edges.FirstOrDefault(
                e => e.Source == parameterVertex && e.Target == resourceVertex);

            edge.Should().NotBeNull($"{parameterName} should be referenced by {resourceName}");
        }
    }
}