namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;

    using Amazon.CloudFormation;
    using Amazon.CloudFormation.Model;

    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.EmbeddedResourceLoader;

    using FluentAssertions;

    using Moq;

    using Xunit;

    using Parameter = Amazon.CloudFormation.Model.Parameter;

    public class CfnStackTests : AutoResourceLoader
    {
#pragma warning disable 649
        [EmbeddedResource("test-stack.yaml", "Firefly.CloudFormationParser.Tests.Common")]
        private string templateContent;
#pragma warning restore 649

        [Fact]
        public async void ShouldReadBothResourcesWhenExcludeConditionalResourcesIsFalse()
        {
            var mockClient = this.CreateMockCloudFormationClient();

            using var settings = new DeserializerSettingsBuilder()
                .WithCloudFormationStack(mockClient.Object, "my-stack").Build();

            var template = await Template.Deserialize(settings);

            template.Resources.Count().Should().Be(2, "conditional resources were not excluded");
        }

        [Fact]
        public async void ShouldReadOnlyR1WhenR2ExcludedByConditions()
        {
            var mockClient = this.CreateMockCloudFormationClient();

            using var settings = new DeserializerSettingsBuilder()
                .WithCloudFormationStack(mockClient.Object, "my-stack")
                .WithExcludeConditionalResources(true)
                .Build();

            var template = await Template.Deserialize(settings);

            template.Resources.FirstOrDefault(r => r.Name == "R2").Should()
                .BeNull("R2 should be excluded by condition");
        }

        [Fact]
        public async void ShouldProvideUserValueForNoEchoParameter()
        {
            var mockClient = this.CreateMockCloudFormationClient();

            using var settings = new DeserializerSettingsBuilder()
                .WithCloudFormationStack(mockClient.Object, "my-stack")
                .WithParameterValues(new Dictionary<string, object>
                                         {
                                             {"P3", "user-value"}
                                         })
                .Build();

            await Template.Deserialize(settings);

            settings.ParameterValues["P3"].Should().Be("user-value");
        }

        private Mock<IAmazonCloudFormation> CreateMockCloudFormationClient()
        {
            var mockClient = new Mock<IAmazonCloudFormation>();

            mockClient.Setup(c => c.GetTemplateAsync(It.IsAny<GetTemplateRequest>(), default)).ReturnsAsync(
                new GetTemplateResponse { TemplateBody = this.templateContent });

            mockClient.Setup(c => c.DescribeStacksAsync(It.IsAny<DescribeStacksRequest>(), default)).ReturnsAsync(
                new DescribeStacksResponse
                    {
                        Stacks = new List<Stack>
                                     {
                                         new Stack
                                             {
                                                 Parameters = new List<Parameter>
                                                                  {
                                                                      new Parameter
                                                                          {
                                                                              ParameterKey = "P1",
                                                                              ParameterValue = "p1-value"
                                                                          },
                                                                      new Parameter
                                                                          {
                                                                              ParameterKey = "P2", ParameterValue = "0"
                                                                          },
                                                                      new Parameter
                                                                          {
                                                                              ParameterKey = "P3",
                                                                              ParameterValue = string.Empty
                                                                          }
                                                                  }
                                             }
                                     }
                    });

            mockClient.Setup(c => c.GetTemplateSummaryAsync(It.IsAny<GetTemplateSummaryRequest>(), default))
                .ReturnsAsync(new GetTemplateSummaryResponse
                                  {
                                      Parameters = new List<ParameterDeclaration>
                                                       {
                                                           new ParameterDeclaration
                                                               {
                                                                   ParameterKey = "P1",
                                                                   ParameterType = "String",
                                                                   NoEcho = false
                                                               },
                                                           new ParameterDeclaration
                                                               {
                                                                   ParameterKey = "P2",
                                                                   ParameterType = "Number",
                                                                   NoEcho = false
                                                               },
                                                           new ParameterDeclaration
                                                               {
                                                                   ParameterKey = "P3",
                                                                   ParameterType = "String",
                                                                   NoEcho = true
                                                               }
                                                       }
                                  });

            return mockClient;
        }
    }
}