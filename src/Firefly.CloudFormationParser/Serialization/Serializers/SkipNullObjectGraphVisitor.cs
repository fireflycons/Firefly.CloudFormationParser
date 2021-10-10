namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.ObjectGraphVisitors;

    /// <summary>
    /// Suppress emission of properties that have a null value
    /// </summary>
    /// <seealso cref="YamlDotNet.Serialization.ObjectGraphVisitors.ChainedObjectGraphVisitor" />
    internal class SkipNullObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkipNullObjectGraphVisitor"/> class.
        /// </summary>
        /// <param name="nextVisitor">The next visitor.</param>
        public SkipNullObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        {
        }

        /// <summary>
        /// Determines whether the element should be entered and emitted 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if the object should be serialized, else <c>false</c></returns>
        public override bool Enter(IObjectDescriptor value, IEmitter context)
        {
            return !IsNullValue(value) && base.Enter(value, context);
        }

        /// <summary>
        /// Determines whether the mapping should be entered and emitted 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if the object should be serialized, else <c>false</c></returns>
        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            return !IsNullValue(value) && base.EnterMapping(key, value, context);
        }

        /// <summary>
        /// Determines whether [is null value] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is null value] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNullValue(IObjectDescriptor value)
        {
            return value.Value == null;
        }
    }
}