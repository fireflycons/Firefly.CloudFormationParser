namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class RefIntrinsicTests
    {
        [Fact]
        public void ShouldReturnReferenceNameWhenGetReferencedObjectsCalled()
        {
            const string Reference = "MyParameter";
            var template = new Mock<ITemplate>();
            var @ref = new RefIntrinsic();

            @ref.SetValue(new object[] { Reference });
            @ref.GetReferencedObjects(template.Object).First().Should().Be(Reference);
        }

        [Theory]
        [InlineData("")]
        [InlineData("ref1,ref2")]
        public void ShouldThrowForIncorrectNumberOfOperands(string operands)
        {
            var refs = operands == string.Empty ? new List<object>() : operands.Split(',').Cast<object>().ToList();
            var intrinsic = new RefIntrinsic();

            var action = new Action(() => intrinsic.SetValue(refs));

            action.Should().Throw<ArgumentException>().WithMessage(
                $"{intrinsic.LongName}: Expected 1 values. Got {refs.Count}. (Parameter 'values')");
        }
    }
}