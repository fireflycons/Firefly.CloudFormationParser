namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.TemplateObjects;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class SubInstrinsicTests
    {
        private const string TemplateParameterName = "Param1";
        private const string TemplateParameterValue = "value";
        private const string ResourceName = "Resource1";
        private const string SubstitutionParameterName = "P1";

        [Fact]
        public void ShouldReturnNestedReferenceNamesWhenGetReferencedObjectsCalled()
        {
            var references = new List<string> { TemplateParameterName, ResourceName };
            var template = new Mock<ITemplate>();
            var sub = new SubIntrinsic();

            var @ref = new RefIntrinsic();
            @ref.SetValue(ResourceName);

            sub.SetValue(
                new List<object> { $"blah-${{{TemplateParameterName}}}-${{{SubstitutionParameterName}}}", new Dictionary<object, object> { { SubstitutionParameterName, @ref } } });

            sub.GetReferencedObjects(template.Object).Should().BeEquivalentTo(references);
        }

        [Fact]
        public void ShouldReturnReferenceNamesWhenGetReferencedObjectsCalled()
        {

            var references = new List<string> { TemplateParameterName, ResourceName };
            var template = new Mock<ITemplate>();
            var sub = new SubIntrinsic();

            sub.SetValue($"blah=${{{TemplateParameterName}}}-${{{ResourceName}}}");

            sub.GetReferencedObjects(template.Object).Should().BeEquivalentTo(references);
        }

        [Fact]
        public void ShouldEvaluateExpression()
        {
            const string RegionRef = "AWS::Region";
            const string Region = "eu-west-1";

            var expression = $"blah-${{{TemplateParameterName}}}-${{{SubstitutionParameterName}}}-${{{RegionRef}}}";
            var expected = $"blah-{TemplateParameterValue}-{ResourceName}-{Region}";

            var parameter = new Mock<IParameter>();
            parameter.Setup(p => p.Name).Returns(TemplateParameterName);
            parameter.Setup(p => p.GetCurrentValue()).Returns(TemplateParameterValue);

            var pseudoParameter = new Mock<IParameter>();
            pseudoParameter.Setup(p => p.Name).Returns(RegionRef);
            pseudoParameter.Setup(p => p.GetCurrentValue()).Returns(Region);


            var resource = new Mock<IResource>();
            resource.Setup(r => r.Name).Returns(ResourceName);

            var template = new Mock<ITemplate>();
            template.Setup(t => t.Parameters).Returns(new List<IParameter> { parameter.Object });
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter> { pseudoParameter.Object });
            template.Setup(t => t.Resources).Returns(new List<IResource> { resource.Object });

            var @ref = new RefIntrinsic();
            @ref.SetValue(ResourceName);

            var sub = new SubIntrinsic();
            sub.SetValue(
                new List<object> { expression, new Dictionary<object, object> { { SubstitutionParameterName, @ref } } });

            sub.Evaluate(template.Object).Should().Be(expected);
        }
    }
}