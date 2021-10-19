namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class SplitIntrinsicTests
    {
        private const string Delimeter = "|";
        private const string ParameterName = "Param1";

        [Fact]
        public void ShouldEvaluateSimpleSplit()
        {
            var strings = new List<string> { "one", "two", "three" };

            var template = new Mock<ITemplate>();

            var splitIntrinsic = new SplitIntrinsic();
            splitIntrinsic.SetValue(new object[] { Delimeter, string.Join(Delimeter, strings) });

            splitIntrinsic.Evaluate(template.Object).Should().BeEquivalentTo(strings);
        }

        [Fact]
        public void ShouldFollowRulesForEmptyDelimiters()
        {
            // https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-split.html
            var strings = new List<string> { "a", string.Empty, "c", string.Empty };

            var template = new Mock<ITemplate>();

            var splitIntrinsic = new SplitIntrinsic();
            splitIntrinsic.SetValue(new object[] { Delimeter, string.Join(Delimeter, strings) });

            splitIntrinsic.Evaluate(template.Object).Should().BeEquivalentTo(strings);
        }

        [Fact]
        public void ShouldEvaluteSplitFromIntrinsicReturn()
        {
            var strings = new List<string> { "one", "two", "three" };
            var template = SetupTemplate(strings, out var splitIntrinsic);

            splitIntrinsic.Evaluate(template).Should().BeEquivalentTo(strings);
        }

        [Fact]
        public void ShouldReturnCorrectReferences()
        {
            var strings = new List<string> { "one", "two", "three" };
            var expectedReferences = new List<string> { ParameterName };
            var template = SetupTemplate(strings, out var splitIntrinsic);

            splitIntrinsic.GetReferencedObjects(template).Should().BeEquivalentTo(expectedReferences);
        }

        private static ITemplate SetupTemplate(List<string> strings, out SplitIntrinsic splitIntrinsic)
        {
            var param = new Mock<IParameter>();
            param.Setup(p => p.Name).Returns(ParameterName);
            param.Setup(p => p.GetCurrentValue()).Returns(string.Join(Delimeter, strings));

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Parameters).Returns(new List<IParameter> { param.Object });
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            var refIntrinsic = new RefIntrinsic();
            refIntrinsic.SetValue(ParameterName);

            splitIntrinsic = new SplitIntrinsic();
            splitIntrinsic.SetValue(new object[] { Delimeter, refIntrinsic });
            return template.Object;
        }
    }
}