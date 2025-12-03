using System.Collections.Generic;
using System.IO;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.PlatformManagement.LocalAI;
using PhoenixEngine.TranslateManage;
using PhoenixEngineR.LanguageManagement;

namespace PhoenixEngineR.SSEAT
{
    public class API
    {
        private PexReader PexReader = null;
        public API()
        {
            PhoenixR.Init();
            PexReader = new PexReader();

            string SetCachePath = Bridge.GetFullPath(@"\Cache");
            if (!Directory.Exists(SetCachePath))
            {
                Directory.CreateDirectory(SetCachePath);
            }
        }

        /// <summary>
        /// Set the root directory of the folder where the program is located.
        /// </summary>
        /// <param name="StartupPath"></param>
        public void SetStartupPath(string StartupPath)
        {
            Bridge.StartupPath = StartupPath;
        }

        public void SetApiKey(string ApiKey, string AIModel, int EnableState, int IsFreeDeepL, int LocalAIPort)
        {
            bool FreeDeepLEnable = false;
            if (IsFreeDeepL == 1)
            {
                FreeDeepLEnable = true;
            }
            bool Enable = false;
            if (EnableState == 1)
            {
                Enable = true;
            }

            switch (ApiKey)
            {
                case "LocalAI":
                    {
                        //LocalAI 
                        PhoenixRConfig.LMLocalAIEnable = Enable;

                        if (LocalAIPort > 0)
                        {
                            PhoenixRConfig.LMPort = LocalAIPort;
                        }
                        if (AIModel.Length > 0)
                        {
                            PhoenixRConfig.LMModel = AIModel;
                        }
                        //EngineConfig.LMModel = "google/gemma-3-12b";
                    }
                break;
            }

            PhoenixRConfig.Save();
        }

        #region JsonGetter
        public string GetValue(string Json, string Name)
        {
            return JsonGeter.GetValue(Json, Name);
        }
        #endregion

        #region Translation

        /// <summary>
        /// LM_Translate
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public string LM_Translate(string Source, string FromLang,string ToLang,bool UseAIMemory)
        {
            Languages From = LanguageHelper.FromLanguageCode(FromLang);

            var GetResult = new LMStudio().QuickTrans(
                new List<string>() { },
                Source,
                From,
                LanguageHelper.FromLanguageCode(ToLang),
                PhoenixRConfig.ContextEnable,
                PhoenixRConfig.ContextLimit,
                "","");

            if (PhoenixRConfig.ContextEnable)
            {
                PhoenixR.AIMemory.AddTranslation(From,Source,GetResult);
            }

            return GetResult;
        }

        //It needs to be invoked when switching languages, and also when the translation is complete and context support is no longer required.
        public void ClearAIMemory()
        {
            PhoenixR.AIMemory.Clear();
        }

        public void Config(string ProxyUrl, bool ContextEnable, int ContextLimit)
        {
            PhoenixRConfig.ProxyUrl = ProxyUrl;
            PhoenixRConfig.ContextEnable = ContextEnable;
            PhoenixRConfig.ContextLimit = ContextLimit;

            PhoenixRConfig.Save();
        }
       
        #endregion

        #region Papyrus Read Write
        public int DownLoadAndInstallChampollion()
        {
            bool State = true;
            //Frist Check ToolPath
            if (!File.Exists(Bridge.GetFullPath(@"Tool\Champollion.exe")))
            {
                if (ToolDownloader.DownloadChampollion())
                {
                    State = true;
                }
            }

            if (State)
            {
                return 1;
            }

            return 0;
        }

        private string LastSetInputPath = "";
        public string ReadPexFile(string InputPath)
        {
            try
            {
                LastSetInputPath = InputPath;
                PexReader?.LoadPexFile(InputPath);
                return Bridge.GetJson(Bridge.Return<List<StringParam>>(1, "", PexReader.Strings));
            }
            catch
            {
                return Bridge.GetJson(Bridge.Return<List<StringParam>>(-1, "", new List<StringParam>()));
            }
        }
        public int SavePexFile()
        {
            if (File.Exists(LastSetInputPath) && LastSetInputPath.Length > 0)
            {
                try
                {
                    PexReader.SavePexFile(LastSetInputPath);
                    LastSetInputPath = string.Empty;
                    return 1;
                }
                catch
                {
                    return -1;
                }
            }

            return 0;
        }

        #endregion

    }
}