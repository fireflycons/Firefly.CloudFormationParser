namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Serialization.Serializers;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-findinmap.html">Fn::FindInMap</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    public class FindInMapIntrinsic : AbstractArrayIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!FindInMap";

        /// <inheritdoc />
        public override string LongName => "Fn::FindInMap";

        /// <summary>
        /// Gets or sets the name of the map.
        /// </summary>
        /// <value>
        /// The name of the map.
        /// </value>
        public string MapName { get; set; } = "##UNSET";

        /// <summary>
        /// Gets or sets the second level key.
        /// </summary>
        /// <value>
        /// The second level key.
        /// </value>
        public object SecondLevelKey => this.Items.Count == this.MaxValues - 1 ? this.Items[1] : "##UNSET";

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <summary>
        /// Gets or sets the top level key.
        /// </summary>
        /// <value>
        /// The top level key. May be a !Ref
        /// </value>
        public object TopLevelKey => this.Items.Count == this.MaxValues - 1 ? this.Items[0] : "##UNSET";

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new SequenceEmitterTrait();

        /// <inheritdoc />
        internal override int MaxValues => 3;

        /// <inheritdoc />
        internal override int MinValues => 3;

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
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
            var list = (List<object>)values;

            this.ValidateValues(this.MinValues, this.MaxValues, list);
            this.MapName = (string)list[0];
            this.Items = list.Skip(1).ToList();

            // this.TopLevelKey = list[1];
            // this.SecondLevelKey = list[2];
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.TagName} [ {this.MapName}, {this.TopLevelKey}, {this.SecondLevelKey} ]";
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteLongForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.MapName, this.TopLevelKey, this.SecondLevelKey });
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            this.EmitterTrait.WriteShortForm(
                this,
                emitter,
                nestedValueSerializer,
                new[] { this.MapName, this.TopLevelKey, this.SecondLevelKey });
        }
    }
}