using System;
using System.Collections.Generic;
using System.Text;

namespace Firefly.CloudFormationParser.Tests.Integration
{
    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.CloudFormationParser.Utils;

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

            var visitor = new MappingTemplateObjectVisitor();

            template.Object.Mappings.Accept(visitor);
        }

        private class MappingTemplateObjectVisitor : ITemplateObjectVisitor
        {
            public void VisitIntrinsic(ITemplateObject templateObject, PropertyPath path, IIntrinsic intrinsic)
            {
            }

            public void BeforeVisitObject<T>(ITemplateObject templateObject, PropertyPath path, IDictionary<T, object> item)
            {
            }

            public void VisitProperty<T>(ITemplateObject templateObject, PropertyPath path, KeyValuePair<T, object> item)
            {
            }

            public void AfterVisitObject<T>(ITemplateObject templateObject, PropertyPath path, IDictionary<T, object> item)
            {
            }

            public void BeforeVisitList<T>(ITemplateObject templateObject, PropertyPath path, IList<T> item)
            {
            }

            public void AfterVisitList<T>(ITemplateObject templateObject, PropertyPath path, IList<T> item)
            {
            }

            public void VisitListItem(ITemplateObject templateObject, PropertyPath path, object item)
            {
            }
        }
    }
}
