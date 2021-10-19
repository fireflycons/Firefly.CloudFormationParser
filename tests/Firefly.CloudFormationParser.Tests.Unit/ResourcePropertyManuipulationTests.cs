namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.TemplateObjects;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class ResourcePropertyManuipulationTests
    {
        private const string Param1Name = "Param1";

        private const string Param1Value = "param-value-1";

        [Fact]
        public void ShouldReturnDictResourcePropertyValue()
        {
            var resource = SetupDictionaryResource();
            resource.GetResourcePropertyValue("Prop1.Prop2").Should().Be("a-value");
        }

        [Fact]
        public void ShouldReturnListResourcePropertyValue()
        {
            var resource = SetupListResource();
            resource.GetResourcePropertyValue("Prop1.Prop2.1").Should().Be("b-value");
        }

        [Fact]
        public void ShouldSetDictResourcePropertyValue()
        {
            var resource = SetupDictionaryResource();
            resource.UpdateResourceProperty("Prop1.Prop2", new { MyProp = "MyValue " });
            resource.GetResourcePropertyValue("Prop1.Prop2").Should().BeAssignableTo<IDictionary>(
                "the object should be converted to a dictionary within the resource");
        }

        [Fact]
        public void ShouldSetListResourcePropertyValue()
        {
            var resource = SetupListResource();
            resource.UpdateResourceProperty("Prop1.Prop2.1", new { MyProp = "MyValue " });
            resource.GetResourcePropertyValue("Prop1.Prop2.1").Should().BeAssignableTo<IDictionary>(
                "the object should be converted to a dictionary within the resource");
        }

        [Fact]
        public void ShouldReturnIntrinsicResourceValue()
        {
            var @ref = new RefIntrinsic();
            @ref.SetValue(new object[] { Param1Name });
            
            var template = SetupMockTemplate();
            var resource = SetupIntrinsicResource(template, @ref);

            resource.GetResourcePropertyValue("Prop1").Should().Be(
                Param1Value,
                "the ref intrinsic should have been evaluated");
        }


        [Fact]
        public void ShouldCreatePropertyWhenItDoesNotExist()
        {
            var expectedSecurityGroups = new List<string> { "sg-123", "sg-456", "sg-first" };

            var resource = new Resource
                               {
                                   Name = "MyFunction",
                                   Type = "AWS::Serverless::Function",
                                   Properties = new Dictionary<string, object>()
                               };

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Resources).Returns(new List<IResource> { resource });

            resource.Template = template.Object;

            resource.UpdateResourceProperty("VpcConfig.SecurityGroupIds", expectedSecurityGroups);

            resource.GetResourcePropertyValue("VpcConfig.SecurityGroupIds").Should()
                .BeEquivalentTo(expectedSecurityGroups);
        }

        private static IResource SetupDictionaryResource()
        {
            IResource resource = new Resource
                                     {
                                         Properties = new Dictionary<string, object>
                                                          {
                                                              {
                                                                  "Prop1",
                                                                  new Dictionary<object, object>
                                                                      {
                                                                          { "Prop2", "a-value" }
                                                                      }
                                                              }
                                                          }
                                     };
            return resource;
        }

        private static IResource SetupListResource()
        {
            IResource resource = new Resource
                                     {
                                         Properties = new Dictionary<string, object>
                                                          {
                                                              {
                                                                  "Prop1",
                                                                  new Dictionary<object, object>
                                                                      {
                                                                          {
                                                                              "Prop2",
                                                                              new List<object> { "a-value", "b-value" }
                                                                          }
                                                                      }
                                                              }
                                                          }
                                     };
            return resource;
        }

        private static IResource SetupIntrinsicResource(ITemplate template, IIntrinsic intrinsic)
        {
            IResource resource = new Resource
                                     {
                                         Properties = new Dictionary<string, object>
                                                          {
                                                              {
                                                                  "Prop1",
                                                                  intrinsic
                                                              }
                                                          },
                                         Template = template
                                     };
            return resource;
        }

        private static ITemplate SetupMockTemplate()
        {
            var template = new Mock<ITemplate>();

            template.Setup(
                t => t.Parameters).Returns(new List<Parameter>
                                        {
                                            new Parameter { Name = Param1Name, Type = "String", Default = Param1Value }
                                        });
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());
            return template.Object;
        }
    }
}