namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Utils;

    using FluentAssertions;

    using Xunit;

    public class NetworkUtilsTests
    {
        [Fact]
        public void ShouldReturnConsecutiveSubnets()
        {
            var expectedSubnets = new List<string>
                                      {
                                          "10.0.0.0/24", "10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24",
                                      };

            var network = new Network("10.0.0.0/16");
            var result = network.GetSubnets(4, 8);

            result.Should().BeEquivalentTo(expectedSubnets);
        }
    }
}