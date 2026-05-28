using System;

namespace Core
{
    public static class RogueCardEffectValueParser
    {
        private static readonly char[] Separators = { '|', '/', ',', ';' };

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
            if (normalizedValue.EndsWith("%", StringComparison.Ordinal))
            {
                normalizedValue = normalizedValue.Substring(0, normalizedValue.Length - 1);
            }

            return double.TryParse(normalizedValue, out value);
        }
    }
}
