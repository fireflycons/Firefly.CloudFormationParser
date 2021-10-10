namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization.Serializers;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-getatt.html">Fn::GetAtt</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    [DebuggerDisplay("{TagName} {LogicalId}.{AttributeName}")]
    public class GetAttIntrinsic : AbstractArrayIntrinsic, IReferenceIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!GetAtt";

        /// <summary>
        /// Gets or sets the name of the attribute. This may be a <see cref="RefIntrinsic"/> or a string.
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        public object AttributeName { get; set; } = new object();

        /// <summary>
        /// Gets or sets the logical identifier.
        /// </summary>
        /// <value>
        /// The logical identifier.
        /// </value>
        public string LogicalId { get; set; } = string.Empty;

        /// <inheritdoc />
        public override string LongName => "Fn::GetAtt";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new SequenceEmitterTrait();

        /// <inheritdoc />
        internal override int MaxValues => 2;

        /// <inheritdoc />
        internal override int MinValues => 2;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            string attribute = this.AttributeName switch
                {
                    RefIntrinsic refTag => refTag.GetReferencedObjects(template).First(),
                    string s => s,
                    _ => throw new InvalidOperationException(
                             $"Unexpected type {this.AttributeName.GetType().Name} for attribute value of Fn::GetAtt")
                };

            return new Tuple<string, string>(this.LogicalId, attribute);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            var (item1, item2) = (Tuple<string, string>)this.Evaluate(template);

            return new List<string> { $"{item1}.{item2}" };
        }

        /// <inheritdoc />
        public override void SetValue(IEnumerable<object> values)
        {
            var list = (List<object>)values;

            this.ValidateValues(this.MinValues, this.MaxValues, list);
            this.LogicalId = (string)list[0];
            this.AttributeName = list[1];
        }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            if (this.AttributeName is IDictionary)
            {
                return new List<UnresolvedTagProperty>
                           {
                               new UnresolvedTagProperty
                                   {
                                       Intrinsic = this,
                                       Property = this.GetType().GetProperty(nameof(this.AttributeName))
                                   }
                           };
            }

            return new List<UnresolvedTagProperty>();
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(this, emitter, nestedValueSerializer, new[] { this.LogicalId, this.AttributeName });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(this, emitter, nestedValueSerializer, new[] { this.LogicalId, this.AttributeName });
        }
    }
}