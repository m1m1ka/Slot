using System;

namespace Core
{
    public static class RogueCardEffectValueParser
    {
        private static readonly char[] Separators = { '|', '/', ',', '，', ';', '；' };

        public static string[] Split(string expression)
        {
            return string.IsNullOrWhiteSpace(expression)
                ? Array.Empty<string>()
                : expression.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static bool TryParseNumber(string rawValue, out double value)
        {
            value = 0d;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            string normalizedValue = rawValue.Trim();
            bool isPercent = normalizedValue.EndsWith("%", StringComparison.Ordinal);
            if (isPercent)
            {
                normalizedValue = normalizedValue.Substring(0, normalizedValue.Length - 1);
            }

            if (!double.TryParse(normalizedValue, out value))
            {
                return false;
            }

            if (isPercent)
            {
                value /= 100d;
            }

            return true;
        }
    }
}
