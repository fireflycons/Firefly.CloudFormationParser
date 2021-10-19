namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class Base64IntrinsicTests
    {
        [Fact]
        public void ShouldEncodeLiteralValue()
        {
            const string LiteralValue = "A string literal";
            var template = new Mock<ITemplate>();
            var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(LiteralValue));

            var intrinsic = new Base64Intrinsic();
            intrinsic.SetValue(LiteralValue);
            var result = intrinsic.Evaluate(template.Object).ToString();

            result.Should().Be(expected);
        }

        [Fact]
        public void ShouldEncodeReferencedParameterValue()
        {
            const string LiteralValue = "A string literal";
            const string ParameterName = "Param1";
            var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(LiteralValue));

            var mockParameter = new Mock<IParameter>();
            mockParameter.Setup(p => p.Name).Returns(ParameterName);
            mockParameter.Setup(p => p.CurrentValue).Returns(LiteralValue);

            var mockTemplate = new Mock<ITemplate>();
            mockTemplate.Setup(t => t.Parameters).Returns(new List<IParameter> { mockParameter.Object });
            mockTemplate.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            var refIntrinsic = new RefIntrinsic();
            refIntrinsic.SetValue(LiteralValue);

            var base64Intrinsic = new Base64Intrinsic();
            base64Intrinsic.SetValue(refIntrinsic);
            var result = base64Intrinsic.Evaluate(mockTemplate.Object).ToString();

            result.Should().Be(expected);
        }

        [Fact]
        public void ShouldNotThrowWhenArgumentIsIntrinsic()
        {
            var intrinsic = new Base64Intrinsic();

            var action = new Action(() => intrinsic.SetValue(new[] { new SubIntrinsic() }));

            action.Should().NotThrow();
        }

        [Fact]
        public void ShouldNotThrowWhenArgumentIsScalar()
        {
            var intrinsic = new Base64Intrinsic();

            var action = new Action(() => intrinsic.SetValue(new[] { "blah" }));

            action.Should().NotThrow();
        }
    }
}