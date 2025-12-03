using System.Collections.Generic;
using System.Text.RegularExpressions;
using PhoenixEngine.TranslateManage;
using PhoenixEngineR.LanguageManagement;
using PhoenixEngineR.RequestManagement;

namespace PhoenixEngine.EngineManagement
{
    public class PhoenixR
    {
        public static string Version = "1.0";
        public static string CurrentPath = "";

        public static Dictionary<string, string> TransData = new Dictionary<string, string>();

        public static AITranslationMemory AIMemory = new AITranslationMemory();

        public static void Init()
        {
            CurrentPath = GetFullPath(@"\");

            PhoenixRConfig.Load();
            ProxyCenter.UsingProxy();
        }

        public static string LastLoadFileName = "";

        public static string GetFullPath(string Path)
        {
            string GetShellPath = System.Windows.Forms.Application.StartupPath;
            if (GetShellPath.EndsWith(@"\"))
            {
                if (Path.StartsWith(@"\"))
                {
                    Path = Path.Substring(1);
                }
            }
            return GetShellPath + Path;
        }

        public static void AddAIMemory(Languages From,string Original, string Translated)
        {
            AIMemory.AddTranslation(From, Original, Translated);
        }
    }
}
