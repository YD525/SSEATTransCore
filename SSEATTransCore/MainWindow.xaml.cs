using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using SSEATTransCore.DelegateManagement;
using SSEATTransCore.ServerManagement;

namespace SSEATTransCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ServerHelper Server = null;
        public bool IsDebug = false;
        public int CurrentPort = 0;
        public MainWindow()
        {
            Server = new ServerHelper();

            string[] Args = Environment.GetCommandLineArgs();
            string CommandLine = "";
            foreach (var GetParam in Args)
            {
                CommandLine += GetParam + " ";
            }

            //-Debug 
            //-SetPort 15230

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

            if (CanShowWindows)
            {
                this.Height = 650;
                this.Width = 500;
                this.ShowInTaskbar = true;
            }

            SetLog("Start WebService:" + "http://localhost:" + CurrentPort + "/SSEAT", DateTime.Now);
        }

        public void SetLog(string Str, DateTime Time)
        {
            if (IsDebug)
            {
                this.Dispatcher.Invoke(new Action(() => {
                    Log.Text += string.Format("{0}->{1}\n",Time.ToString("yyyy-MM-dd HH:mm:ss"),Str);
                }));
            }
        }
    }
}
