
using System;
using System.Collections.Generic;
using System.Threading;
using PhoenixEngine.TranslateCore;

namespace PhoenixEngine.TranslateManagement
{
    public static class SpeechHelper
    {
        private static readonly object VoiceLock = new object();
        private static dynamic VoiceInstance = null;

        private static readonly Dictionary<Languages, string[]> VoiceHints = new Dictionary<Languages, string[]>()
{
    { Languages.English, new string[] { "English", "David", "Zira", "George" } },
    { Languages.SimplifiedChinese, new string[] { "Chinese", "Huihui", "Zh-cn" } },
    { Languages.TraditionalChinese, new string[] { "Chinese (Traditional)", "Zh-hk", "Zh-tw" } },
    { Languages.Japanese, new string[] { "Japanese", "Haruka", "Ja-jp" } },
    { Languages.German, new string[] { "German", "De-de" } },
    { Languages.Korean, new string[] { "Korean", "Heami", "Ko-kr" } },
    { Languages.Turkish, new string[] { "Turkish", "Tr-tr" } },
    { Languages.Brazilian, new string[] { "Portuguese", "Pt-br" } },
    { Languages.Russian, new string[] { "Russian", "Ru-ru" } },
    { Languages.Italian, new string[] { "Italian", "It-it" } },
    { Languages.Spanish, new string[] { "Spanish", "Es-es" } },
    { Languages.Hindi, new string[] { "Hindi", "Hi-in" } },
    { Languages.Urdu, new string[] { "Urdu", "Ur-pk" } },
    { Languages.Indonesian, new string[] { "Indonesian", "Id-id" } }
};

        public static void TryPlaySound(string Text,bool CanCreatTrd = false)
        {
            Action PlaySoundAction = new Action(() => {
                try
                {
                    Languages Lang = LanguageHelper.DetectLanguageByLine(Text);
                    lock (VoiceLock)
                    {
                        if (VoiceInstance == null)
                        {
                            Type VoiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
                            VoiceInstance = Activator.CreateInstance(VoiceType);
                            VoiceInstance.Volume = 100;
                            VoiceInstance.Rate = 0;
                        }

                        dynamic Voices = VoiceInstance.GetVoices();
                        dynamic BestMatch = null;

                        if (VoiceHints.TryGetValue(Lang, out var Hints))
                        {
                            foreach (dynamic Token in Voices)
                            {
                                string Desc = Token.GetDescription().ToString();
                                string LangAttr = Token.GetAttribute("Language")?.ToString() ?? "";

                                foreach (var Hint in Hints)
                                {
                                    if (Desc.ToLower().Contains(Hint.ToLower()) ||
                                    LangAttr.ToLower().Contains(Hint.ToLower()))
                                    {
                                        BestMatch = Token;
                                        break;
                                    }
                                }

                                if (BestMatch != null)
                                    break;
                            }
                        }

                        if (BestMatch != null)
                            VoiceInstance.Voice = BestMatch;

                        VoiceInstance.Speak("", 2); // Purge before speak
                        VoiceInstance.Speak(Text, 1); // Async speak
                    }
                }
                catch
                {
                }
            });

            if (!CanCreatTrd)
            {
                PlaySoundAction.Invoke();
            }
            else
            {
                new Thread(() => {
                    PlaySoundAction.Invoke();
                }).Start();
            }
        }
    }
}
