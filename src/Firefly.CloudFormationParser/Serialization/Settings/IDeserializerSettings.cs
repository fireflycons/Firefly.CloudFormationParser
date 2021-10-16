namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Amazon.S3;

    using Firefly.CloudFormationParser.TemplateObjects;

    /// <summary>
    /// <para>
    /// Defines properties required by the CloudFormation deserializer.
    /// </para>
    /// <para>
    /// An object inherited from this interface is returned by a call to <see cref="DeserializerSettingsBuilder.Build">DeserializerSettingsBuilder.Build</see>.
    /// You should dispose this object following template deserialization to free any resources that are internally allocated such as
    /// streams and readers.
    /// </para>
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
        /// property evaluates to <c>false</c> from the template object returned by the call to <see cref="Template.Deserialize">Template.Deserialize</see>
        /// </para>
        /// </summary>
        /// <value>
        ///   <c>true</c> if conditional objects should be excluded; otherwise, <c>false</c>.
        /// </value>
        bool ExcludeConditionalResources { get; }

        /// <summary>
        /// <para>
        /// Gets an optional dictionary of values to assign to parameters.
        /// </para>
        /// <para>
        /// If left unset, then parameter defaults where present will be used in evaluations, or where there is no
        /// declared default value, the default for the type - empty string, zero for number etc.
        /// </para>
        /// <para>
        /// When deserializing directly from an existing CloudFormation Stack, current values of parameters will be retrieved
        /// from the stack and populated here. The API will not return values for parameters flagged <c>NoEcho</c>, therefore
        /// if you want to be able to evaluate values for these parameters, pass a dictionary of the values you want to use to
        /// <see cref="DeserializerSettingsBuilder.WithParameterValues">DeserializerSettingsBuilder.WithParameterValues</see>, and these will be merged with the remaining parameters
        /// from the stack. 
        /// </para>
        /// </summary>
        /// <value>
        /// The parameter values.
        /// </value>
        IDictionary<string, object> ParameterValues { get; }

        /// <summary>
        /// Gets the S3 client to use for any implied S3 operations, such as retrieving nested stacks or includes.
        /// </summary>
        /// <value>
        /// The S3 client.
        /// </value>
        IAmazonS3? S3Client { get; }

        /// <summary>
        /// <para>
        /// Gets the template content. This should be implemented as an <c>async</c> method.
        /// </para>
        /// <para>
        /// Implementations should prepare the template data as a reader from the information passed to the implementation's constructor.
        /// It is the job of the implementation to dispose of any resources involved in this operation after the template has been read.
        /// </para>
        /// </summary>
        /// <returns>A reader positioned at the start of the template to parse.</returns>
        Task<TextReader> GetContentAsync();
    }
}