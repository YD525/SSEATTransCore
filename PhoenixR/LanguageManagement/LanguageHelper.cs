using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixEngineR.LanguageManagement
{
    public enum Languages
    {
        Null = -2, English = 0, SimplifiedChinese = 1, Japanese = 2, German = 5, Korean = 6, Turkish = 7, Brazilian = 8, Russian = 9, TraditionalChinese = 10, Italian = 11, Spanish = 12, Hindi = 13, Urdu = 15, Indonesian = 16, French = 17, Vietnamese = 20, Polish = 22, CanadianFrench = 23, Portuguese = 25, Ukrainian = 26
    }
    public static class LanguageConverter
    {
        private static readonly Dictionary<Languages, string> LanguageCodeMap = new Dictionary<Languages, string>()
        {
            [Languages.English] = "en",
            [Languages.SimplifiedChinese] = "zh-CN",
            [Languages.TraditionalChinese] = "zh-TW",
            [Languages.Japanese] = "ja",
            [Languages.German] = "de",
            [Languages.Korean] = "ko",
            [Languages.Turkish] = "tr",
            [Languages.Brazilian] = "pt-BR",
            [Languages.Portuguese] = "pt",
            [Languages.Russian] = "ru",
            [Languages.Ukrainian] = "uk",
            [Languages.Italian] = "it",
            [Languages.Spanish] = "es",
            [Languages.Hindi] = "hi",
            [Languages.Urdu] = "ur",
            [Languages.Indonesian] = "id",
            [Languages.French] = "fr",
            [Languages.CanadianFrench] = "fr-CA",
            [Languages.Vietnamese] = "vi",
            [Languages.Polish] = "pl",
            [Languages.Null] = ""
        };

        private static readonly Dictionary<string, Languages> CodeToLanguageMap = new Dictionary<string, Languages>(StringComparer.OrdinalIgnoreCase);
        static LanguageConverter()
        {
            foreach (var pair in LanguageCodeMap)
            {
                if (!string.IsNullOrWhiteSpace(pair.Value))
                {
                    CodeToLanguageMap[pair.Value] = pair.Key;
                }
            }
        }
        public static string ToLanguageCode(Languages lang)
        {
            return LanguageCodeMap.TryGetValue(lang, out var code) ? code : "";
        }
        public static Languages FromLanguageCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Languages.Null;

            return CodeToLanguageMap.TryGetValue(code, out var lang) ? lang : Languages.Null;
        }
    }

    public class LanguageHelper
    {
        public static string ToLanguageCode(Languages Lang)
        {
            return LanguageConverter.ToLanguageCode(Lang);
        }

        public static Languages FromLanguageCode(string Code)
        {
            Languages Lang = LanguageConverter.FromLanguageCode(Code);
            return Lang;
        }
        public class FileLanguageDetect
        {
            LanguageDetect LanguageDetectItem = new LanguageDetect();

            public Languages GetLang()
            {
                return LanguageDetectItem.GetMaxLang();
            }
        }
        public class LanguageDetect
        {
            public Dictionary<Languages, double> Array = new Dictionary<Languages, double>();

            public void Add(Languages Lang)
            {
                if (Array.ContainsKey(Lang))
                {
                    Array[Lang] = Array[Lang] + 1;
                }
                else
                {
                    Array.Add(Lang, 1);
                }
            }

            public void Add(Languages Lang, double Ratio)
            {
                if (Array.ContainsKey(Lang))
                {
                    Array[Lang] = Array[Lang] + Ratio;
                }
                else
                {
                    Array.Add(Lang, Ratio);
                }
            }


            public Languages GetMaxLang()
            {
                if (Array.Count > 0)
                {
                    return Array
                      .OrderByDescending(kv => kv.Value)
                      .First().Key;
                }
                return Languages.English;
            }
        }

    }
}
