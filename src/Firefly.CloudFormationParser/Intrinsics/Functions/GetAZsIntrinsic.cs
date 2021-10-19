namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization.Serializers;
    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-getavailabilityzones.html">Fn::GetAZs</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class GetAZsIntrinsic : AbstractScalarIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!GetAZs";

        /// <inheritdoc />
        public override string LongName => "Fn::GetAZs";

        /// <summary>
        /// Gets the region to get AZs for.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public object Region => this.Value;

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new ScalarEmitterTrait();

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            string regionToGet;

            switch (this.Region)
            {
                case string s when !string.IsNullOrEmpty(s):

                    regionToGet = s;
                    break;

                case string _:

                    var pp = template.Parameters.Concat(template.PseudoParameters).FirstOrDefault(p => p.Name == "AWS::Region");

                    if (pp == null)
                    {
                        throw new InvalidOperationException(
                            "Pseudo parameter AWS::Region not found. Should have been declared by now");
                    }

                    regionToGet = pp.CurrentValue!.ToString();
                    break;

                case AbstractIntrinsic intrinsic:

                    regionToGet = intrinsic.Evaluate(template).ToString();
                    break;

                default:

                    throw new ArgumentException(
                        $"Fn::GetAZs: Invalid type for region argument: {this.Region.GetType().Name}");
            }

            return RegionInfo.GetAZs(regionToGet);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            if (this.Region is AbstractIntrinsic intrinsic)
            {
                return intrinsic.GetReferencedObjects(template);
            }

            if (this.Region is string s && (s == "AWS::Region" || string.IsNullOrEmpty(s)))
            {
                return new[] { "AWS::Region" };
            }

            return new List<string>();
        }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            if (this.Region is IDictionary)
            {
                return new List<UnresolvedTagProperty>
                           {
                               new UnresolvedTagProperty
                                   {
                                       Intrinsic = this, Property = this.GetType().GetProperty(nameof(this.Value))
                                   }
                           };
            }

            return new List<UnresolvedTagProperty>();
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(this, emitter, nestedValueSerializer, new[] { this.Region });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(this, emitter, nestedValueSerializer, new[] { this.Region });
        }
    }
}