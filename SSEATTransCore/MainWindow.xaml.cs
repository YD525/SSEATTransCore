using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JsonCore;
using PhoenixEngine.ConvertManager;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.TranslateCore;
using PhoenixEngine.TranslateManage;
using PhoenixEngineR.TranslateManage;
using SSEATTransCore.DelegateManagement;
using SSEATTransCore.ServerManagement;
using SSEATTransCore.SkyrimManagement;
namespace SSEATTransCore
{
    public class Empty
    {

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ServerHelper Server = null;
        public PexReader PexReader = null;
        public bool IsDebug = false;
        public int CurrentPort = 0;

        //SSEATTransCore.exe -Debug -SetPort 11152
        public MainWindow()
        {
            Server = new ServerHelper();

            TranslatorExtend.Init();

            PexReader = new PexReader();

            string[] Args = Environment.GetCommandLineArgs();
            string CommandLine = "";
            foreach (var GetParam in Args)
            {
                CommandLine += GetParam + " ";
            }

            bool CanShowWindows = false;

            List<string> Params = CommandLine.Split('-').ToList();
            Params.RemoveAt(0);

            foreach (var GetParam in Params)
            {
                if (GetParam.Trim().Equals("Debug"))
                {
                    CanShowWindows = true;
                    IsDebug = true;
                }
                else
                if (GetParam.Trim().StartsWith("SetPort "))
                {
                    int GetPort = int.Parse(GetParam.Substring(GetParam.IndexOf("SetPort ")
                    + "SetPort ".Length).Trim());

                    DelegateHelper.SetLog += SetLog;

                    CurrentPort = GetPort;
                    Server.Init(GetPort);

                    //Start WebService 
                    //http://localhost:Port/SSEAT HttpPost
                }
            }

            // Wire the HTTP callback once the window is created
            ServerHelper.CallBack = (request, response) =>
            {
                var result = HandleRequest(request, response);
                return JsonHelper.GetJson(result);
            };

            InitializeComponent();

            this.Hide();

            if (CanShowWindows)
            {
                this.Height = 650;
                this.Width = 500;
                this.ShowInTaskbar = true;

                this.Show();
            }

            SetLog("Start WebService:" + "http://localhost:" + CurrentPort + "/SSEAT", DateTime.Now);
        }

        //Json returned after general response request
        //Json {code=1,xxxxxx}

        public object Return(int Code)
        {
            return Return<Empty>(Code, string.Empty,new Empty());
        }
        public object Return(int Code,string Message)
        {
            return Return<Empty>(Code, Message, new Empty());
        }
        public object Return<T>(int Code, string Message,T Data) where T : new()
        {
            return new Result<T>(Code, Message, Data);
        }

        //Process Http request and return 
        public object HandleRequest(HttpListenerRequest Request, HttpListenerResponse Response)
        {
            object Json = new object();

