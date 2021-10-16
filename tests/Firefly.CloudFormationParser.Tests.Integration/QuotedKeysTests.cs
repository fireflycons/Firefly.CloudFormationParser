namespace Firefly.CloudFormationParser.Tests.Integration
{
    using System;

    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;
    using Firefly.EmbeddedResourceLoader;

    using FluentAssertions;

    using Xunit;

    using YamlDotNet.Core;

    public class QuotedKeysTests : AutoResourceLoader
    {
#pragma warning disable 649
        [EmbeddedResource("quoted-keys.yaml")]
        private string templateContent;
#pragma warning restore

        public async void ShouldQuoteNumericKeyOnSerialization()
        {
            const string numericKey = "0123456789012";
            string yaml;

            var template = await Template.Deserialize(new DeserializerSettingsBuilder().WithTemplateString(this.templateContent).Build());

            try
            {
                yaml = Template.Serialize(template);
            }
            catch (YamlException e)
            {
                Skip.If(
                    e.InnerException is ArgumentNullException { Message: "Value cannot be null. (Parameter 'key')" },
                    "Bug in YamlDotNet where a key name of 'null' is not quoted on serialization. https://github.com/aaubry/YamlDotNet/issues/591.");

                throw;
            }

            yaml.Should().Contain($"'{numericKey}'");
        }
    }
}