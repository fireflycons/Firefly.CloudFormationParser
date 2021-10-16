namespace Firefly.CloudFormationParser.Serialization.Settings
{
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
    /// This class reads a template from an existing CloudFormation stack, and populates <see cref="IDeserializerSettings.ParameterValues"/>
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
    internal class CfnStackDeserializerSettings : AbstractDeserializerSettings
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
        public override async Task<TextReader> GetContentAsync()
        {
            var templateSummaryResponse =
                await this.client.GetTemplateSummaryAsync(new GetTemplateSummaryRequest { StackName = this.stackId });

            var noEchoParameters = templateSummaryResponse.Parameters.Where(p => p.NoEcho).Select(p => p.ParameterKey)
                .ToList();

            var stackResponse =
                (await this.client.DescribeStacksAsync(new DescribeStacksRequest { StackName = this.stackId })).Stacks
                .First();

            // Merge stack parameters with user ones.
            // Overwrite user ones from stack, but omit any NoEcho ones from 
            // stack as these will be undefined.
            var mergeParameters = stackResponse.Parameters.Where(p => !noEchoParameters.Contains(p.ParameterKey))
                .ToDictionary(p => p.ParameterKey, p => (object)p.ParameterValue);

            foreach (var pk in noEchoParameters)
            {
                if (this.ParameterValues.All(kv => kv.Key != pk))
                {
                    // Add fixed value for unreferenced NoEco params
                    mergeParameters.Add(pk, "UNRESOLVED-NOECHO");
                }
            }

            foreach (var kv in mergeParameters)
            {
                this.ParameterValues[kv.Key] = kv.Value;
            }

            var templateResponse = await this.client.GetTemplateAsync(
                                       new GetTemplateRequest
                                           {
                                               StackName = this.stackId, TemplateStage = TemplateStage.Processed
                                           });

            this.reader = new StringReader(templateResponse.TemplateBody);

            return this.reader;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this.reader?.Dispose();
        }
    }
}