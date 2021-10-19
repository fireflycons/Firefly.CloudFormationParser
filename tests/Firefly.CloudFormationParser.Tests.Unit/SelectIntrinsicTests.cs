namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.TemplateObjects;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class SelectIntrinsicTests
    {
        const string RegionRef = "AWS::Region";
        const string Region = "eu-west-1";
        const string AZ1 = "eu-west-1b";

        [Theory]
        [InlineData(0, "zero")]
        [InlineData(1, "one")]
        [InlineData(2, "two")]
        [InlineData(3, "three")]
        [InlineData(4, "four")]
        public void ShouldReturnIndexedItem(int index, string expected)
        {
            /*
                !Select [ index, [ "zero", "one", "two", "three", "four" ] ]
            */
            var template = new Mock<ITemplate>();
            var items = new[] { "zero", "one", "two", "three", "four" };
            
            var select = new SelectIntrinsic();
            select.SetValue(new object[] { index, items });

            select.Evaluate(template.Object).ToString().Should().Be(expected);
        }

        [Fact]
        public void ShouldThrowArgumentOutOfRangeExceptionWhenArgumentOutOfRange()
        {
            /*
                !Select [ 5, [ "zero", "one", "two", "three", "four" ] ]
            */
            var template = new Mock<ITemplate>();
            var items = new[] { "zero", "one", "two", "three", "four" };

            var select = new SelectIntrinsic();
            select.SetValue(new object[] { 5, items });

            var action = new Action(() => select.Evaluate(template.Object));

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(0, "zero")]
        [InlineData(1, "one")]
        [InlineData(2, "two")]
        [InlineData(3, "three")]
        [InlineData(4, "four")]
        public void ShouldEvaluateCorrectValueFromListStringParameter(int index, string expected)
        {
            /*
                !Select [ index, !Ref Param1 ]
            */
            const string ParameterName = "Param1";
            var items = new[] { "zero", "one", "two", "three", "four" };

            var parameter = new Parameter { Name = ParameterName, Type = "List<String>" };
            parameter.SetCurrentValue(new Dictionary<string, object> { { ParameterName, items } });

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Parameters).Returns(new List<IParameter> { parameter });
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            var refIntrinsic = new RefIntrinsic();
            refIntrinsic.SetValue(ParameterName);

            var select = new SelectIntrinsic();
            select.SetValue(new object[] { index, refIntrinsic });

            select.Evaluate(template.Object).ToString().Should().Be(expected);
        }

        [Theory]
        [InlineData(0, "zero")]
        [InlineData(1, "one")]
        [InlineData(2, "two")]
        [InlineData(3, "three")]
        [InlineData(4, "four")]
        public void ShouldEvaluateCorrectValueFromCommaDelimitedListParameter(int index, string expected)
        {
            /*
                !Select [ index, !Ref Param1 ]
            */
            const string ParameterName = "Param1";
            const string Items = "zero, one, two, three, four";

            var parameter = new Parameter { Name = ParameterName, Type = "CommaDelimitedList" };
            parameter.SetCurrentValue(new Dictionary<string, object> { { ParameterName, Items } });

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Parameters).Returns(new List<IParameter> { parameter });
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            var refIntrinsic = new RefIntrinsic();
            refIntrinsic.SetValue(ParameterName);

            var select = new SelectIntrinsic();
            select.SetValue(new object[] { index, refIntrinsic });

            select.Evaluate(template.Object).ToString().Should().Be(expected);
        }

        [Fact]
        public void ShouldEvaluateAzFromGetAZs()
        {
            // A classic use case
            var template = SetupTemplate(out var selectIntrinsic);

            selectIntrinsic.Evaluate(template).Should().Be(AZ1);
        }

        [Fact]
        public void ShouldReturnCorrectReferences()
        {
            var template = SetupTemplate(out var selectIntrinsic);
            var expectedReferences = new List<string> { RegionRef };

            selectIntrinsic.GetReferencedObjects(template).Should().BeEquivalentTo(expectedReferences);
        }

        private static ITemplate SetupTemplate(out SelectIntrinsic selectIntrinsic)
        {
            var refIntrinsic = new RefIntrinsic();
            refIntrinsic.SetValue(RegionRef);

            var getAzsIntrinsic = new GetAZsIntrinsic();
            getAzsIntrinsic.SetValue(refIntrinsic);

            var pseudoParameter = new Mock<IParameter>();
            pseudoParameter.Setup(p => p.Name).Returns(RegionRef);
            pseudoParameter.Setup(p => p.GetCurrentValue()).Returns(Region);

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Parameters).Returns(new List<IParameter>());
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter> { pseudoParameter.Object });

            selectIntrinsic = new SelectIntrinsic();
            selectIntrinsic.SetValue(new object[] { 1, getAzsIntrinsic });
            return template.Object;
        }
    }
}