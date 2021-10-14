namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.TemplateObjects;

    using FluentAssertions;

    using Xunit;

    public class ResourcePropertyManuipulationTests
    {
        [Fact]
        public void ShouldReturnDictResourcePropertyValue()
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

            resource.GetResourcePropertyValue("Prop1.Prop2").Should().Be("a-value");
        }

        [Fact]
        public void ShouldSetDictResourcePropertyValue()
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

            resource.UpdateResourceProperty("Prop1.Prop2", new ObjectValue());

            resource.GetResourcePropertyValue("Prop1.Prop2").Should().BeAssignableTo<IDictionary>("the object should be converted to a dictionary within the resource");
        }

        [Fact]
        public void ShouldReturnListResourcePropertyValue()
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

            resource.GetResourcePropertyValue("Prop1.Prop2.1").Should().Be("b-value");
        }

        [Fact]
        public void ShouldSetListResourcePropertyValue()
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

            resource.UpdateResourceProperty("Prop1.Prop2.1", new ObjectValue());

            resource.GetResourcePropertyValue("Prop1.Prop2.1").Should().BeAssignableTo<IDictionary>("the object should be converted to a dictionary within the resource");
        }

        private class ObjectValue
        {
            public string MyValue { get; set; } = "MyValue";
        }
    }
}