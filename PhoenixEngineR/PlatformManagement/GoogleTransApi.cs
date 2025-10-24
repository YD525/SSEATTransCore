using System.Net.Http;
using System.Web;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.TranslateCore;
using PhoenixEngine.TranslateManage;
using PhoenixEngineR.RequestManagement;
using static PhoenixEngine.EngineManagement.DataTransmission;

namespace PhoenixEngine.PlatformManagement
{
    public class GoogleTransApi
    {
        private static readonly HttpClient _HttpClient = CreateHttpClient();
        private static HttpClient CreateHttpClient()
        {
            try
            {
                if (ProxyCenter.CurrentProxy!=null)
                {
                    var Proxy = ProxyCenter.CurrentProxy;

                    var Handler = new HttpClientHandler
                    {
                        Proxy = Proxy,
                        UseProxy = true
                    };

                    return new HttpClient(Handler);
                }
                else
                {
                    return new HttpClient();
                }
            }
            catch { return new HttpClient(); }
        }
        public string Translate(string Text, Languages TargetLanguage, Languages? SourceLanguage, ref PlatformCall Call)
        {
            try
            {
                string TargetLang = LanguageHelper.ToLanguageCode(TargetLanguage);
                string SourceLang = SourceLanguage.HasValue ? LanguageHelper.ToLanguageCode(SourceLanguage.Value) : "auto";

                string Url = "https://translation.googleapis.com/language/translate/v2" +
                             "?key=" + EngineConfig.GoogleApiKey +
                             "&q=" + HttpUtility.UrlEncode(Text) +
                             "&target=" + TargetLang +
                             "&source=" + SourceLang;

                Call.Platform = PlatformType.GoogleApi;
                Call.SendString = Url;

                HttpResponseMessage Response = _HttpClient.GetAsync(Url).Result;
                Response.EnsureSuccessStatusCode();

                string Json = Response.Content.ReadAsStringAsync().Result;
                Call.ReceiveString = Json;

                string Marker = "\"translatedText\":\"";
                int Start = Json.IndexOf(Marker);
                if (Start >= 0)
                {
                    Start += Marker.Length;
                    int End = Json.IndexOf("\"", Start);
                    if (End > Start)
                    {
                        Call.Success = true;
                        string Result = Json.Substring(Start, End - Start);
                        Result = Result.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                        return Result;
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
