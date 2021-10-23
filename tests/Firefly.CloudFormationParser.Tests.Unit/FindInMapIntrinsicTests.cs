namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.TemplateObjects;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class FindInMapIntrinsicTests
    {
        private const string MapName = "Map";

        private const string SecondLevelKey = "Second";

        private const string TopLevelKey = "Top";

        [Fact]
        public void ShouldFetchListMapEntry()
        {
            var expectedValue = new List<object> { "Value1", "Value2" };
            var template = new Mock<ITemplate>();

            template.Setup(t => t.Mappings).Returns(
                new MappingSection
                    {
                        {
                            MapName,
                            new Dictionary<object, object>
                                {
                                    {
                                        TopLevelKey,
                                        new Dictionary<object, object> { { SecondLevelKey, expectedValue } }
                                    }
                                }
                        }
                    });

            var intrinsic = new FindInMapIntrinsic();

            intrinsic.SetValue(new object[] { MapName, TopLevelKey, SecondLevelKey });

            intrinsic.Evaluate(template.Object).Should().BeEquivalentTo(expectedValue);
        }

        [Fact]
        public void ShouldFetchScalarMapEntry()
        {
            const string ExpectedValue = "value";
            var template = new Mock<ITemplate>();

            template.Setup(t => t.Mappings).Returns(
                new MappingSection
                    {
                        {
                            MapName,
                            new Dictionary<object, object>
                                {
                                    {
                                        TopLevelKey,
                                        new Dictionary<object, object> { { SecondLevelKey, ExpectedValue } }
                                    }
                                }
                        }
                    });

            var intrinsic = new FindInMapIntrinsic();

            intrinsic.SetValue(new object[] { MapName, TopLevelKey, SecondLevelKey });

            intrinsic.Evaluate(template.Object).Should().Be(ExpectedValue);
        }

        [Fact]
        public void ShouldFetchValueWhenTopLevelKeyIsIntrinsic()
        {
            const string RegionRef = "AWS::Region";
            const string Region = "eu-west-1";
            const string ExpectedValue = "value";

            var @ref = new RefIntrinsic();
            @ref.SetValue("AWS::Region");

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
                                    { Region, new Dictionary<object, object> { { SecondLevelKey, ExpectedValue } } }
                                }
                        }
                    });

            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter> { pp });
            template.Setup(t => t.UserParameterValues)
                .Returns(new Dictionary<string, object> { { RegionRef, Region } });

            var intrinsic = new FindInMapIntrinsic();
            intrinsic.SetValue(new object[] { MapName, @ref, SecondLevelKey });

            intrinsic.Evaluate(template.Object).Should().Be(ExpectedValue);
        }


        [Fact]
        public void ShouldFetchValueWhenMapNameIsIntrinsic()
        {
            const string ParamName = "Param1";
            const string RegionRef = "AWS::Region";
            const string Region = "eu-west-1";
            const string ExpectedValue = "value";

            var @ref = new RefIntrinsic();
            @ref.SetValue(ParamName);

            var param = new Mock<IParameter>();
            param.Setup(p => p.Name).Returns(ParamName);
            param.Setup(p => p.GetCurrentValue()).Returns(MapName);

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Mappings).Returns(
                new MappingSection
                    {
                        {
                            MapName,
                            new Dictionary<object, object>
                                {
                                    { Region, new Dictionary<object, object> { { SecondLevelKey, ExpectedValue } } }
                                }
                        }
                    });

            template.Setup(t => t.Parameters).Returns(new List<IParameter> { param.Object });
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());
            template.Setup(t => t.UserParameterValues)
                .Returns(new Dictionary<string, object> { { RegionRef, Region } });

            var intrinsic = new FindInMapIntrinsic();
            intrinsic.SetValue(new object[] { @ref, Region, SecondLevelKey });

            intrinsic.Evaluate(template.Object).Should().Be(ExpectedValue);
        }
    }
}