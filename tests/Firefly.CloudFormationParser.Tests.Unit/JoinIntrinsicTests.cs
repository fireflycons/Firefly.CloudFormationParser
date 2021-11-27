namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;

    using Amazon.CloudFormation;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.TemplateObjects;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class JoinIntrinsicTests
    {
        const string MapName = "Map";
        const string SecondLevelKey = "Second";
        const string RegionRef = "AWS::Region";
        const string Region = "eu-west-1";
        const string RegionalValue = "ireland";

        [Fact]
        public void ShouldEvaluateJoin()
        {
            var expectedValue = $"region.{RegionalValue}";

            var template = SetupJoin(out var joinIntrinsic);

            joinIntrinsic.Evaluate(template).Should().Be(expectedValue);
        }

        [Fact]
        public void ShouldReturnCorrectReferences()
        {
            var template = SetupJoin(out var joinIntrinsic);

            var refs = joinIntrinsic.GetReferencedObjects(template).ToList();

            refs.Should().HaveCount(1);
            refs.First().Should().Be(RegionRef);
        }

        private static ITemplate SetupJoin(out JoinIntrinsic joinIntrinsic)
        {
            var @ref = new RefIntrinsic(RegionRef);

            var pp = PseudoParameter.Create(RegionRef);
            pp.SetCurrentValue(new Dictionary<string, object> { { RegionRef, Region } });

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Mappings).Returns(
                new MappingSection
                    {
                        {
                            MapName,
                            new Dictionary<object, object>
                                {
                                    { Region, new Dictionary<object, object> { { SecondLevelKey, RegionalValue } } }
                                }
                        }
                    });

            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter> { pp });
            template.Setup(t => t.UserParameterValues).Returns(new Dictionary<string, object> { { RegionRef, Region } });

            var findInMapIntrinsic = new FindInMapIntrinsic(MapName, @ref, SecondLevelKey);

            joinIntrinsic = new JoinIntrinsic(new object[] { ".", new object[] { "region", findInMapIntrinsic } });
            return template.Object;
        }
    }
}