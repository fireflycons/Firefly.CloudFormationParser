namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization.Serializers;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-cidr.html">Fn::Cidr</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class CidrIntrinsic : AbstractArrayIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Cidr";

        private static readonly Regex validIpv4Regex = new Regex(
            @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)/(2[0-8]|1[6-9])");

        /// <summary>
        /// Gets or sets the number of subnet bits for the CIDR. For example, specifying a value "8" for this parameter will create a CIDR with a mask of "/24"..
        /// </summary>
        /// <value>
        /// The CIDR bits.
        /// </value>
        public int CidrBits { get; set; }

        /// <summary>
        /// Gets or sets The number of CIDRs to generate.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the user-specified CIDR address block to be split into smaller CIDR blocks.
        /// </summary>
        /// <value>
        /// The IP block.
        /// </value>
        public object IpBlock { get; set; } = string.Empty;

        /// <inheritdoc />
        public override string LongName => "Fn::Cidr";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new SequenceEmitterTrait();

        /// <inheritdoc />
        internal override int MaxValues => 3;

        /// <inheritdoc />
        internal override int MinValues => 3;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            string cidr = this.IpBlock switch
                {
                    string s => s,
                    IIntrinsic intrinsic => (string)intrinsic.Evaluate(template),
                    _ => throw new ArgumentException(
                             $"Fn::Cidr: Cannot evaluate argument of type {this.IpBlock.GetType().Name}")
                };

            if (!validIpv4Regex.IsMatch(cidr))
            {
                throw new ArgumentException($"Fn::Cidr: {cidr} is not a valid IPv4 CIDR (must be in range /16 - /28)");
            }

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            return new List<string>();
        }

        /// <inheritdoc />
        public override void SetValue(IEnumerable<object> values)
        {
            var list = values.ToList();

            this.ValidateValues(this.MinValues, this.MaxValues, list);
            this.IpBlock = list[0];
            this.Count = int.Parse((string)list[1]);
            this.CidrBits = int.Parse((string)list[2]);
        }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            if (this.IpBlock is IDictionary)
            {
                return new List<UnresolvedTagProperty>
                           {
                               new UnresolvedTagProperty
                                   {
                                       Intrinsic = this, Property = this.GetType().GetProperty(nameof(this.IpBlock))
                                   }
                           };
            }

            return new List<UnresolvedTagProperty>();
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(
                this,
                emitter,
                nestedValueSerializer,
                new List<object> { this.IpBlock, this.Count, this.CidrBits });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(
                this,
                emitter,
                nestedValueSerializer,
                new List<object> { this.IpBlock, this.Count, this.CidrBits });
        }
    }
}