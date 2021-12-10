namespace Firefly.CloudFormationParser.Intrinsics
{
    /// <summary>
    /// Specifies the type of the intrinsic
    /// </summary>
    public enum IntrinsicType
    {
        /// <summary>
        /// The !And intrinsic
        /// </summary>
        And,

        /// <summary>
        /// The !Base64 intrinsic
        /// </summary>
        Base64,

        /// <summary>
        /// The !Cidr intrinsic
        /// </summary>
        Cidr,

        /// <summary>
        /// The !Condition intrinsic
        /// </summary>
        Condition,

        /// <summary>
        /// The !Equals intrinsic
        /// </summary>
        Equals,

        /// <summary>
        /// The !FindInMap intrinsic
        /// </summary>
        FindInMap,

        /// <summary>
        /// The !GetAtt intrinsic
        /// </summary>
        GetAtt,

        /// <summary>
        /// The !GetAZs intrinsic
        /// </summary>
        GetAZs,

        /// <summary>
        /// The !If intrinsic
        /// </summary>
        If,

        /// <summary>
        /// The !ImportValue intrinsic
        /// </summary>
        ImportValue,

        /// <summary>
        /// The !Join intrinsic
        /// </summary>
        Join,

        /// <summary>
        /// The !Not intrinsic
        /// </summary>
        Not,

        /// <summary>
        /// The !Or intrinsic
        /// </summary>
        Or,

        /// <summary>
        /// The !Ref intrinsic
        /// </summary>
        Ref,

        /// <summary>
        /// The !Select intrinsic
        /// </summary>
        Select,

        /// <summary>
        /// The !Split intrinsic
        /// </summary>
        Split,

        /// <summary>
        /// The !Sub intrinsic
        /// </summary>
        Sub
    }
}