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
        public Dictionary<string, PexReader> MutiPexReader = new Dictionary<string, PexReader>();
        public API()
        {
            PhoenixR.Init();
            MutiPexReader.Clear();

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
                            PhoenixRConfig.LMModel = "(Auto)";
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
        public string LM_Translate(string Source, string FromLang, string ToLang, bool UseAIMemory)
        {
            Languages From = LanguageHelper.FromLanguageCode(FromLang);

            var GetResult = new LMStudio().QuickTrans(
                new List<string>() { },
                Source,
                From,
                LanguageHelper.FromLanguageCode(ToLang),
                PhoenixRConfig.ContextEnable,
                PhoenixRConfig.ContextLimit,
                "", "");

            if (PhoenixRConfig.ContextEnable)
            {
                PhoenixR.AIMemory.AddTranslation(From, Source, GetResult);
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
        /// <summary>
        /// Download Missing Tool
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// Use thread locks,Supports multi-threaded calls.
        /// </summary>
        public object WriteLock = new object();
        /// <summary>
        /// Modify the record specified in Pex using Key(ID).
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void SetCache(string Key, string Value)
        {
            lock (WriteLock)
            {
                try
                {
                    if (PhoenixR.TransData.ContainsKey(Key))
                    {
                        PhoenixR.TransData[Key] = Value;
                    }
                    else
                    {
                        PhoenixR.TransData.Add(Key, Value);
                    }
                }
                catch { }
            }
        }

       
        public object PexReaderLock = new object();
        /// <summary>
        /// Support read muti pex file
        /// </summary>
        public string MutiReadPexFile(string InputPath)
        {
            lock (PexReaderLock)
            {
                List<StringParam> StringParams = new List<StringParam>();
                if (MutiPexReader.ContainsKey(InputPath))
                {
                    if (MutiPexReader[InputPath].Strings != null)
                        StringParams.AddRange(MutiPexReader[InputPath].Strings);
                }
                else
                {
                    try
                    {
                        PexReader NewPexReader = new PexReader();
                        NewPexReader.LoadPexFile(InputPath);

                        if (NewPexReader.Strings != null)
                        {
                            if (NewPexReader.Strings.Count > 0)
                            {
                                StringParams = NewPexReader.Strings;
                                MutiPexReader.Add(InputPath, NewPexReader);
                            }
                        }
                    }
                    catch
                    {
                        return Bridge.GetJson(Bridge.Return<List<StringParam>>(-1, "", new List<StringParam>()));
                    }
                }

                if (StringParams.Count > 0)
                {
                    return Bridge.GetJson(Bridge.Return<List<StringParam>>(1, "", StringParams));
                }
                else
                {
                    return Bridge.GetJson(Bridge.Return<List<StringParam>>(0, "", new List<StringParam>()));
                }
            }
        }

        /// <summary>
        /// Apply change to target file
        /// </summary>
        /// <param name="InputPath"></param>
        /// <returns></returns>
        public int MutiSavePexFile(string InputPath)
        {
            lock (PexReaderLock)
            {
                bool SaveState = false;

                try
                {
                    if (MutiPexReader.ContainsKey(InputPath))
                    {
                        var GetReader = MutiPexReader[InputPath];
                        if (GetReader.SavePexFile(InputPath))
                        {
                            SaveState = true;
                            GetReader.Close();
                        }

                        MutiPexReader.Remove(InputPath);
                    }
                }
                catch { }

                if (SaveState)
                {
                    return 1;
                }

                return -1;
            }
        }

        /// <summary>
        /// Return Opened Pex Count
        /// </summary>
        /// <returns></returns>
        public int GetPexReaderCount()
        {
            return MutiPexReader.Count;
        }
      
        /// <summary>
        /// Current Pex Path
        /// </summary>
        public string LastOpenPexFilePath = "";
        public PexReader SinglePexReader = new PexReader();

        /// <summary>
        /// Open Pex File (Single)
        /// </summary>
        /// <param name="InputPath"></param>
        /// <returns></returns>
        public string ReadPexFile(string InputPath)
        {
            try
            {
                SinglePexReader.LoadPexFile(InputPath);
            }
            catch
            {
                return Bridge.GetJson(Bridge.Return<List<StringParam>>(-1, "", new List<StringParam>()));
            }

            if (SinglePexReader.Strings != null)
            {
                if (SinglePexReader.Strings.Count > 0)
                {
                    return Bridge.GetJson(Bridge.Return<List<StringParam>>(1, "", SinglePexReader.Strings));
                }
            }

            return Bridge.GetJson(Bridge.Return<List<StringParam>>(0, "", new List<StringParam>()));
        }

        /// <summary>
        /// Save Current Pex File (Single)
        /// </summary>
        /// <returns></returns>
        public int SavePexFile()
        {
            if (LastOpenPexFilePath.Length == 0)
            {
                return 0;
            }
            if (File.Exists(LastOpenPexFilePath))
            {
                return 0;
            }

            bool SaveState = false;

            try
            {
                if (SinglePexReader.SavePexFile(LastOpenPexFilePath))
                {
                    SaveState = true;
                    SinglePexReader.Close();
                }
            }
            catch { }

            if (SaveState)
            {
                return 1;
            }

            return 0;
        }
        #endregion

    }
}