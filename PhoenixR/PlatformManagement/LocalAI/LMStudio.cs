using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.TranslateManage;
using PhoenixEngineR.LanguageManagement;
using PhoenixEngineR.RequestManagement;
using static PhoenixEngine.PlatformManagement.LocalAI.LocalAIJson;

namespace PhoenixEngine.PlatformManagement.LocalAI
{
    public class LMStudio
    {
        public OpenAIResponse CallAI(string Msg,ref string Recv)
        {
            int GetCount = Msg.Length;
            OpenAIItem NOpenAIItem = new OpenAIItem(PhoenixRConfig.LMModel);
            NOpenAIItem.store = true;
            NOpenAIItem.messages.Add(new OpenAIMessage("user", Msg));
            var GetResult = CallAI(NOpenAIItem,ref Recv);
            return GetResult;
        }

        public OpenAIResponse CallAI(OpenAIItem Item,ref string Recv)
        {
            string GenUrl = PhoenixRConfig.LMHost + ":" + PhoenixRConfig.LMPort + "/v1/chat/completions";
            string GetJson = JsonConvert.SerializeObject(Item);
            WebHeaderCollection Headers = new WebHeaderCollection();
            //Headers.Add("Authorization", string.Format("Bearer {0}", DeFine.GlobalLocalSetting.LMKey));
            HttpItem Http = new HttpItem()
            {
                URL = GenUrl,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36",
                Method = "Post",
                Header = Headers,
                Accept = "*/*",
                Postdata = GetJson,
                Cookie = "",
                ContentType = "application/json"
                //Timeout = DeFine.GlobalRequestTimeOut
                //ProxyIp = ProxyCenter.GlobalProxyIP
            };
            try
            {
                Http.Header.Add("Accept-Encoding", " gzip");
            }
            catch { }

            string GetResult = new HttpHelper().GetHtml(Http).Html;

            Recv = GetResult;
            try
            {
                return JsonConvert.DeserializeObject<OpenAIResponse>(GetResult);
            }
            catch
            {
                return null;
            }
        }
        //"Important: When translating, strictly keep any text inside angle brackets (< >) or square brackets ([ ]) unchanged. Do not modify, translate, or remove them.\n\n"
        public string QuickTrans(List<string> CustomWords, string TransSource, Languages FromLang, Languages ToLang, bool UseAIMemory, int AIMemoryCountLimit, string AIParam,string Type)
        {
            List<string> Related = new List<string>();

            if (PhoenixRConfig.ContextEnable && UseAIMemory)
            {
                Related = PhoenixR.AIMemory.FindRelevantTranslations(FromLang, TransSource, AIMemoryCountLimit);
            }

            if (PhoenixRConfig.UserCustomAIPrompt.Trim().Length > 0)
            {
                AIParam = AIParam + "\n" + PhoenixRConfig.UserCustomAIPrompt;
            }

            var GetTransSource = AIPrompt.GenerateTranslationPrompt(FromLang,ToLang,TransSource,Type, Related,CustomWords, AIParam);
            
            string Send = GetTransSource;
            string Recv = "";
            var GetResult = CallAI(Send,ref Recv);

            if (GetResult != null)
            {
                if (GetResult.choices != null)
                {
                    string GetStr = "";
                    if (GetResult.choices.Length > 0)
                    {
                        GetStr = GetResult.choices[0].message.content.Trim();
                    }
                    if (GetStr.Trim().Length > 0)
                    {
                        try
                        {
                            GetStr = JsonGeter.GetValue(GetStr);
                        }
                        catch
                        {
                            return string.Empty;
                        }

                        if (GetStr.Trim().Equals("<translated_text>"))
                        {
                            return string.Empty;
                        }

                        return GetStr;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            return string.Empty;
        }
    }
}
