namespace Firefly.CloudFormationParser.Intrinsics.Functions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Utils;
    using Firefly.CloudFormationParser.Serialization.Serializers;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents the <see href="https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-sub.html">Fn::Sub</see> intrinsic.
    /// </summary>
    /// <seealso cref="IIntrinsic" />
    [DebuggerDisplay("{TagName} {Expression}")]
    public class SubIntrinsic : AbstractIntrinsic, IReferenceIntrinsic
    {
        /// <summary>
        /// The tag
        /// </summary>
        public const string Tag = "!Sub";

        /// <summary>
        /// Regex to find substitution parameters in the expression
        /// </summary>
        private static readonly Regex SubstitutionRx =
            new Regex(@"\$\{(?<id>(([a-zA-Z][a-zA-Z0-9]+(\.[a-zA-Z]+)?)|(AWS::[a-zA-Z]+))*)\}");

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>
        /// The expression.
        /// </value>
        public string Expression { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the item list for the intrinsic
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public List<object> Items { get; protected set; } = new List<object>();

        /// <inheritdoc />
        public override string LongName => "Fn::Sub";

        /// <summary>
        /// Gets or sets the substitutions.
        /// </summary>
        /// <value>
        /// The substitutions.
        /// </value>
        public Dictionary<object, object> Substitutions { get; set; } = new Dictionary<object, object>();

        /// <inheritdoc />
        public override string TagName => Tag;

        /// <inheritdoc />
        internal override IEmitterTrait EmitterTrait { get; } = new EmptyEmitterTrait();

        /// <inheritdoc />
        public override object Evaluate(ITemplate template)
        {
            var replacements = new Dictionary<string, string>();

            // Build list of replacements
            foreach (var match in SubstitutionRx.Matches(this.Expression).Cast<Match>())
            {
                var reference = match.Groups["id"].Value;

                if (this.Substitutions.ContainsKey(reference))
                {
                    var sub = this.Substitutions[reference];

                    if (sub == null)
                    {
                        throw new InvalidOperationException($"Value for '{match}' in Fn::Sub map cannot be null.");
                    }

                    switch (sub)
                    {
                        case IIntrinsic intrinsic:

                            replacements.Add(reference, intrinsic.Evaluate(template).ToString());
                            break;

                        default:

                            replacements.Add(reference, sub.ToString());
                            break;
                    }

                    continue;
                }

                // Implicit Ref
                var @ref = new RefIntrinsic();

                @ref.SetValue(reference);
                replacements.Add(reference, @ref.Evaluate(template).ToString());
            }

            var evaluatedExpression = this.Expression;

            return replacements.Aggregate(
                evaluatedExpression,
                (current, replacement) => current.Replace($"${{{replacement.Key}}}", replacement.Value));
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencedObjects(ITemplate template)
        {
            var refs = new List<string>();

            var mc = SubstitutionRx.Matches(this.Expression);

            foreach (var match in mc.Cast<Match>())
            {
                var param = match.Groups["id"].Value;

                if (this.Substitutions != null && this.Substitutions.ContainsKey(param))
                {
                    switch (this.Substitutions[param])
                    {
                        case string s:

                            refs.Add(s);
                            break;

                        case AbstractIntrinsic tag:

                            refs.AddRange(tag.GetReferencedObjects(template));
                            break;
                    }
                }
                else
                {
                    refs.Add(param);
                }
            }

            return refs;
        }

        /// <inheritdoc />
        public override void SetValue(IEnumerable<object> values)
        {
            var list = values.ToList();

            this.ValidateValues(1, 2, list);

            this.Expression = (string)list[0];

            if (list.Count > 1)
            {
                this.Substitutions = ((Dictionary<object, object>)list[1]).ToDictionary(
                    kv => kv.Key,
                    kv => this.UnpackIntrinsic(kv.Value));
            }
        }

        /// <inheritdoc />
        internal override IList<UnresolvedTagProperty> GetUnresolvedDictionaryProperties()
        {
            var unresolved = new List<UnresolvedTagProperty>();

            // ReSharper disable once IsExpressionAlwaysTrue
            if (this.Substitutions is IDictionary)
            {
                unresolved.Add(
                    new UnresolvedTagProperty
                        {
                            Property = this.GetType().GetProperty(nameof(this.Substitutions)), Intrinsic = this
                        });
            }

            return unresolved;
        }

        /// <inheritdoc />
        internal override void WriteLongForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            if (!this.Substitutions.Any())
            {
                new ScalarEmitterTrait().WriteLongForm(this, emitter, nestedValueSerializer, new[] { this.Expression });
                return;
            }

            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(this.LongName));
            emitter.Emit(
                new SequenceStart(AnchorName.Empty, YamlDotNet.Core.TagName.Empty, false, SequenceStyle.Block));
            this.EmitSubsitutions(emitter, nestedValueSerializer);
            emitter.Emit(new MappingEnd());
        }

        /// <inheritdoc />
        internal override void WriteShortForm(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            if (!this.Substitutions.Any())
            {
                new ScalarEmitterTrait().WriteShortForm(this, emitter, nestedValueSerializer, new[] { this.Expression });
                return;
            }

            emitter.Emit(new SequenceStart(AnchorName.Empty, new TagName(this.TagName), false, SequenceStyle.Block));

            this.EmitSubsitutions(emitter, nestedValueSerializer);
        }

        /// <summary>
        /// Emits the body of the !sub tag
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="nestedValueSerializer">An <see cref="IValueSerializer"/> with which to write nested objects such as dictionaries.</param>
        private void EmitSubsitutions(IEmitter emitter, IValueSerializer nestedValueSerializer)
        {
            if (this.Substitutions == null)
            {
                // null check is already done before this is called, by we still need to shut to compiler up.
                throw new InvalidOperationException(
                    "Fn::Sub: Cannot call EmitSubsitutions when there are no substitutions");
            }

            emitter.Emit(new Scalar(this.Expression));
            emitter.Emit(new MappingStart());

            foreach (var kv in this.Substitutions)
            {
                emitter.Emit(new Scalar(kv.Key.ToString()));

                switch (kv.Value)
                {
                    case AbstractIntrinsic nestedIntrinsic:

                        nestedIntrinsic.WriteYaml(emitter, nestedValueSerializer);
                        break;

                    case IDictionary _:
                    case IList _:

                        nestedValueSerializer.SerializeValue(emitter, kv.Value, kv.Value.GetType());
                        break;

                    default:
                        emitter.Emit(new Scalar(kv.Value.ToString()));
                        break;
                }
            }

            emitter.Emit(new MappingEnd());
            emitter.Emit(new SequenceEnd());
        }
    }
}