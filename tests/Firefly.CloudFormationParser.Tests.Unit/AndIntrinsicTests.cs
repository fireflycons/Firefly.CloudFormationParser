namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class AndIntrinsicTests
    {
        [Theory]
        [InlineData("true,true", true)]
        [InlineData("true,false", false)]
        [InlineData("false,false", false)]
        [InlineData("true,true,true", true)]
        [InlineData("true,true,false", false)]
        [InlineData("true,false,true", false)]
        [InlineData("true,false,false", false)]
        [InlineData("false,true,true", false)]
        [InlineData("false,true,false", false)]
        [InlineData("false,false,true", false)]
        [InlineData("false,false,false", false)]
        public void ShouldReturnCorrectEvaluationForBooleanLiterals(string operands, bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var bools = operands.Split(',').Select(bool.Parse).Cast<object>().ToList();
            var intrinsic = new AndIntrinsic(bools);

            ((bool)intrinsic.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("1,1", true)]
        [InlineData("1,0", false)]
        [InlineData("0,0", false)]
        [InlineData("1,1,1", true)]
        [InlineData("1,1,0", false)]
        [InlineData("1,0,1", false)]
        [InlineData("1,0,0", false)]
        [InlineData("0,1,1", false)]
        [InlineData("0,1,0", false)]
        [InlineData("0,0,1", false)]
        [InlineData("0,0,0", false)]
        public void ShouldReturnCorrectEvaluationForNumericLiterals(string operands, bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var ints = operands.Split(',').Select(int.Parse).Cast<object>().ToList();
            var intrinsic = new AndIntrinsic(ints);

            ((bool)intrinsic.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("true,true,true,true,true,true,true,true,true,true,true")]
        public void ShouldThrowForIncorrectNumberOfOperands(string operands)
        {
            var bools = operands.Split(',').Select(bool.Parse).Cast<object>().ToList();

            var action = new Func<IIntrinsic>(() => new AndIntrinsic(bools));

            action.Should().Throw<ArgumentException>().WithMessage(
                $"{AndIntrinsic.Tag}: Expected between 2 and 10 values. Got {bools.Count}. (Parameter 'values')");
        }
    }
}