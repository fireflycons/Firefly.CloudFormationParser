namespace Firefly.CloudFormationParser.Tests.Integration
{
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.CloudFormationParser.Tests.Common;
    using Firefly.EmbeddedResourceLoader;

    using FluentAssertions;

    using Xunit;

    public class PolicyConditionTests : AutoResourceLoader
    {
#pragma warning disable 649 
        [EmbeddedResource("policy-condition-pre.yaml")]
        private string policyConditionPre;

        [EmbeddedResource("policy-condition-post.yaml")]
        private string policyConditionPost;
#pragma warning restore

        [Fact]
        public async void PolicyConditionShouldNotBeDeserializedAsIntrinsicWhenConditionsDeclaredFirst()
        {
            var template = await Template.Deserialize(new StringDeserializerSettings(this.policyConditionPre));
            TestHelpers.ContainsObjectOfType(template.Resources.First().Properties, typeof(IIntrinsic)).Should()
                .BeFalse(
                    "'Condition' within resource properties (e.g. policies) should not be deserialized as intrinsic");

            var logicalIntrinsic = (AbstractLogicalIntrinsic)(template.Conditions!.Last().Value);

            logicalIntrinsic.Operands.First().Should().BeOfType<ConditionIntrinsic>("'Condition' within Conditions section should be deserialized as intrinsic");
        }

        [Fact]
        public async void PolicyConditionShouldNotBeDeserializedAsIntrinsicWhenConditionsDeclaredLast()
        {
            var template = await Template.Deserialize(new StringDeserializerSettings(this.policyConditionPost));
            TestHelpers.ContainsObjectOfType(template.Resources.First().Properties, typeof(IIntrinsic)).Should()
                .BeFalse(
                    "'Condition' within resource properties (e.g. policies) should not be deserialized as intrinsic");

            var logicalIntrinsic = (AbstractLogicalIntrinsic)(template.Conditions!.Last().Value);

            logicalIntrinsic.Operands.First().Should().BeOfType<ConditionIntrinsic>("'Condition' within Conditions section should be deserialized as intrinsic");
        }
    }
}