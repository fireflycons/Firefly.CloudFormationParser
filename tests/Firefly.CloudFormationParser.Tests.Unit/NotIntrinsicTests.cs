namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class NotIntrinsicTests
    {
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void ShouldReturnCorrectEvaluationForBooleanLiterals(bool operand, bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var intrinsic = new NotIntrinsic(operand);

            ((bool)intrinsic.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(0, true)]
        [InlineData(-1, false)]
        [InlineData(int.MaxValue, false)]
        public void ShouldReturnCorrectEvaluationForNumericLiterals(int operand, bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var intrinsic = new NotIntrinsic(operand);

            ((bool)intrinsic.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("")]
        [InlineData("true,true")]
        public void ShouldThrowForIncorrectNumberOfOperands(string operands)
        {
            var bools = operands == string.Empty
                            ? new List<object>()
                            : operands.Split(',').Select(bool.Parse).Cast<object>().ToList();

            var action = new Func<IIntrinsic>(() => new NotIntrinsic(bools));

            action.Should().Throw<ArgumentException>().WithMessage(
                $"{NotIntrinsic.Tag}: Expected 1 values. Got {bools.Count}. (Parameter 'values')");
        }
    }
}