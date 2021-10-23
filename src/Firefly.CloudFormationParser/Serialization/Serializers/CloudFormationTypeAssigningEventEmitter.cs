namespace Firefly.CloudFormationParser.Serialization.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.EventEmitters;
    using YamlDotNet.Serialization.Schemas;

    internal class CloudFormationTypeAssigningEventEmitter : ChainedEventEmitter
    {
        private static readonly FieldInfo EventsField = typeof(Emitter).GetField(
            "events",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo StateField = typeof(Emitter).GetField(
            "state",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo CheckSimpleKeyMethod = typeof(Emitter).GetMethod(
            "CheckSimpleKey",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private enum EmitterState
        {
            StreamStart,
            StreamEnd,
            FirstDocumentStart,
            DocumentStart,
            DocumentContent,
            DocumentEnd,
            FlowSequenceFirstItem,
            FlowSequenceItem,
            FlowMappingFirstKey,
            FlowMappingKey,
            FlowMappingSimpleValue,
            FlowMappingValue,
            BlockSequenceFirstItem,
            BlockSequenceItem,
            BlockMappingFirstKey,
            BlockMappingKey,
            BlockMappingSimpleValue,
            BlockMappingValue
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFormationTypeAssigningEventEmitter"/> class.
        /// </summary>
        /// <param name="nextEmitter">The next emitter.</param>
        public CloudFormationTypeAssigningEventEmitter(IEventEmitter nextEmitter)
            : base(nextEmitter)
        {
        }

        /// <summary>
        /// Emits the specified event information.
        /// </summary>
        /// <param name="eventInfo">The event information.</param>
        /// <param name="emitter">The emitter.</param>
        /// <exception cref="System.NotSupportedException">TypeCode.{typeCode} is not supported.</exception>
        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            var suggestedStyle = ScalarStyle.Plain;

            var value = eventInfo.Source.Value;

            if (value == null)
            {
                eventInfo.Tag = JsonSchema.Tags.Null;
                eventInfo.RenderedValue = string.Empty;
            }
            else
            {
                var typeCode = Type.GetTypeCode(eventInfo.Source.Type);
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        eventInfo.Tag = JsonSchema.Tags.Bool;
                        eventInfo.RenderedValue = YamlFormatter.FormatBoolean(value);
                        break;

                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        eventInfo.Tag = JsonSchema.Tags.Int;
                        eventInfo.RenderedValue = YamlFormatter.FormatNumber(value);
                        break;

                    case TypeCode.Single:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = YamlFormatter.FormatNumber((float)value);
                        break;

                    case TypeCode.Double:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = YamlFormatter.FormatNumber((double)value);
                        break;

                    case TypeCode.Decimal:
                        eventInfo.Tag = JsonSchema.Tags.Float;
                        eventInfo.RenderedValue = YamlFormatter.FormatNumber(value);
                        break;

                    case TypeCode.String:
                    case TypeCode.Char:
                        eventInfo.Tag = FailsafeSchema.Tags.Str;
                        eventInfo.RenderedValue = value.ToString()!;

                        // Use dirty hack to determine if a key is being emitted.
                        if (emitter.IsEmittingKey() && eventInfo.RenderedValue.All(char.IsDigit))
                        {
                            // Emit numeric keys quoted.
                            suggestedStyle = ScalarStyle.SingleQuoted;
                        }
                        else
                        {
                            suggestedStyle = ScalarStyle.Any;
                        }

                        break;

                    case TypeCode.DateTime:
                        eventInfo.Tag = DefaultSchema.Tags.Timestamp;
                        eventInfo.RenderedValue = YamlFormatter.FormatDateTime(value);
                        break;

                    case TypeCode.Empty:
                        eventInfo.Tag = JsonSchema.Tags.Null;
                        eventInfo.RenderedValue = string.Empty;
                        break;

                    default:
                        if (eventInfo.Source.Type == typeof(TimeSpan))
                        {
                            eventInfo.RenderedValue = YamlFormatter.FormatTimeSpan(value);
                            break;
                        }

                        throw new NotSupportedException($"TypeCode.{typeCode} is not supported.");
                }
            }

            eventInfo.IsPlainImplicit = true;
            if (eventInfo.Style == ScalarStyle.Any)
            {
                eventInfo.Style = suggestedStyle;
            }

            base.Emit(eventInfo, emitter);
        }

        private static class YamlFormatter
        {
            private static readonly NumberFormatInfo NumberFormat = new NumberFormatInfo
                                                                        {
                                                                            CurrencyDecimalSeparator = ".",
                                                                            CurrencyGroupSeparator = "_",
                                                                            CurrencyGroupSizes = new[] { 3 },
                                                                            CurrencySymbol = string.Empty,
                                                                            CurrencyDecimalDigits = 99,
                                                                            NumberDecimalSeparator = ".",
                                                                            NumberGroupSeparator = "_",
                                                                            NumberGroupSizes = new[] { 3 },
                                                                            NumberDecimalDigits = 99,
                                                                            NaNSymbol = ".nan",
                                                                            PositiveInfinitySymbol = ".inf",
                                                                            NegativeInfinitySymbol = "-.inf"
                                                                        };

            public static string FormatBoolean(object boolean)
            {
                return boolean.Equals(true) ? "true" : "false";
            }

            public static string FormatDateTime(object dateTime)
            {
                return ((DateTime)dateTime).ToString("o", CultureInfo.InvariantCulture);
            }

            public static string FormatNumber(object number)
            {
                return Convert.ToString(number, NumberFormat)!;
            }

            public static string FormatNumber(double number)
            {
                return number.ToString("G17", NumberFormat);
            }

            public static string FormatNumber(float number)
            {
                return number.ToString("G17", NumberFormat);
            }

            public static string FormatTimeSpan(object timeSpan)
            {
                return ((TimeSpan)timeSpan).ToString();
            }
        }
    }
}