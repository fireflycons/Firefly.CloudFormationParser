namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class OrIntrinsicTests
    {
        [Theory]
        [InlineData("true,true", true)]
        [InlineData("true,false", true)]
        [InlineData("false,false", false)]
        [InlineData("true,true,true", true)]
        [InlineData("true,true,false", true)]
        [InlineData("true,false,true", true)]
        [InlineData("true,false,false", true)]
        [InlineData("false,true,true", true)]
        [InlineData("false,true,false", true)]
        [InlineData("false,false,true", true)]
        [InlineData("false,false,false", false)]
        public void ShouldReturnCorrectEvaluationForBooleanLiterals(string operands, bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var bools = operands.Split(',').Select(bool.Parse).Cast<object>().ToList();
            var or = new OrIntrinsic();
            or.SetValue(bools);

            ((bool)or.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("1,1", true)]
        [InlineData("1,0", true)]
        [InlineData("0,0", false)]
        [InlineData("1,1,1", true)]
        [InlineData("1,1,0", true)]
        [InlineData("1,0,1", true)]
        [InlineData("1,0,0", true)]
        [InlineData("0,1,1", true)]
        [InlineData("0,1,0", true)]
        [InlineData("0,0,1", true)]
        [InlineData("0,0,0", false)]
        public void ShouldReturnCorrectEvaluationForNumericLiterals(string operands, bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var ints = operands.Split(',').Select(int.Parse).Cast<object>().ToList();
            var or = new OrIntrinsic();
            or.SetValue(ints);

            ((bool)or.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("true,true,true,true,true,true,true,true,true,true,true")]
        public void ShouldThrowForIncorrectNumberOfOperands(string operands)
        {
            var bools = operands.Split(',').Select(bool.Parse).Cast<object>().ToList();
            var intrinsic = new OrIntrinsic();

            var action = new Action(() => intrinsic.SetValue(bools));

            action.Should().Throw<ArgumentException>().WithMessage(
                $"{intrinsic.LongName}: Expected between 2 and 10 values. Got {bools.Count}. (Parameter 'values')");
        }
    }
}