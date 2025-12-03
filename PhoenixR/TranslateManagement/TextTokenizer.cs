using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PhoenixEngine.TranslateManage;
using PhoenixEngineR.LanguageManagement;

namespace PhoenixEngine.TranslateManagement
{
    public class TextTokenizer
    {

        private static readonly HashSet<string> EnglishStopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the", "and", "or", "but", "if", "then", "else",
            "on", "in", "at", "to", "for", "with", "by", "of", "is",
            "it", "this", "that", "these", "those", "as", "are", "was",
            "were", "be", "been", "has", "have", "had", "do", "does", "did",
            "so", "such", "not", "no", "nor", "too", "very", "can", "will",
            "just", "up", "down", "out", "over", "under", "again", "more",
            "most", "some", "any", "each", "all"
        };

        public const int MaxGram = 3 + 1;

        private class TokenWithIndex
        {
            public string Token;
            public int Index;
        }
        public static string[] Tokenize(Languages Lang, string Text)
        {
            Text = Text.Replace('_', ' ').Replace('-', ' ');

            if (Lang.IsSpaceDelimitedLanguage())
            {
                Text = Regex.Replace(Text, "(?<!^)([A-Z])", " $1");
                string[] tokens = Text.Split(new[] { ' ', '.', ',', '?', '!', ';', ':', '(', ')', '[', ']', '{', '}', '"', '\'' },
                    StringSplitOptions.RemoveEmptyEntries);

                List<string> resultTokens = new List<string>();
                for (int i = 0; i < tokens.Length; i++)
                {
                    string t = tokens[i];
                    if (t.Length > 1 && !(Lang == Languages.English && EnglishStopWords.Contains(t)))
                    {
                        resultTokens.Add(t);
                    }
                }

                return resultTokens.ToArray();
            }

            if (!Lang.IsNoSpaceLanguage())
            {
                string[] tokens = Text.Split(new[] { ' ', '.', ',', '?', '!', ';', ':', '(', ')', '[', ']', '{', '}', '"', '\'' },
                    StringSplitOptions.RemoveEmptyEntries);

                List<string> resultTokens = new List<string>();
                for (int i = 0; i < tokens.Length; i++)
                {
                    string t = tokens[i];
                    if (t.Length > 1)
                        resultTokens.Add(t);
                }

                return resultTokens.ToArray();
            }

            List<TokenWithIndex> TokensWithIndex = new List<TokenWithIndex>();
            for (int I = 0; I < Text.Length; I++)
            {
                TokensWithIndex.Add(new TokenWithIndex { Token = " " + Text[I] + " ", Index = I });
            }

            List<string> Result = new List<string>();
            string[] SingleTokens = new string[TokensWithIndex.Count];
            int[] Indices = new int[TokensWithIndex.Count];

            for (int i = 0; i < TokensWithIndex.Count; i++)
            {
                SingleTokens[i] = TokensWithIndex[i].Token;
                Indices[i] = TokensWithIndex[i].Index;
            }

            for (int I = 0; I < SingleTokens.Length; I++)
            {
                for (int Len = 1; Len <= MaxGram && I + Len <= SingleTokens.Length; Len++)
                {
                    bool IsContinuous = true;
                    for (int J = I; J < I + Len - 1; J++)
                    {
                        if (Indices[J + 1] != Indices[J] + 1)
                        {
                            IsContinuous = false;
                            break;
                        }
                    }
                    if (!IsContinuous) continue;

                    System.Text.StringBuilder TokenSb = new System.Text.StringBuilder();
                    for (int K = I; K < I + Len; K++)
                    {
                        TokenSb.Append(SingleTokens[K]);
                    }
                    string Token = TokenSb.ToString().Replace(" ", "");
                    if (!string.IsNullOrWhiteSpace(Token) && Token.Length > 1)
                    {
                        Result.Add(Token);
                    }
                }
            }

            return Result.ToArray();
        }
    }
}
