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

    public class RefIntrinsicTests
    {
        [Fact]
        public void ShouldReturnReferenceNameWhenGetReferencedObjectsCalled()
        {
            const string Reference = "MyParameter";
            var template = new Mock<ITemplate>();
            var @ref = new RefIntrinsic(Reference);

            @ref.GetReferencedObjects(template.Object).First().Should().Be(Reference);
        }

        [Theory]
        [InlineData("")]
        [InlineData("ref1,ref2")]
        public void ShouldThrowForIncorrectNumberOfOperands(string operands)
        {
            var refs = operands == string.Empty ? new List<object>() : operands.Split(',').Cast<object>().ToList();

            var action = new Func<IIntrinsic>(() => new RefIntrinsic(refs));

            action.Should().Throw<ArgumentException>().WithMessage(
                $"{RefIntrinsic.Tag}: Expected 1 values. Got {refs.Count}. (Parameter 'values')");
        }
    }
}