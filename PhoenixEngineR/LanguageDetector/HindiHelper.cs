﻿using System.Linq;
using System.Text.RegularExpressions;

namespace PhoenixEngine.LanguageDetector
{
    public static class HindiHelper
    {
        private static readonly Regex HindiCharRegex = new Regex(@"\p{IsDevanagari}", RegexOptions.Compiled);

        private static readonly string[] HindiKeywords = new[]
        {
        "है", "में", "के", "और", "का", "की", "से", "यह", "मैं", "तुम", "यहाँ"
    };

        public static bool IsProbablyHindi(string input, int keywordThreshold=2)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            int keywordHits = 0;
            for (int i = 0; i < HindiKeywords.Length; i++)
            {
                if (input.Contains(HindiKeywords[i]))
                    keywordHits++;
            }

            bool hasHindiChars = HindiCharRegex.IsMatch(input);

            return hasHindiChars && keywordHits >= keywordThreshold;
        }

        public static double GetHindiScore(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            int keywordHits = 0;
            for (int i = 0; i < HindiKeywords.Length; i++)
            {
                if (input.Contains(HindiKeywords[i]))
                    keywordHits++;
            }

            int charCount = HindiCharRegex.Matches(input).Count;
            int length = input.Length;

            return (keywordHits * 2.0 + charCount) / length;
        }
    }
}
