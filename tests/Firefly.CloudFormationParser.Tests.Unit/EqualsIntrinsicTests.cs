namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class EqualsIntrinsicTests
    {
        [Theory]
        [InlineData(false, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void ShouldReturnCorrectEvaluationForBooleanLiterals(
            bool leftOperand,
            bool rightOperand,
            bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var intrinsic = new EqualsIntrinsic(new List<object> { leftOperand, rightOperand });

            ((bool)intrinsic.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, false)]
        [InlineData(1, 1, true)]
        public void ShouldReturnCorrectEvaluationForNumericLiterals(
            int leftOperand,
            int rightOperand,
            bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var intrinsic = new EqualsIntrinsic(new List<object> { leftOperand, rightOperand });

            ((bool)intrinsic.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("hi", "hi", true)]
        [InlineData("hi", "HI", false)]
        public void ShouldReturnCorrectEvaluationForStringLiterals(
            string leftOperand,
            string rightOperand,
            bool expectedResult)
        {
            var template = new Mock<ITemplate>();
            var intrinsic = new EqualsIntrinsic(new List<object> { leftOperand, rightOperand });

            ((bool)intrinsic.Evaluate(template.Object)).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("one")]
        [InlineData("one,two,three")]
        public void ShouldThrowForIncorrectNumberOfOperands(string operands)
        {
            var operandList = operands.Split(".");

            var action = new Func<IIntrinsic>(() => new EqualsIntrinsic(operandList));

            action.Should().Throw<ArgumentException>().WithMessage(
                $"{EqualsIntrinsic.Tag}: Expected 2 values. Got {operandList.Length}. (Parameter 'values')");
        }
    }
}