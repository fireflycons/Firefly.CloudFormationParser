namespace Firefly.CloudFormationParser.Tests.Integration
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.CloudFormationParser.TemplateObjects.Traversal;
    using Firefly.CloudFormationParser.TemplateObjects.Traversal.AcceptExtensions;

    using Moq;

    using Xunit;

    public class MappingVisitor
    {
        private const string MapName = "Map";

        private const string SecondLevelKey = "Second";

        private const string TopLevelKey = "Top";

        [Fact]
        public void Test()
        {
            var template = new Mock<ITemplate>();

            template.Setup(t => t.Mappings).Returns(
                new MappingSection
                    {
                        {
                            "Map1",
                            new Dictionary<object, object>
                                {
                                    {
                                        "TopLevelKey",
                                        new Dictionary<object, object>
                                            {
                                                { "Second1", "foo" },
                                                { "Second2", "bar" },
                                                { "Second3", new List<object> { "fizz1", "fizz2" } },
                                                { "Number", 0 },
                                                { "Boolean", false }
                                            }
                                    }
                                }
                        },
                        {
                            "Map2",
                            new Dictionary<object, object>
                                {
                                    {
                                        "TopLevelKey",
                                        new Dictionary<object, object>
                                            {
                                                { "Second1", "foo" },
                                                { "Second2", "bar" },
                                                { "Second3", new List<object> { "fizz1", "fizz2" } }
                                            }
                                    }
                                }
                        }
                    });

            var visitor = new MappingTemplateObjectVisitor(template.Object);

            template.Object.Mappings.Accept(visitor);
        }

        private class MappingTemplateObjectVisitor : TemplateObjectVisitor<NullTemplateObjectVisitorContext>
        {
            public MappingTemplateObjectVisitor(ITemplate template)
                : base(template)
            {
            }
        }
    }
}