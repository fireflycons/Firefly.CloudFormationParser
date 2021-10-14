namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Amazon.CloudFormation;
    using Amazon.CloudFormation.Model;

    /// <summary>
    /// <para>
    /// Deserializer settings for reading a stack template directly from the CloudFormation API, i.e. the template of a deployed CloudFormation Stack.
    /// </para>
    /// <para>
    /// This class reads a template from an existing CloudFormation stack, and populates <see cref="CfnStackDeserializerSettings.ParameterValues"/>
    /// with the current values of the stack's parameters as set by the most recent update. Parameters that were declared with <c>NoEcho</c>
    /// will receive undefined values.
    /// </para>
    /// </summary>
    /// <example>
    /// How to deserialize a template given a stack name or ARN
    /// <code>
    /// public async Task&lt;ITemplate&gt; ReadFromCloudFormation(
    ///     IAmazonCloudFormation client,
    ///     string stackNameOrARN)
    /// {
    ///     using var settings = new CfnStackDeserializerSettings(client, stackNameOrARN);
    ///     return await Template.Deserialize(settings); 
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// How to deserialize a template given a stack name or ARN and omit resources based on condition evaluation.
    /// Note that parameter values are read from the deployed stack and used in the condition evaluation.
    /// <code>
    /// public async Task&lt;ITemplate&gt; ReadFromCloudFormation(
    ///     IAmazonCloudFormation client,
    ///     string stackNameOrARN)
    /// {
    ///     using var settings = new CfnStackDeserializerSettings(client, stackNameOrARN)
    ///         {
    ///             ExcludeConditionalResources = true
    ///         };
    /// 
    ///     return await Template.Deserialize(settings); 
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IDeserializerSettings" />
    public class CfnStackDeserializerSettings : IDeserializerSettings
    {
        /// <summary>
        /// The client
        /// </summary>
        private readonly IAmazonCloudFormation client;

        /// <summary>
        /// The stack identifier
        /// </summary>
        private readonly string stackId;

        /// <summary>
        /// The reader on the stream which wee return from <see cref="GetContentAsync"/>
        /// </summary>
        private TextReader? reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="CfnStackDeserializerSettings"/> class.
        /// </summary>
        /// <param name="client">An AWS CloudFormation client with which to read the given stack.</param>
        /// <param name="stackId">Name or ARN of stack to read template from.</param>
        public CfnStackDeserializerSettings(IAmazonCloudFormation client, string stackId)
        {
            this.stackId = stackId;
            this.client = client;
        }

        /// <inheritdoc />
        public bool ExcludeConditionalResources { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets an optional dictionary of values to assign to parameters.
        /// </para>
        /// <para>
        /// If this is unset at the time <see cref="GetContentAsync"/> is called, it will be populated
        /// with the current values of the parameters as defined by the deployed stack. Note that for
        /// parameters that were declared <c>NoEcho</c>, the values of these parameters will be undefined.
        /// </para>
        /// </summary>
        /// <value>
        /// The parameter values.
        /// </value>
        public IDictionary<string, object>? ParameterValues { get; set; }

        /// <inheritdoc />
        public async Task<TextReader> GetContentAsync()
        {
            if (this.ParameterValues == null)
            {
                var stackRespose =
                    (await this.client.DescribeStacksAsync(new DescribeStacksRequest { StackName = this.stackId }))
                    .Stacks.First();

                this.ParameterValues = stackRespose.Parameters.ToDictionary(
                    p => p.ParameterKey,
                    p => (object)p.ParameterValue);
            }

            var templateResponse = await this.client.GetTemplateAsync(
                                       new GetTemplateRequest
                                           {
                                               StackName = this.stackId,
                                               TemplateStage = TemplateStage.Processed
                                           });

            this.reader = new StringReader(templateResponse.TemplateBody);

            return this.reader;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.reader?.Dispose();
        }
    }
}