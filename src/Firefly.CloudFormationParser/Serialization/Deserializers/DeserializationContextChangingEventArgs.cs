namespace Firefly.CloudFormationParser.Serialization.Deserializers
{
    using System;

    internal class DeserializationContextChangingEventArgs : EventArgs
    {
        public DeserializationContext CurrentContext { get; set; }
    }
}