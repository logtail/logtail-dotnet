using System;
using System.Collections.Generic;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Layouts;
using NLog.MessageTemplates;

namespace Logtail.NLog
{
    public class ColorValueFormatter : IValueFormatter
    {
        IValueFormatter valueFormatter;

        private const string AnsiRed = "\x1b[31;1m";
        private const string AnsiGreen = "\x1b[32;1m";
        private const string AnsiYellow = "\x1b[33;1m";
        private const string AnsiBlue = "\x1b[34;1m";
        private const string AnsiCyan = "\x1b[36;1m";
        private const string AnsiGray = "\x1b[37;1m";
        private const string AnsiReset = "\x1b[0m";

        public ColorValueFormatter(IValueFormatter valueFormatter = null)
        {
            this.valueFormatter = valueFormatter != null ? valueFormatter : ConfigurationItemFactory.Default.ValueFormatter;
        }

        public bool FormatValue(
            Object value,
            string format,
            CaptureType captureType,
            System.IFormatProvider formatProvider,
            System.Text.StringBuilder builder
        ) {
            System.Text.StringBuilder innerBuilder = new System.Text.StringBuilder();

            bool isSerializable = valueFormatter.FormatValue(value, format, captureType, formatProvider, innerBuilder);

            if (innerBuilder.Length > 0) {
                builder.Append(StartColor(value));
                builder.Append(innerBuilder.ToString());
                builder.Append(AnsiReset);
            }

            return isSerializable;
        }

        string StartColor(Object value) {
            if (value is string || value is char) {
                return AnsiCyan;
            }
            if (value is int || value is uint || value is long || value is ulong) {
                return AnsiYellow;
            }
            if (value is bool boolValue) {
                return boolValue ? AnsiGreen : AnsiRed;
            }
            if (value is null) {
                return AnsiGray;
            }

            return AnsiBlue;
        }
    }
}
