namespace Firefly.CloudFormationParser
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.TemplateObjects;

    /// <summary>
    /// Interface describing a CloudFormation Resource.
    /// </summary>
    public interface IResource : ITemplateObject
    {
        /// <summary>
        /// <para>
        /// Gets or sets the resource's condition.
        /// </para>
        /// <para>
        /// When present, associates this output with a condition defined in the <c>Conditions</c> section of the template.
        /// </para>
        /// </summary>
        /// <value>
        /// The CloudFormation condition which will be <c>null</c> if the template did not provide this property.
        /// </value>
        string? Condition { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the resource's description.
        /// </para>
        /// <para>
        /// A string type that describes the output value. The value for the description declaration must be a literal string that's between 0 and 1024 bytes in length.
        /// </para> 
        /// </summary>
        /// <value>
        /// The resource's description which will be <c>null</c> if the template did not provide this property.
        /// </value>
        object? Description { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the resource type.
        /// </para>
        /// The resource type identifies the type of resource that you are declaring. For example, AWS::EC2::Instance declares an EC2 instance.
        /// <para>
        /// </para>
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        string? Type { get; set; }

        /// <summary>
        /// Gets or sets the resource version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        /// <remarks>
        /// This property is valid for custom resources.
        /// </remarks>
        string? Version { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the resource properties.
        /// </para>
        /// <para>
        /// Resource properties are additional options that you can specify for a resource.
        /// For example, for each EC2 instance, you must specify an Amazon Machine Image (AMI) ID for that instance.
        /// </para>
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        Dictionary<string, object>? Properties { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the metadata.
        /// </para>
        /// <para>
        /// You can use the optional Metadata section to include arbitrary JSON or YAML objects that provide details about the resource.
        /// Often used with instances or launch templates to provide data for <c>cfn-init</c>.
        /// </para>
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the creation policy.
        /// </summary>
        /// <value>
        /// The creation policy.
        /// </value>
        Dictionary<string, object>? CreationPolicy { get; set; }

        /// <summary>
        /// Gets or sets the update policy.
        /// </summary>
        /// <value>
        /// The update policy.
        /// </value>
        Dictionary<string, object>? UpdatePolicy { get; set; }

        /// <summary>
        /// Gets or sets the deletion policy.
        /// </summary>
        /// <value>
        /// The deletion policy.
        /// </value>
        string? DeletionPolicy { get; set; }

        /// <summary>
        /// Gets or sets the update replace policy.
        /// </summary>
        /// <value>
        /// The update replace policy.
        /// </value>
        string? UpdateReplacePolicy { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets explicit dependencies on other resources in the template.
        /// </para>
        /// <para>
        /// You should use the convenience property <see cref="ExplicitDependencies"/> to get the list of dependencies.
        /// </para>
        /// </summary>
        /// <value>
        /// The dependencies which will be <c>null</c> if the template did not provide this property. 
        /// </value>
        object? DependsOn { get; set; }

        /// <summary>
        /// Gets the explicit dependencies on other resources in the template.
        /// </summary>
        /// <value>
        /// A list of explicit dependencies.
        /// </value>
        IEnumerable<string> ExplicitDependencies { get; }

        /// <summary>
        /// Gets a resource property value.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The value of the property; else <c>null</c> if the property path was not found.</returns>
        object? GetResourcePropertyValue(string propertyPath);


        /// <summary>
        /// <para>
        /// Updates a property of this resource.
        /// </para>
        /// <para>
        /// You would want to do this if you were implementing the functionality of <c>aws cloudformation package</c>
        /// to rewrite local file paths to S3 object locations.
        /// </para>
        /// </summary>
        /// <param name="propertyPath">Path to the property you want to set within this resource's <c>Properties</c> section,
        /// e.g. for a <c>AWS::Glue::Job</c> the path would be <c>Command.ScriptLocation</c>.
        /// </param>
        /// <param name="newValue">The new value.</param>
        /// <exception cref="FormatException">Resource format is unknown (not JSON or YAML)</exception>
        void UpdateResourceProperty(string propertyPath, object newValue);
    }
}