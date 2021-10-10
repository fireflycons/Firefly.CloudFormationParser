﻿namespace Firefly.CloudFormationParser.TemplateObjects
{
#pragma warning disable 1574

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents a template parameter
    /// </summary>
    [DebuggerDisplay("Param {Name}")]
    public class Parameter : ITemplateObject, IParameter
    {
        /// <inheritdoc cref="IParameter.AllowedPattern"/>>
        [YamlMember(Order = 5)]
        public string? AllowedPattern { get; set; }

        /// <inheritdoc cref="IParameter.AllowedValues"/>>
        [YamlMember(Order = 6)]
        public List<string>? AllowedValues { get; set; }

        /// <inheritdoc cref="IParameter.ConstraintDescription"/>>
        [YamlMember(Order = 3)]
        public string? ConstraintDescription { get; set; }

        /// <inheritdoc cref="IParameter.CurrentValue"/>>
        [YamlIgnore]
        public object? CurrentValue { get; private set; }

        /// <inheritdoc cref="IParameter.Default"/>>
        [YamlMember(Order = 2)]
        public string? Default { get; set; }

        /// <inheritdoc cref="IParameter.Description"/>>
        [YamlMember(Order = 0)]
        public string? Description { get; set; }

        /// <inheritdoc cref="IParameter.MaxLength"/>>
        [YamlMember(Order = 8)]
        public int? MaxLength { get; set; }

        /// <inheritdoc cref="IParameter.MaxValue"/>>
        [YamlMember(Order = 10)]
        public double? MaxValue { get; set; }

        /// <inheritdoc cref="IParameter.MinLength"/>>
        [YamlMember(Order = 7)]
        public int? MinLength { get; set; }

        /// <inheritdoc cref="IParameter.MinValue"/>>
        [YamlMember(Order = 9)]
        public double? MinValue { get; set; }

        /// <inheritdoc cref="IParameter.Name"/>>
        [YamlIgnore]
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc cref="IParameter.NoEcho"/>>
        [YamlMember(Order = 4)]
        public bool NoEcho { get; set; }

        /// <inheritdoc cref="IParameter.Type"/>>
        [YamlMember(Order = 1)]
        public string Type { get; set; } = string.Empty;

        /// <inheritdoc cref="IParameter.GetClrType"/>>
        public Type GetClrType()
        {
            if (this.Type == string.Empty)
            {
                throw new InvalidOperationException("CloudFormation parameter must have a declared type.");
            }

            switch (this.Type)
            {
                case "Number":

                    return typeof(double);

                case "List<Number>":

                    return typeof(IEnumerable<double>);
            }

            if (this.Type == "CommaDelimitedList" || this.Type.StartsWith("List<"))
            {
                // Any remaining lists are list of AWS-Specific parameter types, which are all strings
                return typeof(IEnumerable<string>);
            }

            // Everything else is string
            return typeof(string);
        }

        /// <summary>
        /// Gets the current value of the parameter.
        /// </summary>
        /// <returns>Current value.</returns>
        public object GetCurrentValue()
        {
            if (this.CurrentValue != null)
            {
                return this.CurrentValue;
            }

            if (this.Default != null)
            {
                return ProcessParameterValue(this.Default, this.Type);
            }

            // Return default value or a default for the current parameter type
            var clrType = this.GetClrType();

            return clrType == typeof(string) ? string.Empty : Activator.CreateInstance(this.GetClrType());
        }

        /// <inheritdoc cref="IParameter.SetCurrentValue"/>>
        public void SetCurrentValue(IDictionary<string, object>? parameterValues)
        {
            if (parameterValues == null)
            {
                return;
            }

            if (this.Name == null)
            {
                throw new InvalidOperationException("Cannot set value on as yet unnamed parameter.");
            }

            string name = this.Name;

            if (!parameterValues.ContainsKey(name))
            {
                return;
            }

            var value = parameterValues[name];
            var valueType = value.GetType();

            switch (this.Type)
            {
                case "Number":

                    if (valueType.IsArray)
                    {
                        throw new InvalidCastException($"Parameter {name} - cannot assign array value to Number type");
                    }

                    if (this.AllowedValues != null && !this.AllowedValues.Contains(value.ToString()))
                    {
                        throw new ArgumentException(
                            $"Parameter {name} - Value '{value}' does not match allowed values: {string.Join(", ", this.AllowedValues)}");
                    }

                    if (!double.TryParse(value.ToString(), out var d))
                    {
                        throw new InvalidCastException(
                            $"Parameter {name} - cannot assign value of type {valueType.Name} to Number type");
                    }

                    if ((this.MinValue != null && d < this.MinValue) || (this.MaxValue != null && d > this.MaxValue))
                    {
                        throw new ArgumentException(
                            $"Parameter {name} - Value '{value}' does not match value constraint: {this.MinValue} <= x <= {this.MaxValue}");
                    }

                    this.CurrentValue = d;
                    break;

                case "List<Number>":

                    var numericArrayTypes = new List<Type>
                                                {
                                                    typeof(IEnumerable<byte>),
                                                    typeof(IEnumerable<sbyte>),
                                                    typeof(IEnumerable<decimal>),
                                                    typeof(IEnumerable<double>),
                                                    typeof(IEnumerable<short>),
                                                    typeof(IEnumerable<int>),
                                                    typeof(IEnumerable<long>),
                                                    typeof(IEnumerable<ushort>),
                                                    typeof(IEnumerable<uint>),
                                                    typeof(IEnumerable<ulong>)
                                                };

                    if (numericArrayTypes.Contains(valueType))
                    {
                        var values = new List<double>();

                        foreach (var obj in (IEnumerable)value)
                        {
                            try
                            {
                                values.Add(Convert.ToDouble(obj));
                            }
                            catch (InvalidCastException e)
                            {
                                throw new InvalidCastException(
                                    $"Parameter {name} - cannot assign value of type {obj.GetType().Name} to Number type",
                                    e);
                            }
                        }

                        this.CurrentValue = values;
                    }
                    else if (double.TryParse(value.ToString(), out d))
                    {
                        this.CurrentValue = new List<double> { d };
                    }
                    else
                    {
                        throw new InvalidCastException(
                            $"Parameter {name} - cannot assign value of type {valueType.Name} to Number type");
                    }

                    break;

                case "String":

                    if (valueType.IsArray)
                    {
                        throw new InvalidOperationException(
                            $"Parameter {name} - cannot assign array value to String type");
                    }

                    var stringVal = value.ToString();

                    // ReSharper disable once AssignNullToNotNullAttribute - checked by IsNullOrEmpty
                    if (!string.IsNullOrEmpty(this.AllowedPattern) && !Regex.IsMatch(stringVal, this.AllowedPattern))
                    {
                        throw new ArgumentException(
                            $"Parameter {name} - Value '{stringVal}' does not match pattern: {this.AllowedPattern}");
                    }

                    if (this.AllowedValues != null && !this.AllowedValues.Contains(stringVal))
                    {
                        throw new ArgumentException(
                            $"Parameter {name} - Value '{stringVal}' does not match allowed values: {string.Join(", ", this.AllowedValues)}");
                    }

                    this.CurrentValue = value.ToString();
                    break;

                case "CommaDelimitedList":

                    this.CurrentValue = ((string)value).Split(',').Select(s => s.Trim()).ToList();
                    break;

                default: // Remaining List<*>, which are all string lists

                    if (value is IEnumerable enumerable)
                    {
                        var values = enumerable.ToList().Cast<string>().ToList();

                        this.CurrentValue = values;
                    }

                    break;
            }
        }

        /// <summary>
        /// Processes a parameter value doing a split when the type is Comma-delimited list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterType">Type of the parameter.</param>
        /// <returns>The processed value.</returns>
        private static object ProcessParameterValue(object value, string parameterType)
        {
            return parameterType == "CommaDelimitedList"
                       ? value.ToString().Split(',').Select(s => s.Trim()).ToList()
                       : value;
        }
    }
}