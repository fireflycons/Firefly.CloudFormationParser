namespace Firefly.CloudFormationParser.Tests.Unit
{
    using Firefly.CloudFormationParser.Utils;

    using FluentAssertions;

    using Xunit;

    public class PropertyPathTests
    {
        [Theory]
        [InlineData("Manufacturers.0.Name", "Manufacturers[0].Name")]
        [InlineData("Manufacturers.0.Products.0.Price", "Manufacturers[0].Products[0].Price")]
        [InlineData("Manufacturers.1.Products.0.Name", "Manufacturers[1].Products[0].Name")]
        [InlineData("Foo.1.5.Bar", "Foo[1][5].Bar")]
        public void ShouldConvertToJsonPath(string path, string expected)
        {
            var pp = new PropertyPath(path.Split('.'));

            pp.ToJsonPath().Should().Be(expected);
        }
    }
}