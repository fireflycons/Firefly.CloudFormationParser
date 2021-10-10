﻿namespace Firefly.CloudFormationParser
{
    using System;
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Serialization.Settings;
    using Firefly.CloudFormationParser.TemplateObjects;

    /// <summary>
    /// Interface describing a CloudFormation Parameter.
    /// </summary>
    public interface IParameter : ITemplateObject
    {
        /// <summary>
        /// Gets or sets a regular expression that represents the patterns to allow for String types. The pattern must match the entire parameter value provided.
        /// </summary>
        /// <value>
        /// The allowed pattern, which will be <c>null</c> if not defined in the template.
        /// </value>
        string? AllowedPattern { get; set; }

        /// <summary>
        /// Gets or sets an array containing the list of values allowed for the parameter.
        /// </summary>
        /// <value>
        /// The allowed values, which will be <c>null</c> if not defined in the template.
        /// </value>
        List<string>? AllowedValues { get; set; }

        /// <summary>
        /// Gets or sets a string that explains a constraint when the constraint is violated..
        /// </summary>
        /// <value>
        /// The constraint description, which will be <c>null</c> if not defined in the template.
        /// </value>
        string? ConstraintDescription { get; set; }

        /// <summary>
        /// Gets or sets a value of the appropriate type for the template to use if no value is specified when a stack is created.
        /// If you define constraints for the parameter, you must specify a value that adheres to those constraints.
        /// </summary>
        /// <value>
        /// The default value, which will be <c>null</c> if not defined in the template.
        /// </value>
        string? Default { get; set; }

        /// <summary>
        /// Gets or sets a string of up to 4000 characters that describes the parameter.
        /// </summary>
        /// <value>
        /// Description of the parameter, which will be <c>null</c> if not defined in the template.
        /// </value>
        string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to mask the parameter value to prevent it from being displayed in the console, command line tools, or API.
        /// </summary>
        /// <value>
        /// If <c>true</c>, user interfaces should redact the parameter value when displaying.
        /// </value>
        bool NoEcho { get; set; }

        /// <summary>
        /// Gets or sets the data type for the parameter.
        /// </summary>
        /// <value>
        /// The AWS Parameter type.
        /// </value>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets a numeric value that determines the smallest numeric value you want to allow for Number types.
        /// </summary>
        /// <value>
        /// The minimum value, which will be <c>null</c> if not defined in the template.
        /// </value>
        double? MinValue { get; set; }

        /// <summary>
        /// Gets or sets a numeric value that determines the largest numeric value you want to allow for Number types.
        /// </summary>
        /// <value>
        /// The maximum value, which will be <c>null</c> if not defined in the template.
        /// </value>
        double? MaxValue { get; set; }

        /// <summary>
        /// Gets or sets an integer value that determines the smallest number of characters you want to allow for String types.
        /// </summary>
        /// <value>
        /// The minimum length.
        /// </value>
        int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets an integer value that determines the largest number of characters you want to allow for String types.
        /// </summary>
        /// <value>
        /// The maximum length, which will be <c>null</c> if not defined in the template.
        /// </value>
        int? MaxLength { get; set; }

        /// <summary>
        /// <para>
        /// Gets the current value of this parameter
        /// </para>
        /// <para>
        /// This value may be set by passing a <see cref="Dictionary{TKey,TValue}"/> of string,object to <see cref="IDeserializerSettings.ParameterValues"/> method.
        /// </para>
        /// </summary>
        /// <value>
        /// The current value, which will be <c>null</c> if not set by passing a values dictionary to the deserializer.
        /// </value>
        object? CurrentValue { get; }

        /// <summary>
        /// Gets the CLR type for the parameter given the AWS type specified for it in the template.
        /// </summary>
        /// <returns>CLR type</returns>
        Type GetClrType();

        /// <summary>
        /// Gets the current value of the parameter choosing whatever value has been set and if none, the default.
        /// </summary>
        /// <returns>Current value.</returns>
        object GetCurrentValue();

        /// <summary>
        /// Sets the current value of this parameter to use in evaluations.
        /// </summary>
        /// <param name="parameterValues">The parameter values.</param>
        /// <exception cref="System.InvalidCastException">
        /// <list type="bullet">
        /// <item>
        /// <description>Parameter <i>Name</i> - cannot assign array value to Number type</description>
        /// </item>
        /// <item>
        /// <description>Parameter <i>Name</i> - cannot assign value of type <i>type</i> to Number type</description>
        /// </item>
        /// <item>
        /// <description>Parameter <i>Name</i> - cannot assign value of type <i>type</i> to Number type</description>
        /// </item>
        /// <item>
        /// <description>Parameter <i>Name</i> - cannot assign value of type <i>type</i> to Number type</description>
        /// </item>
        /// <item>
        /// <description>Parameter <i>Name</i> - cannot assign array value to String type</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <list type="bullet">
        /// <item>
        /// <description>Parameter <i>Name</i> - Value '<i>value</i>' does not match allowed values: <i>string.Join(", ", AllowedValues)</i></description>
        /// </item>
        /// <item>
        /// <description>Parameter <i>Name</i> - Value '<i>value</i>' does not match value constraint: <i>MinValue</i> &lt;= x &lt;= <i>MaxValue</i></description>
        /// </item>
        /// <item>
        /// <description>Parameter <i>Name</i> - Value '<i>stringVal</i>' does not match pattern: <i>AllowedPattern</i></description>
        /// </item>
        /// <item>
        /// <description>Parameter <i>Name</i> - Value '<i>stringVal</i>' does not match allowed values: <i>AllowedValues</i></description>
        /// </item>
        /// </list>
        /// </exception>
        void SetCurrentValue(IDictionary<string, object>? parameterValues);
    }
}