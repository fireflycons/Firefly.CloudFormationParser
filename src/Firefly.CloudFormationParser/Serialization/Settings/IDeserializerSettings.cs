namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Firefly.CloudFormationParser.TemplateObjects;

    /// <summary>
    /// Defines properties required by the CloudFormation deserializer.
    /// This can be implemented to provide deserialization from other sources e.g. a CloudFormation Stack or S3 bucket.
    /// </summary>
    public interface IDeserializerSettings : IDisposable
    {
        /// <summary>
        /// <para>
        /// Gets a value indicating whether to exclude resources and outputs nullified by the evaluation of conditions.
        /// </para>
        /// <para>
        /// When this is <c>true</c>, the results of the evaluation of conditions in the <c>Conditions</c> block of the template using the values of
        /// the parameters as supplied by <see cref="ParameterValues"/> are used to eliminate any resource or output where its <c>Condition</c>
        /// property evaluates to <c>false</c> from the template object returned by the call to <see cref="Template.Deserialize"/>
        /// </para>
        /// </summary>
        /// <value>
        ///   <c>true</c> if conditional objects should be excluded; otherwise, <c>false</c>.
        /// </value>
        bool ExcludeConditionalResources { get; }

        /// <summary>
        /// Gets an optional dictionary of values to assign to parameters.
        /// </summary>
        /// <value>
        /// The parameter values.
        /// </value>
        IDictionary<string, object>? ParameterValues { get; }

        /// <summary>
        /// <para>
        /// Gets the template content.
        /// </para>
        /// <para>
        /// Implementations should prepare the template data as a reader from the information passed to the implementation's constructor.
        /// It is the job of the implementation to dispose of nay resources involved in this operation after the template has been read.
        /// </para>
        /// </summary>
        /// <returns>A reader positioned at the start of the template to parse.</returns>
        Task<TextReader> GetContentAsync();
    }
}