namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Functions;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class SubInstrinsicTests
    {
        [Fact]
        public void ShouldReturnNestedReferenceNamesWhenGetReferencedObjectsCalled()
        {
            const string Ref1 = "Param1";
            const string Ref2 = "Resource1";

            var references = new List<string> { Ref1, Ref2 };
            var template = new Mock<ITemplate>();
            var sub = new SubIntrinsic();

            var @ref = new RefIntrinsic();
            @ref.SetValue(Ref2);

            sub.SetValue(
                new List<object> { $"blah=${{{Ref1}}}-${{P1}}", new Dictionary<object, object> { { "P1", @ref } } });

            sub.GetReferencedObjects(template.Object).Should().BeEquivalentTo(references);
        }

        [Fact]
        public void ShouldReturnReferenceNamesWhenGetReferencedObjectsCalled()
        {
            const string Ref1 = "Param1";
            const string Ref2 = "Resource1";

            var references = new List<string> { Ref1, Ref2 };
            var template = new Mock<ITemplate>();
            var sub = new SubIntrinsic();

            sub.SetValue($"blah=${{{Ref1}}}-${{{Ref2}}}");

            sub.GetReferencedObjects(template.Object).Should().BeEquivalentTo(references);
        }
    }
}