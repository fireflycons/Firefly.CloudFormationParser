namespace Firefly.CloudFormationParser.Serialization.Settings
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception class thrown for invalid combination of settings in <see cref="DeserializerSettingsBuilder"/>
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class InvalidDeserializerSettingsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDeserializerSettingsException"/> class.
        /// </summary>
        public InvalidDeserializerSettingsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDeserializerSettingsException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidDeserializerSettingsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDeserializerSettingsException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public InvalidDeserializerSettingsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDeserializerSettingsException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected InvalidDeserializerSettingsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}