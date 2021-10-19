namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class CidrIntrinsicTests
    {
        private const string ParameterName = "Cidr";

        private const string ParameterValue = "10.0.0.0/16";

        [Fact]
        public void ShouldReturnConsecutiveSubnetsForLiteralCidr()
        {
            var expectedSubnets = new List<string>
                                      {
                                          "10.0.0.0/24", "10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24",
                                      };

            var template = new Mock<ITemplate>();

            var intrinsic = new CidrIntrinsic();
            intrinsic.SetValue(new object[] { "10.0.0.0/16", 4, 8 });

            intrinsic.Evaluate(template.Object).Should().BeEquivalentTo(expectedSubnets);
        }

        [Fact]
        public void ShouldReturnConsecutiveSubnetsForReferencedCidr()
        {
            var expectedSubnets = new List<string>
                                      {
                                          "10.0.0.0/24", "10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24",
                                      };

            var template = this.SetupTemplate(out var intrinsic);

            intrinsic.Evaluate(template).Should().BeEquivalentTo(expectedSubnets);
        }

        [Fact]
        public void ShouldReturnCorrectReferences()
        {
            var expectedReferences = new List<string> { ParameterName };
            var template = this.SetupTemplate(out var intrinsic);

            intrinsic.GetReferencedObjects(template).Should().BeEquivalentTo(expectedReferences);
        }

        private ITemplate SetupTemplate(out CidrIntrinsic cidr)
        {
            var parameter = new Mock<IParameter>();
            parameter.Setup(p => p.Name).Returns(ParameterName);
            parameter.Setup(p => p.GetCurrentValue()).Returns(ParameterValue);

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Parameters).Returns(new List<IParameter> { parameter.Object });
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter>());

            var @ref = new RefIntrinsic();
            @ref.SetValue(ParameterName);

            cidr = new CidrIntrinsic();
            cidr.SetValue(new object[] { @ref, 4, 8 });

            return template.Object;
        }
    }
}