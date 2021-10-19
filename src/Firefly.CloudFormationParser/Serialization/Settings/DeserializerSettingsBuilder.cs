namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Amazon.CloudFormation;
    using Amazon.S3;

    using Firefly.CloudFormationParser.TemplateObjects;

    /// <summary>
    /// Builder class to construct an <see cref="IDeserializerSettings"/> for the template deserialization process.
    /// </summary>
    public class DeserializerSettingsBuilder
    {
        private bool configuredExclusions;

        private readonly Dictionary<string, object> configuredParameterValues = new Dictionary<string, object>
                                                                                    {
                                                                                        { "AWS::Region", "eu-west-1" }
                                                                                    };

        private IAmazonS3? configuredS3Client;

        private AbstractDeserializerSettings? settings;

        /// <summary>
        /// Builds the settings used by <see cref="Template.Deserialize"/>.
        /// </summary>
        /// <returns>A new implementation of <see cref="IDeserializerSettings"/> appropriate for the builder's configuration.</returns>
        public IDeserializerSettings Build()
        {
            if (this.settings == null)
            {
                throw new InvalidDeserializerSettingsException(
                    "No template source has been specified for deserialization");
            }

            this.settings.ParameterValues = this.configuredParameterValues;
            this.settings.ExcludeConditionalResources = this.configuredExclusions;
            this.settings.S3Client = this.configuredS3Client;

            return this.settings;
        }

        /// <summary>
        /// Specifies the AWS account ID to be used when evaluating <c>AWS::AccountId</c> pseudo-parameter.
        /// </summary>
        /// <param name="accountId">The 12 digit account identifier.</param>
        /// <returns>This builder</returns>
        // ReSharper disable once InconsistentNaming
        public DeserializerSettingsBuilder WithAWSAccountId(string accountId)
        {
            this.configuredParameterValues["AWS::AccountId"] = accountId;
            return this;
        }

        /// <summary>
        /// Specifies the AWS region to be used when evaluating <c>AWS::Region</c> pseudo-parameter and <c>Fn::GetAZs</c>.
        /// </summary>
        /// <param name="region">Region to use. Default <c>us-east-1</c></param>
        /// <returns>This builder</returns>
        // ReSharper disable once InconsistentNaming
        public DeserializerSettingsBuilder WithAWSRegion(string region)
        {
            this.configuredParameterValues["AWS::Region"] = region;
            return this;
        }

        /// <summary>
        /// Specifies an existing CloudFormation Stack from which to acquire the template.
        /// </summary>
        /// <param name="client">A configured CloudFormation client that has access to the stack.</param>
        /// <param name="stackId">The stack identifier - name or ARN.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithCloudFormationStack(IAmazonCloudFormation client, string stackId)
        {
            if (this.settings != null)
            {
                throw new InvalidDeserializerSettingsException(
                    "Cannot call WithCloudFormationStack when another method to set the template source has already been called.");
            }

            this.settings = new CfnStackDeserializerSettings(client, stackId);
            return this;
        }

        /// <summary>
        /// <para>
        /// Sets a value indicating whether to exclude resources and outputs nullified by the evaluation of conditions.
        /// </para>
        /// <para>
        /// When this is <c>true</c>, the results of the evaluation of conditions in the <c>Conditions</c> block of the template using the values of
        /// the parameters as supplied by <see cref="WithParameterValues"/> are used to eliminate any resource or output where its <c>Condition</c>
        /// property evaluates to <c>false</c> from the template object returned by the call to <see cref="Template.Deserialize"/>
        /// </para>
        /// </summary>
        /// <param name="excludeConditionalResources">if set to <c>true</c> [exclude conditional resources].</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithExcludeConditionalResources(bool excludeConditionalResources)
        {
            this.configuredExclusions = excludeConditionalResources;
            return this;
        }

        /// <summary>
        /// <para>
        /// Sets an optional dictionary of values to assign to parameters.
        /// </para>
        /// <para>
        /// If left unset, then parameter defaults where present will be used in evaluations, or where there is no
        /// declared default value, the default for the type - empty string, zero for number etc.
        /// </para>
        /// </summary>
        /// <param name="parameterValues">The parameter values.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithParameterValues(IDictionary<string, object> parameterValues)
        {
            foreach (var kv in parameterValues)
            {
                this.configuredParameterValues[kv.Key] = kv.Value;
            }

            return this;
        }

        /// <summary>
        /// <para>
        /// Specifies an S3 client to use where a template may reference objects in S3 such as nested stacks or includes.
        /// </para>
        /// <para>
        /// This is unnecessary if reading a template directly from S3 using <see cref="WithTemplateS3(IAmazonS3,string,string)"/> or <see cref="WithTemplateS3(IAmazonS3,Uri)"/>
        /// </para>
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithS3Client(IAmazonS3 client)
        {
            this.configuredS3Client = client;
            return this;
        }

        /// <summary>
        /// <para>
        /// Specifies the CloudFormation Stack ARN to be used when evaluating <c>"AWS::StackId</c> pseudo-parameter.
        /// </para>
        /// <para>
        /// This is unnecessary if reading a stack directly using <see cref="WithCloudFormationStack"/>
        /// </para>
        /// </summary>
        /// <param name="stackId">Name of the stack.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithStackId(string stackId)
        {
            this.configuredParameterValues["AWS::StackId"] = stackId;
            return this;
        }

        /// <summary>
        /// <para>
        /// Specifies the CloudFormation Stack name to be used when evaluating <c>"AWS::StackName</c> pseudo-parameter.
        /// </para>
        /// <para>
        /// This is unnecessary if reading a stack directly using <see cref="WithCloudFormationStack"/>
        /// </para>
        /// </summary>
        /// <param name="stackName">Name of the stack.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithStackName(string stackName)
        {
            this.configuredParameterValues["AWS::StackName"] = stackName;
            return this;
        }

        /// <summary>
        /// Specifies template content in a local file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithTemplateFile(string path)
        {
            if (this.settings != null)
            {
                throw new InvalidDeserializerSettingsException(
                    "Cannot call WithTemplateFile when another method to set the template source has already been called.");
            }

            this.settings = new FileDeserializerSettings(path);
            return this;
        }

        /// <summary>
        /// Specifies template content as an S3 object.
        /// </summary>
        /// <param name="client">A configured S3 client that has access to the S3 object.</param>
        /// <param name="bucket">The bucket.</param>
        /// <param name="key">The key.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithTemplateS3(IAmazonS3 client, string bucket, string key)
        {
            if (this.settings != null)
            {
                throw new InvalidDeserializerSettingsException(
                    "Cannot call WithTemplateS3 when another method to set the template source has already been called.");
            }

            this.settings = new S3DeserializerSettings(client, bucket, key);
            return this;
        }

        /// <summary>
        /// Specifies template content as an S3 object.
        /// </summary>
        /// <param name="client">A configured S3 client that has access to the S3 object.</param>
        /// <param name="templateUri">HTTP or S3 uri pointing to the template.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithTemplateS3(IAmazonS3 client, Uri templateUri)
        {
            if (this.settings != null)
            {
                throw new InvalidDeserializerSettingsException(
                    "Cannot call WithTemplateS3 when another method to set the template source has already been called.");
            }

            this.settings = new S3DeserializerSettings(client, templateUri);
            return this;
        }

        /// <summary>
        /// Specifies template content read from an open stream.
        /// </summary>
        /// <param name="stream">The stream form which to read the template.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithTemplateStream(Stream stream)
        {
            if (this.settings != null)
            {
                throw new InvalidDeserializerSettingsException(
                    "Cannot call WithTemplateStream when another method to set the template source has already been called.");
            }

            this.settings = new StreamDeserializerSettings(stream);
            return this;
        }

        /// <summary>
        /// Specifies template content as a YAML or JSON string
        /// </summary>
        /// <param name="templateContent">Content of the template.</param>
        /// <returns>This builder</returns>
        public DeserializerSettingsBuilder WithTemplateString(string templateContent)
        {
            if (this.settings != null)
            {
                throw new InvalidDeserializerSettingsException(
                    "Cannot call WithTemplateString when another method to set the template source has already been called.");
            }

            this.settings = new StringDeserializerSettings(templateContent);
            return this;
        }
    }
}