            try
            {
                string GetType = Request.QueryString.Get("Type");

                switch (GetType)
                {
                    case "CloseBatchTranslation":
                        {
                            //Terminate all threads that are translating and clear the queue
                            Engine.End();
                            Json = Return(1);
                        }
                    break;
                    case "StopBatchTranslation":
                        {
                            //Pause Continue Batch Translation
                            bool PauseState = ConvertHelper.ObjToBool(Request.QueryString.Get("PauseState"));
                            Engine.Stop(PauseState);
                            Json = Return(1);
                        }
                    break;  
                    case "StartBatchTranslation":
                        {
                            //Start batch translation
                            if (Engine.From != Languages.Null && Engine.To != Languages.Null)
                            {
                                Engine.Start(true);
                                Json = Return(1);
                            }

                            Json = Return(0);
                        }
                    break;
                    case "Dequeue":
                        {
                            //Dequeue completed items
                            bool IsEnd = false;
                            var GetUnit = Engine.DequeueTranslated(ref IsEnd);

                            if (IsEnd)
                            {
                                //code == 1 The queue is empty and all entries have been translated.
                                Json = Return<TranslationUnit>(1, "", GetUnit);
                            }
                            else
                            {
                                //code == 0 There is still content in the queue and it still needs to be dequeued.
                                Json = Return<TranslationUnit>(0, "", GetUnit);
                            }
                        }
                        break;
                    //Add items to the queue that need translation.
                    case "Enqueue":
                        {
                            //Queue items that need translation
                            var Form = Server.GetPostData(Request);
                            //Post Payload
                            string FileName = Form["FileName"];
                            string Key = Form["Key"];
                            string Type = Form["Type "];
                            string Original = Form["Original"];
                            string AIParam = Form["AIParam"];
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

                            Json = Return(GetEnqueueCount);
                        }
                        break;
                    case "GetWorkingThreadCount":
                        {
                            //Get the number of working threads
                            Json = Return(1,Engine.GetThreadCount().ToString());
                        }
                        break;
                    case "SetThread":
                        {
                            //Set the maximum number of working threads
                            int ThreadCount = ConvertHelper.ObjToInt(Request.QueryString.Get("ThreadCount"));

                            EngineConfig.MaxThreadCount = ThreadCount;
                            EngineConfig.AutoSetThreadLimit = false;

                            EngineConfig.Save();
                        }
                        break;
                    case "GetData":
                        {
                            var Form = Server.GetPostData(Request);
                            string Key = Form["Key"];

                            lock(Translator.TransDataLocker)
                            if (Translator.TransData.ContainsKey(Key))
                            {
                                Json = Return(1, Translator.TransData[Key]);
                            }
                            else
                            {
                                Json = Return(0,string.Empty);
                            }
                        }
                        break;
                    case "SetData":
                        {
                            var Form = Server.GetPostData(Request);
                            string Key = Form["Key"];
                            string Value = Form["Value"];

                            lock (Translator.TransDataLocker)
                            if (Translator.TransData.ContainsKey(Key))
                            {
                                Translator.TransData[Key] = Value;
                            }
                            else
                            {
                                Translator.TransData.Add(Key, Value);
                            }

                            Json = Return(1);
                        }
                    break;
                    case "SavePexFile":
                        {
                            var Form = Server.GetPostData(Request);

                            string OutputPath = Form["OutputPath"];

                            PexReader.SavePexFile(OutputPath);

                            Json = Return(1);
                        }
                        break;
                    case "ReadPexFile":
                        {
                            var Form = Server.GetPostData(Request);

                            string InputPath = Form["InputPath"];

                            PexReader.LoadPexFile(InputPath);

                            Json = Return<List<StringParam>>(1,"",PexReader.Strings);
                        }
                        break;
                    case "SetApiKey":
                        { 
                            string PlatformType = Request.QueryString.Get("PlatformType");
                            bool Enable = ConvertHelper.ObjToBool(Request.QueryString.Get("Enable"));
                            var Form = Server.GetPostData(Request);

                            //Post Payload
                            string ApiKey = Form["ApiKey"];

                            //Configure the key according to the platform
                            switch (ApiKey)
                            {
                                case "ChatGpt":
                                    {
                                        EngineConfig.ChatGptKey = ApiKey;
                                        EngineConfig.ChatGptApiEnable = Enable;
                                    }
                                break;
                                case "Gemini":
                                    {
                                        EngineConfig.GeminiKey = ApiKey;
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
                                        EngineConfig.DeepLApiEnable = Enable;
                                    }
                                break;
                            }

                            EngineConfig.Save();
                            Json = Return(1);
                        }
                        break;
                    case "SetFromTo":
                        {
                            string From = Request.QueryString.Get("From");
                            string To = Request.QueryString.Get("To");

                            Engine.From = LanguageHelper.FromLanguageCode(From);
                            Engine.To = LanguageHelper.FromLanguageCode(To);

                            if (Engine.From == Languages.Null)
                            {
                                Engine.From = Languages.Auto;
                            }

                            if (Engine.To == Languages.Null)
                            {
                                Json = Return(0, "Target language cannot be empty");
                            }

                            Json = Return(1, string.Empty);
                        }
                        break;
                    case "SetSkyrimPath":
                        {
                            var Form = Server.GetPostData(Request);

                            //Post Payload
                            string SkyrimPath = Form["SkyrimPath"];

                            if (Directory.Exists(SkyrimPath))
                            {
                                SkyrimHelper.SkyrimPath = SkyrimPath;

                                Json = Return(1, SkyrimPath);
                            }

                            Json = Return(0, SkyrimPath);
                        }
                        break;
                    case "TranslateV1":
                        {
                            try
                            {
                                //Suitable for translation one by one
                                var Form = Server.GetPostData(Request);
                                //Post Payload
                                string Original = Form["Original"];

                                TranslationUnit NTranslationUnit = new TranslationUnit(Original.GetHashCode(), Original,
                                    "", Original,
                                    "", "",
                                    PhoenixEngine.TranslateCore.Languages.Auto,
                                    Engine.To,
                                    100
                                    );
                                NTranslationUnit.From = PhoenixEngine.TranslateCore.Languages.Auto;
                                ;
                                bool CanSleep = false;

                                var GetResult = Translator.QuickTrans(NTranslationUnit, ref CanSleep);

                                Json = Return(1, GetResult);
                            }
                            catch (Exception Ex)
                            {
                                Json = Return(0, Ex.Message);
                            }
                        }
                        break;
                    //http://localhost:11152/SSEAT?Type=InitEngine
                    //Must be called before working.
                    case "InitEngine":
                        {
                            Engine.Init();
                            Json = Return(1);
                        }
                        break;
                    //http://localhost:11152/SSEAT?Type=CloseService
                    case "CloseService":
                        {
                            if (Server.Listener != null)
                            {
                                try
                                {
                                    //Close Service
                                    Server.Listener.Close();
                                    GC.SuppressFinalize(this);
                                }
                                catch { }
                            }

                            new Thread(() => {
                                Thread.Sleep(1000);
                                DeFine.ExitAny();
                            }).Start();

                            Json = Return(1);
                        }
                        break;
                    default:
                        {
                            Json = Return(-2);
                        }
                        break;
                }


            }
            catch { return Return(-3); }

            return Json;
        }

        #region Log

        /// <summary>
        /// Output debug information received request and response content
        /// </summary>
        /// <param name="Str"></param>
        /// <param name="Time"></param>
        public void SetLog(string Str, DateTime Time)
        {
            if (IsDebug)//When executing the program, you must add the -Debug command line to be effective
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    Log.Text += string.Format("{0}->{1}\n", Time.ToString("yyyy-MM-dd HH:mm:ss"), Str);
                    Log.ScrollToEnd();
                }));
            }
        }

        #endregion

    }
}
