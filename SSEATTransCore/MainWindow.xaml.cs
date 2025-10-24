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
        public PexReader CurrentPexReader = null;
        public bool IsDebug = false;
        public int CurrentPort = 0;

        //SSEATTransCore.exe -Debug -SetPort 11152
        public MainWindow()
        {
            Server = new ServerHelper();

            TranslatorExtend.Init();

            CurrentPexReader = new PexReader();

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
            return Return(Code, string.Empty);
        }
        public object Return(int Code, string Message)
        {
            return new Result<Empty>(Code, Message, new Empty());
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
                    //http://localhost:11152/SSEAT?Type=ReadPexFile (HTTP GET)
                    //Path Payload
                    case "ReadPexFile":
                        {
                            var Form = Server.GetPostData(Request);

                            //Post Payload
                            string Path = Form["Path"];

                            CurrentPexReader.LoadPexFile(Path);

                            Json = Return(1,JsonHelper.GetJson(CurrentPexReader.Strings));
                        }
                        break;
                    //http://localhost:11152/SSEAT?Type=SetApiKey&PlatformType=ChatGpt&Enable=true (HTTP GET)
                    //ApiKey Payload
                    case "SetApiKey":
                        { 
                            string PlatformType = Request.QueryString.Get("PlatformType");
                            bool Enable = ConvertHelper.ObjToBool(Request.QueryString.Get("Enable"));
                            var Form = Server.GetPostData(Request);

                            //Post Payload
                            string ApiKey = Form["ApiKey"];

                            //if (PlatformType.Equals("ChatGpt"))
                            //{
                            //    EngineConfig.ChatGptKey = ApiKey;
                            //EngineConfig.ChatGptApiEnable = Enable;
                            //}

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
