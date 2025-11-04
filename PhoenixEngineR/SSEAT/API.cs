using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PhoenixEngine.ConvertManager;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.TranslateCore;
using PhoenixEngine.TranslateManage;
using PhoenixEngineR.TranslateManage;

namespace PhoenixEngineR.SSEAT
{
    
    public class API
    {
        private PexReader PexReader = null;
        public API()
        {
            Engine.Init();
            PexReader = new PexReader();

            string SetCachePath = Bridge.GetFullPath(@"\Cache");
            if (!Directory.Exists(SetCachePath))
            {
                Directory.CreateDirectory(SetCachePath);
            }
        }
        public static void SetApiKey(string ApiKey, string AIModel, int EnableState, int IsFreeDeepL, int LocalAIPort)
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
                case "ChatGpt":
                    {
                        EngineConfig.ChatGptKey = ApiKey;
                        EngineConfig.ChatGptModel = AIModel;
                        EngineConfig.ChatGptApiEnable = Enable;
                    }
                    break;
                case "Gemini":
                    {
                        EngineConfig.GeminiKey = ApiKey;
                        EngineConfig.GeminiModel = AIModel;
                        EngineConfig.GeminiApiEnable = Enable;
                    }
                    break;
                case "Cohere":
                    {
                        EngineConfig.CohereKey = ApiKey;
                        EngineConfig.CohereApiEnable = Enable;
                    }
                    break;
                case "DeepL":
                    {
                        EngineConfig.DeepLKey = ApiKey;
                        EngineConfig.IsFreeDeepL = FreeDeepLEnable;
                        EngineConfig.DeepLApiEnable = Enable;
                    }
                    break;
                case "LocalAI":
                    {
                        //LocalAI 
                        EngineConfig.LMLocalAIEnable = Enable;

                        if (LocalAIPort > 0)
                        {
                            EngineConfig.LMPort = LocalAIPort;
                        }
                        if (AIModel.Length > 0)
                        {
                            EngineConfig.LMModel = AIModel;
                        }
                        //EngineConfig.LMModel = "google/gemma-3-12b";
                    }
                    break;
                case "DeepSeek":
                    {
                        EngineConfig.DeepSeekKey = ApiKey;
                        EngineConfig.DeepSeekModel = AIModel;
                        EngineConfig.DeepSeekApiEnable = Enable;

                    }
                    break;
                case "Baichuan":
                    {
                        EngineConfig.BaichuanKey = ApiKey;
                        EngineConfig.BaichuanModel = AIModel;
                        EngineConfig.BaichuanApiEnable = Enable;
                    }
                    break;
            }

            EngineConfig.Save();

        }

        #region JsonGetter
        public static string GetValue(string Json,string Name)
        {
            return JsonGeter.GetValue(Json,Name);
        }
        #endregion

        #region Translation
        public static int StartBatchTranslation()
        {
            if (Engine.From != Languages.Null && Engine.To != Languages.Null)
            {
                return Engine.Start(true);
            }

            return 0;
        }
        public static int ControlBatchTranslationState(int State)
        {
            bool PauseState = false;

            if (State == 1)
            {
                PauseState = true;
            }
            return Engine.Stop(PauseState);
        }
        public static int CloseBatchTranslation()
        {
            return Engine.End();
        }
        public static string Dequeue()
        {
            bool IsEnd = false;
            var GetUnit = Engine.DequeueTranslated(ref IsEnd);

            if (IsEnd)
            {
                //code == 1 The queue is empty and all entries have been translated.
                return Bridge.GetJson(Bridge.Return<TranslationUnit>(1, "", GetUnit));
            }
            else
            {
                //code == 0 There is still content in the queue and it still needs to be dequeued.
                return Bridge.GetJson(Bridge.Return<TranslationUnit>(0, "", GetUnit));
            }
        }
        public static int Enqueue(string FileName, string Key, string Type, string Original, string AIParam)
        {
            TranslationUnit Unit = new TranslationUnit(
            FileName.GetHashCode(),
            Key,
            Type,
            Original,
            "",
            AIParam,
            Engine.From,
            Engine.To,
            100
            );

            int GetEnqueueCount = Engine.AddTranslationUnit(Unit);

            return GetEnqueueCount;
        }
        public static int GetWorkingThreadCount()
        {
            return Engine.GetThreadCount();
        }
        public static void SetThread(int ThreadCount)
        {
            EngineConfig.MaxThreadCount = ThreadCount;
            EngineConfig.AutoSetThreadLimit = false;

            EngineConfig.Save();
        }
        public static int SetTo(string From,string To)
        {
            try 
            {
                Engine.From = LanguageHelper.FromLanguageCode(From);
                Engine.To = LanguageHelper.FromLanguageCode(To);

                return 1;

            } 
            catch 
            { 
                return -1; 
            }
        }
        public static void Config(string ProxyUrl,bool ContextEnable,int ContextLimit)
        { 
            EngineConfig.ProxyUrl = ProxyUrl;
            EngineConfig.ContextEnable = ContextEnable;
            EngineConfig.ContextLimit = ContextLimit;

            EngineConfig.Save();
        }
        public static string TranslateV1(string Original)
        {
            try
            {
                TranslationUnit NTranslationUnit = new TranslationUnit(Original.GetHashCode(), Original,
                    "", Original,
                    "", "",
                    Engine.From,
                    Engine.To,
                    100
                    );
                NTranslationUnit.From = PhoenixEngine.TranslateCore.Languages.Auto;
                ;
                bool CanSleep = false;

                var GetResult = Translator.QuickTrans(NTranslationUnit, ref CanSleep);

                return Bridge.GetJson(Bridge.Return(1, GetResult));
            }
            catch (Exception Ex)
            {
                return Bridge.GetJson(Bridge.Return(-1, Ex.Message));
            }
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
