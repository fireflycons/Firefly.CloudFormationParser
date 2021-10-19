namespace Firefly.CloudFormationParser.Tests.Unit
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;

    using FluentAssertions;

    using Moq;

    using Xunit;

    public class IfIntrinsictests
    {
        private const string FalseConditionName = "FalseCondition";

        private const string NoValue = "AWS::NoValue";

        private const string ParameterName = "KeyArn";

        private const string ParameterValue =
            "arn:aws:kms:us-east-1:123456789012:key/0000000-0000-0000-0000-000000000000";

        private const string TrueConditionName = "TrueCondition";

        [Fact]
        public void ShouldEvaluateFalseCondition()
        {
            var template = this.SetupTemplate(out var intrinsic, false);

            var result = intrinsic.Evaluate(template);
            result.Should().BeOfType(typeof(AWSNoValue));
        }

        [Fact]
        public void ShouldEvaluateTrueCondition()
        {
            // We expect the nested !Ref KeyArn to be evaluated too.
            var expected = new Dictionary<object, object>
                               {
                                   { "PolicyName", "MyPolicy" },
                                   {
                                       "PolicyDocument",
                                       new Dictionary<object, object>
                                           {
                                               { "Version", "2012-10-17 " },
                                               {
                                                   "Statement",
                                                   new List<object>
                                                       {
                                                           new Dictionary<object, object>
                                                               {
                                                                   { "Resource", ParameterValue },
                                                                   { "Effect", "Allow" },
                                                                   { "Action", "kms:Decrypt" }
                                                               }
                                                       }
                                               }
                                           }
                                   }
                               };

            var template = this.SetupTemplate(out var intrinsic, true);

            var result = intrinsic.Evaluate(template);
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ShouldReturnCorrectReferencesForFalseCondition()
        {
            var expected = new List<string> { NoValue };
            var template = this.SetupTemplate(out var intrinsic, false);

            var result = intrinsic.GetReferencedObjects(template);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ShouldReturnCorrectReferencesForTrueCondition()
        {
            var expected = new List<string> { ParameterName };
            var template = this.SetupTemplate(out var intrinsic, true);

            var result = intrinsic.GetReferencedObjects(template);

            result.Should().BeEquivalentTo(expected);
        }

        private ITemplate SetupTemplate(out IfIntrinsic ifIntrinsic, bool logic)
        {
            var evaluatedConditions = new Dictionary<string, bool>
                                          {
                                              { TrueConditionName, true }, { FalseConditionName, false }
                                          };

            var parameter = new Mock<IParameter>();
            parameter.Setup(p => p.Name).Returns(ParameterName);
            parameter.Setup(p => p.GetCurrentValue()).Returns(ParameterValue);

            var noValue = new Mock<IParameter>();
            noValue.Setup(p => p.Name).Returns(NoValue);
            noValue.Setup(p => p.GetCurrentValue()).Returns(new AWSNoValue());

            var template = new Mock<ITemplate>();
            template.Setup(t => t.EvaluatedConditions).Returns(evaluatedConditions);
            template.Setup(t => t.Parameters).Returns(new List<IParameter> { parameter.Object });
            template.Setup(t => t.PseudoParameters).Returns(new List<IParameter> { noValue.Object });

            var refIntrinsic = new RefIntrinsic();
            refIntrinsic.SetValue(ParameterName);

            var noValueIntrinsic = new RefIntrinsic();
            noValueIntrinsic.SetValue(NoValue);

            ifIntrinsic = new IfIntrinsic();
            ifIntrinsic.SetValue(
                new List<object>
                    {
                        logic ? TrueConditionName : FalseConditionName,
                        new Dictionary<object, object>
                            {
                                { "PolicyName", "MyPolicy" },
                                {
                                    "PolicyDocument",
                                    new Dictionary<object, object>
                                        {
                                            { "Version", "2012-10-17 " },
                                            {
                                                "Statement",
                                                new List<object>
                                                    {
                                                        new Dictionary<object, object>
                                                            {
                                                                { "Resource", refIntrinsic },
                                                                { "Effect", "Allow" },
                                                                { "Action", "kms:Decrypt" }
                                                            }
                                                    }
                                            }
                                        }
                                }
                            },
                        noValueIntrinsic
                    });

            return template.Object;
        }
    }
}