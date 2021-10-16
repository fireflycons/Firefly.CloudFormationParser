namespace Firefly.CloudFormationParser.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.CloudFormationParser.Tests.Common;
    using Firefly.EmbeddedResourceLoader;

    using FluentAssertions;

    using Xunit;

    public class GetAZsIntrinsicTests : AutoResourceLoader
    {
#pragma warning disable 649
        [EmbeddedResource("getazs-tests.yaml")]
        private string templateContent;
#pragma warning restore 649

        [Theory]
        [InlineData("Resource1")]
        [InlineData("Resource2")]
        [InlineData("Resource3")]
        [InlineData("Resource4")]
        public async void GetAZWithinResourceShouldReferenceAWSRegionPseudoParameter(string resourceName)
        {
            const string PseudoParameterName = "AWS::Region";

            var template = await Template.Deserialize(new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var initialDotGraph = TestHelpers.GenerateDotGraph(template);
            var parameterVertex = template.DependencyGraph.Vertices.First(v => v.Name == PseudoParameterName);
            var resourceVertex = template.DependencyGraph.Vertices.First(v => v.Name == resourceName);

            var edge = template.DependencyGraph.Edges.FirstOrDefault(
                e => e.Source == parameterVertex && e.Target == resourceVertex);
        }

        [Fact]
        public async void GetAZWithinResourceWithExplicitRegionShouldResolveToRegionAZs()
        {
            const string ExpectedRegion = "us-east-1";
            const string ResourceName = "Resource5";
            var azs = new List<string>
                          {
                              $"{ExpectedRegion}a",
                              $"{ExpectedRegion}b",
                              $"{ExpectedRegion}c",
                              $"{ExpectedRegion}d",
                              $"{ExpectedRegion}e",
                              $"{ExpectedRegion}f",
                          };

            var template = await Template.Deserialize(new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var intrinsic = (GetAZsIntrinsic)template.Resources.First(r => r.Name == ResourceName).Properties
                .First(p => p.Key == "Value").Value;

            intrinsic.Evaluate(template).Should().BeEquivalentTo(azs);
        }
    }
}