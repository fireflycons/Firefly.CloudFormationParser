namespace Firefly.CloudFormationParser.TemplateObjects
{
#pragma warning disable 1574

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Firefly.CloudFormationParser.Utils;

    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Represents a template parameter
    /// </summary>
    [DebuggerDisplay("Param {Name}")]
    public class Parameter : IParameter
    {
        /// <summary>
        /// The allowed pattern regex.
        /// </summary>
        private Regex? allowedPattern;

        /// <inheritdoc cref="IParameter.AllowedPattern"/>>
        [YamlIgnore]
        public Regex? AllowedPattern => this.allowedPattern;

        /// <summary>
        /// <para>
        /// Gets or sets a regular expression that represents the patterns to allow for String types. The pattern must match the entire parameter value provided.
        /// </para>
        /// <para>
        /// Use the convenience property <see cref="AllowedPattern"/> to access the pattern as a regular expression.
        /// </para>
        /// </summary>
        /// <value>
        /// The allowed pattern, which will be <c>null</c> if not defined in the template.
        /// </value>
        [YamlMember(Order = 5, Alias = "AllowedPattern", ScalarStyle = ScalarStyle.SingleQuoted, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string? AllowedPatternString
        {
            get => this.allowedPattern?.ToString();

            set => this.allowedPattern = value == null ? null : new Regex(value);
        }

        /// <inheritdoc cref="IParameter.AllowedValues"/>>
        [YamlMember(Order = 6, DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitEmptyCollections)]
        public List<string>? AllowedValues { get; set; }

        /// <inheritdoc cref="IParameter.ConstraintDescription"/>>
        [YamlMember(Order = 3, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string? ConstraintDescription { get; set; }

        /// <inheritdoc cref="IParameter.CurrentValue"/>>
        [YamlIgnore]
        public object? CurrentValue { get; private set; }

        /// <inheritdoc cref="IParameter.Default"/>>
        [YamlMember(Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string? Default { get; set; }

        /// <inheritdoc cref="IParameter.Description"/>>
        [YamlMember(Order = 0, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string? Description { get; set; }

        /// <inheritdoc cref="IParameter.MaxLength"/>>
        [YamlMember(Order = 8, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public int? MaxLength { get; set; }

        /// <inheritdoc cref="IParameter.MaxValue"/>>
        [YamlMember(Order = 10, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public double? MaxValue { get; set; }

        /// <inheritdoc cref="IParameter.MinLength"/>>
        [YamlMember(Order = 7, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public int? MinLength { get; set; }

        /// <inheritdoc cref="IParameter.MinValue"/>>
        [YamlMember(Order = 9, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public double? MinValue { get; set; }

        /// <inheritdoc cref="IParameter.Name"/>>
        [YamlIgnore]
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc cref="IParameter.NoEcho"/>>
        [YamlMember(Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public bool NoEcho { get; set; }

        /// <inheritdoc cref="IParameter.Type"/>>
        [YamlMember(Order = 1)]
        public string Type { get; set; } = string.Empty;

        /// <inheritdoc cref="IParameter.HasMinLength"/>
        [YamlIgnore]
        public bool HasMinLength => this.MinLength != null;

        /// <inheritdoc cref="IParameter.HasMinValue"/>
        [YamlIgnore]
        public bool HasMinValue => this.MinValue != null;

        /// <inheritdoc cref="IParameter.HasMaxLength"/>
        [YamlIgnore]
        public bool HasMaxLength => this.MaxLength != null;

        /// <inheritdoc cref="IParameter.HasMaxValue"/>
        [YamlIgnore]
        public bool HasMaxValue => this.MaxValue != null;

        /// <inheritdoc cref="IParameter.IsSsmParameter"/>
        [YamlIgnore]
        public bool IsSsmParameter => this.Type.StartsWith("AWS::SSM::Parameter");

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
                    if (this.AllowedPattern != null && !this.AllowedPattern.IsMatch(stringVal))
                    {
                        throw new ArgumentException(
                            $"Parameter {name} - Value '{stringVal}' does not match pattern: {this.AllowedPatternString}");
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