using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PhoenixEngineR.RequestManagement;
using System.IO.Compression;

namespace PhoenixEngineR.SSEAT
{
    public class ToolDownloader
    {
        private const string ChampollionMd5Sign = "bab15b1f4c45b41fbb024bd61087dab6";//v1.3.2
        private const string PapyrusAssemblerMD5Sign = "55a426bda1af9101ad5359f276805ab6";
        private const string ScriptCompileMD5Sign = "9774f28bb11963ca3fb06797bbbc33ec";

        public static string GetMD5(byte[] Data)
        {
            using (var Md5 = MD5.Create())
            {
                byte[] Hash = Md5.ComputeHash(Data);
                StringBuilder Sb = new StringBuilder();
                foreach (byte B in Hash)
                {
                    Sb.Append(B.ToString("x2"));
                }
                return Sb.ToString();
            }
        }

        private static void DownloadAndExtract(string Url, string DestinationFolder, IWebProxy Proxy = null)
        {
            string TempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");

            try
            {
                var Handler = new HttpClientHandler();
                if (Proxy != null)
                {
                    Handler.Proxy = Proxy;
                    Handler.UseProxy = true;
                }

                using (var HttpClient = new HttpClient(Handler))
                {
                    HttpClient.Timeout = TimeSpan.FromSeconds(20);

                    using (var Response = HttpClient.GetAsync(Url).GetAwaiter().GetResult())
                    {
                        Response.EnsureSuccessStatusCode();
                        using (var FS = new FileStream(TempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            Response.Content.CopyToAsync(FS).GetAwaiter().GetResult();
                        }
                    }
                }


                if (!Directory.Exists(DestinationFolder))
                    Directory.CreateDirectory(DestinationFolder);

                ZipFile.ExtractToDirectory(TempFile, DestinationFolder);
            }
            finally
            {
                if (File.Exists(TempFile))
                    File.Delete(TempFile);
            }
        }


        public static bool DownloadChampollion(string ProxyIP = "")
        {
            WebProxy SetProxy = null;

            if (ProxyIP.Length > 0)
            {
                ProxyCenter.CurrentProxy = new WebProxy(ProxyIP);
            }

            if (ProxyCenter.CurrentProxy != null)
            {
                SetProxy = ProxyCenter.CurrentProxy;
            }

            string SetFolder = Bridge.GetFullPath(@"Tool\");

            DownloadAndExtract(
                "https://github.com/Orvid/Champollion/releases/download/v1.3.2/Champollion.v1.3.2.zip",
                 SetFolder,
                 SetProxy
                );

            string SetFilePath = Bridge.GetFullPath(@"Tool\Champollion.exe");
            if (File.Exists(SetFilePath))
            {
                byte[] ReadFileData = DataHelper.ReadFile(SetFilePath);

                string CurrentMD5 = GetMD5(ReadFileData);

                // Verify the downloaded file's MD5 signature to ensure its integrity and authenticity.
                // Purpose: Protect users from potentially malicious files if the remote repository 
                // (e.g., GitHub) updates or tampers with the file.
                if (CurrentMD5.Equals(ChampollionMd5Sign))
                {
                    return true;

                }
            }

            return false;
        }
    }
}
