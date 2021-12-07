namespace Firefly.CloudFormationParser.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.CloudFormationParser.Utils;
    using Firefly.EmbeddedResourceLoader;

    using FluentAssertions;

    using Xunit;

    public class ResourceVisitor : AutoResourceLoader
    {
#pragma warning disable 649
        [EmbeddedResource("resource-visit.yaml")]
        private string templateContent;
#pragma warning restore 649

        [Fact]
        public async void SingleReferenceIsLocated()
        {
            var template = await Template.Deserialize(
                               new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var visitor = new IntrinsicCountingTemplateObjectVisitor();

            template.Resources.First(r => r.Name == "SingleReferenceIsLocated").Accept(visitor);

            visitor.VisitedIntrinsics.Should().HaveCount(1).And.AllBeOfType<RefIntrinsic>();
        }

        [Fact]
        public async void IfConditionReturnsOneOfTwoPossibleRefs()
        {
            var template = await Template.Deserialize(
                               new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var visitor = new IntrinsicCountingTemplateObjectVisitor();

            template.Resources.First(r => r.Name == "IfConditionReturnsOneOfTwoPossibleRefs").Accept(visitor);

            visitor.VisitedIntrinsics.Should().HaveCount(2, "we visited !If and one of two possible !Ref");
            visitor.VisitedIntrinsics.Last().Should().BeOfType<RefIntrinsic>().And
                .Contain(i => ((RefIntrinsic)i).Value.ToString() == "P1", "!Ref P1 should be selected");
        }

        [Fact]
        public async void SelectReturnsOneOfTwoPossibleRefs()
        {
            var template = await Template.Deserialize(
                               new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var visitor = new IntrinsicCountingTemplateObjectVisitor();

            template.Resources.First(r => r.Name == "SelectReturnsOneOfTwoPossibleRefs").Accept(visitor);

            visitor.VisitedIntrinsics.Should().HaveCount(2, "we visited !Select and one of two possible !Ref");
            visitor.VisitedIntrinsics.Last().Should().BeOfType<RefIntrinsic>().And
                .Contain(i => ((RefIntrinsic)i).Value.ToString() == "P1", "!Ref P1 should be selected");
        }

        [Fact]
        public async void SubWithTwoRefArgumentsAndOneImplicitRef()
        {
            var template = await Template.Deserialize(
                               new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var visitor = new IntrinsicCountingTemplateObjectVisitor();

            template.Resources.First(r => r.Name == "SubWithTwoRefArgumentsAndOneImplicitRef").Accept(visitor);

            visitor.VisitedIntrinsics.Should().HaveCount(4, "we visited !Sub and two !Ref as substitution parameters and one implicit !Ref");
        }

        [Fact]
        public async void GetAttIsLocated()
        {
            var template = await Template.Deserialize(
                               new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var visitor = new IntrinsicCountingTemplateObjectVisitor();

            template.Resources.First(r => r.Name == "GetAttIsLocated").Accept(visitor);

            visitor.VisitedIntrinsics.Should().HaveCount(1).And.AllBeOfType<GetAttIntrinsic>();
        }

        [Fact]
        public async void GetAttWithRef()
        {
            var template = await Template.Deserialize(
                               new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());
            var visitor = new IntrinsicCountingTemplateObjectVisitor();

            template.Resources.First(r => r.Name == "GetAttWithRef").Accept(visitor);

            visitor.VisitedIntrinsics.Should().HaveCount(2);
        }

        private class IntrinsicCountingTemplateObjectVisitor : TemplateObjectVisitor
        {
            public List<IIntrinsic> VisitedIntrinsics { get; } = new List<IIntrinsic>();

            public override bool VisitIntrinsic(ITemplateObject templateObject, PropertyPath path, IIntrinsic intrinsic)
            {
                this.VisitedIntrinsics.Add(intrinsic);
                return true;
            }
        }
    }
}