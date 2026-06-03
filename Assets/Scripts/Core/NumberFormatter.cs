using System;
using System.Globalization;

namespace Core
{
    public static class NumberFormatter
    {
        private static readonly string[] Units = { string.Empty, "k", "m", "b", "t" };

        public static string FormatCompact(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "0";
            }

            double sign = value < 0d ? -1d : 1d;
            double absValue = Math.Abs(value);
            int unitIndex = 0;
            while (absValue >= 1000d && unitIndex < Units.Length - 1)
            {
                absValue /= 1000d;
                unitIndex++;
            }

            double roundedValue = Math.Round(absValue, 2, MidpointRounding.AwayFromZero);
            if (roundedValue >= 1000d && unitIndex < Units.Length - 1)
            {
                absValue /= 1000d;
                unitIndex++;
            }

            double displayValue = absValue * sign;
            return $"{displayValue.ToString("0.##", CultureInfo.InvariantCulture)}{Units[unitIndex]}";
        }

        public static string FormatSignedCompact(double value)
        {
            return value > 0d ? $"+{FormatCompact(value)}" : FormatCompact(value);
        }
    }
}
