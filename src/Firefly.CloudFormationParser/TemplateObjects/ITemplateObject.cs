namespace Firefly.CloudFormationParser.TemplateObjects
{
    /// <summary>
    /// Interface describing a template object
    /// </summary>
    public interface ITemplateObject
    {
        /// <summary>
        /// <para>
        /// Gets or sets the name of the object
        /// </para>
        /// <para>
        /// Names are set on these objects for convenience by the post-processing stage,
        /// as they are not part of the object definition as per CloudFormation.
        /// They are set as follows:
        /// <list type="bullet">
        /// <item>
        /// <description>Parameter - The parameter name</description>
        /// </item>
        /// <item>
        /// <description>Resource - The logical ID</description>
        /// </item>
        /// <item>
        /// <description>Output - The output name</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }
    }
}