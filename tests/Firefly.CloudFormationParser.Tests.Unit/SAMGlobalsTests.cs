namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.TemplateObjects;

    using FluentAssertions;

    using Moq;

    using Xunit;

    // https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/sam-specification-template-anatomy-globals.html
    public class SAMGlobalsTests
    {
        private const string ApiName = "MyRestApi";

        private const string LambdaHander = "index.handler";

        private static readonly GlobalSection globals = new GlobalSection
                                                            {
                                                                {
                                                                    "Function",
                                                                    new Dictionary<object, object>
                                                                        {
                                                                            { "Handler", LambdaHander },
                                                                            {
                                                                                "VpcConfig",
                                                                                new Dictionary<object, object>
                                                                                    {
                                                                                        {
                                                                                            "SecurityGroupIds",
                                                                                            new List<object>
                                                                                                {
                                                                                                    "sg-123", "sg-456"
                                                                                                }
                                                                                        }
                                                                                    }
                                                                            }
                                                                        }
                                                                },
                                                                {
                                                                    "Api",
                                                                    new Dictionary<object, object>
                                                                        {
                                                                            { "Name", ApiName }
                                                                        }
                                                                },
                                                                {
                                                                    "HttpApi",
                                                                    new Dictionary<object, object>
                                                                        {
                                                                            {
                                                                                "Tags",
                                                                                new Dictionary<object, object>
                                                                                    {
                                                                                        { "Tag1", "Value1" },
                                                                                        { "Tag2", "Value2" }
                                                                                    }
                                                                            }
                                                                        }
                                                                }
                                                            };

        [Fact]
        public void ShouldCombineListEntries()
        {
            var expectedSecurityGroups = new List<string> { "sg-123", "sg-456", "sg-first" };

            var resource = new Resource()
                               {
                                   Name = "MyFunction",
                                   Type = "AWS::Serverless::Function",
                                   Properties = new Dictionary<string, object>
                                                    {
                                                        {
                                                            "VpcConfig",
                                                            new Dictionary<object, object>
                                                                {
                                                                    {
                                                                        "SecurityGroupIds",
                                                                        new List<object> { "sg-123", "sg-first" }
                                                                    }
                                                                }
                                                        }
                                                    }
                               };

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Resources).Returns(new List<IResource> { resource });
            template.Setup(t => t.Globals).Returns(globals);
            template.Setup(t => t.Parameters).Returns(new List<IParameter>());
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            resource.Template = template.Object;

            resource.GetResourcePropertyValue("VpcConfig.SecurityGroupIds").Should()
                .BeEquivalentTo(expectedSecurityGroups);
        }

        [Fact]
        public void ShouldMergeMapValues()
        {
            var expectedResult = new Dictionary<object, object>
                                     {
                                         { "Tag1", "Value1" }, { "Tag2", "ReplacedValue2" }, { "Tag3", "Value3" }
                                     };

            var resource = new Resource()
                               {
                                   Name = "MyHttpApi",
                                   Type = "AWS::Serverless::HttpApi",
                                   Properties = new Dictionary<string, object>
                                                    {
                                                        {
                                                            "Tags",
                                                            new Dictionary<object, object>
                                                                {
                                                                    { "Tag2", "ReplacedValue2" }, { "Tag3", "Value3" }
                                                                }
                                                        }
                                                    }
                               };

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Resources).Returns(new List<IResource> { resource });
            template.Setup(t => t.Globals).Returns(globals);
            template.Setup(t => t.Parameters).Returns(new List<IParameter>());
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            resource.Template = template.Object;

            resource.GetResourcePropertyValue("Tags").Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void ShouldNotReturnLamdaHandlerFromGlobalsIfDeclaredOnFunction()
        {
            const string MyHandler = "index.my_handler";

            var resource = new Resource()
                               {
                                   Name = "MyFunction",
                                   Type = "AWS::Serverless::Function",
                                   Properties = new Dictionary<string, object> { { "Handler", MyHandler } }
                               };

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Resources).Returns(new List<IResource> { resource });
            template.Setup(t => t.Globals).Returns(globals);
            template.Setup(t => t.Parameters).Returns(new List<IParameter>());
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            resource.Template = template.Object;
        }

        [Fact]
        public void ShouldReturnLamdaHandlerFromGlobals()
        {
            var resource = new Resource()
                               {
                                   Name = "MyFunction",
                                   Type = "AWS::Serverless::Function",
                                   Properties = new Dictionary<string, object>()
                               };

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Resources).Returns(new List<IResource> { resource });
            template.Setup(t => t.Globals).Returns(globals);
            template.Setup(t => t.Parameters).Returns(new List<IParameter>());
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            resource.Template = template.Object;

            resource.GetResourcePropertyValue("Handler").Should().Be(LambdaHander);
        }
    }
}