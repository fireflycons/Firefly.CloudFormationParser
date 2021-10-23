namespace Firefly.CloudFormationParser.Serialization
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Serialization.Deserializers;
    using Firefly.CloudFormationParser.Serialization.Serializers;
    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.EventEmitters;

    /// <summary>
    /// Serialize/Deserialize a CloudFormation Template
    /// </summary>
    internal class TemplateSerializer
    {
        /// <summary>
        /// Deserializes a <see cref="Template"/> from the content in the specified settings object
        /// </summary>
        /// <param name="settings"><see cref="IDeserializerSettings"/> containing template content.</param>
        /// <returns>A <see cref="Template"/></returns>
        public static async Task<Template> Deserialize(IDeserializerSettings settings)
        {
            var objectFactory = new CloudFormationSectionObjectFactory();
            var resolver = new IntrinsicFunctionNodeTypeResolver();

            // Allow object factory to notify type resolver when we move from parameters to resources to outputs
            objectFactory.DeserializationContextChanging += resolver.DeserializationContextChanged;

            var deserializer = new DeserializerBuilder().WithObjectFactory(objectFactory)
                .WithNodeDeserializer(new SubTagDeserializer(), s => s.OnTop())
                .WithNodeDeserializer(new GetAZsTagDeserializer(), s => s.OnTop())
                .WithNodeDeserializer(new Base64TagDeserializer(), s => s.OnTop())
                .WithNodeDeserializer(new GetAttTagDeserializer(), s => s.OnTop())
                .WithNodeDeserializer(new ScalarTagDeserializer<ImportValueIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ScalarTagDeserializer<RefIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ScalarTagDeserializer<ConditionIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<NotIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<EqualsIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<SelectIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<JoinIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<SplitIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<FindInMapIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<CidrIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<IfIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<AndIntrinsic>(), s => s.OnTop())
                .WithNodeDeserializer(new ArrayTagDeserializer<OrIntrinsic>(), s => s.OnTop())
                .WithNodeTypeResolver(resolver).Build();

            try
            {
                return deserializer.Deserialize<Template>(await settings.GetContentAsync());
            }
            catch (YamlException e)
            {
                if (e.InnerException is SerializationException se
                    && se.Message.Contains($"not found on type '{typeof(Resource).FullName}'"))
                {
                    throw new SerializationException(
                        "Templates containing macros that process properties added at Resource level cannot currently be deserialized by this library.",
                        e);
                }

                throw;
            }
            finally
            {
                objectFactory.DeserializationContextChanging -= resolver.DeserializationContextChanged;
            }
        }

        /// <summary>
        /// Serializes the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>YAML string.</returns>
        public static string Serialize(ITemplate template)
        {
            var typeConverters = new List<AbstractIntrinsicFunctionTypeConverter>
                                     {
                                         new IntrinsicFunctionTypeConverter<AndIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<Base64Intrinsic>(),
                                         new IntrinsicFunctionTypeConverter<CidrIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<ConditionIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<EqualsIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<FindInMapIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<GetAttIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<GetAZsIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<IfIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<ImportValueIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<JoinIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<NotIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<OrIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<RefIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<SelectIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<SplitIntrinsic>(),
                                         new IntrinsicFunctionTypeConverter<SubIntrinsic>()
                                     };

            var serializerBuilder =
                new SerializerBuilder()
                    .WithEventEmitter(inner => new CloudFormationTypeAssigningEventEmitter(inner), loc => loc.InsteadOf<TypeAssigningEventEmitter>())
                    .WithEmissionPhaseObjectGraphVisitor(
                    args => new SkipNullObjectGraphVisitor(args.InnerVisitor)); // Don't emit empty template sections

            // Register the type converters
            foreach (var tc in typeConverters)
            {
                serializerBuilder.WithTypeConverter(tc);
            }

            // Now we can build the internal value serializer and apply to all the type converters
            var valueSerializer = serializerBuilder.BuildValueSerializer();

            foreach (var tc in typeConverters)
            {
                tc.ValueSerializer = valueSerializer;
            }

            var serializer = serializerBuilder.Build();

            return serializer.Serialize(template);
        }
    }
}