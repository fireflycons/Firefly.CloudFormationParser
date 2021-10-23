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
        public object MapName { get; set; } = "##UNSET";

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
            var evaluatedMapName = this.MapName switch
                {
                    string s => s,
                    IIntrinsic intrinsic => intrinsic.Evaluate(template).ToString(),
                    _ => throw new InvalidOperationException(
                             $"{this.LongName}: Invalid type {this.MapName.GetType().Name} for map name")
                };

            if (template.Mappings == null)
            {
                throw new System.InvalidOperationException("Cannot evaluate FindInMap. Template has no mappings");
            }

            if (!template.Mappings.ContainsKey(evaluatedMapName))
            {
                throw new System.InvalidOperationException($"FindInMap: Map not found '{this.MapName}'");
            }

            var map = (Dictionary<object, object>)template.Mappings[evaluatedMapName]!;

            map = (Dictionary<object, object>)this.GetNextMapLevel(template, map, this.TopLevelKey);
            return this.GetNextMapLevel(template, map, this.SecondLevelKey);
        }

        private object GetNextMapLevel(ITemplate template, IReadOnlyDictionary<object, object> map, object key)
        {
            string nextLevelKey = key switch
                {
                    string s => s,
                    IIntrinsic intrinsic => intrinsic.Evaluate(template).ToString(),
                    _ => throw new System.InvalidOperationException(
                             $"FindInMap: invalid type for top level key '{key.GetType().Name}'")
                };

            if (!map.ContainsKey(nextLevelKey))
            {
                throw new System.InvalidOperationException(
                    $"FindInMap: Map '{this.MapName}' does not contain key '{nextLevelKey}'");
            }

            return map[nextLevelKey];
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            var refs = new List<string>();
            
            foreach (var k in new[] { this.MapName, this.TopLevelKey, this.SecondLevelKey })
            {
                if (k is IIntrinsic intrinsic)
                {
                    refs.AddRange(intrinsic.GetReferencedObjects(template));
                }
            }

            return refs;
        }

        /// <inheritdoc />
        public override void SetValue(IEnumerable<object> values)
        {
            var list = values.ToList();

            this.ValidateValues(this.MinValues, this.MaxValues, list);
            this.MapName = list[0];
            this.Items = list.Skip(1).Select(this.UnpackIntrinsic).ToList();
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