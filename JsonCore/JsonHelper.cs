using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonCore
{
    public class JsonHelper
    {
        public static string GetJson(object Obj)
        {
            string GetJson = string.Empty;

            if (Obj != null)
            {
                try 
                {
                    GetJson = JsonConvert.SerializeObject(Obj);
                }
                catch { }
            }

            return GetJson;
        }

   

        public static T ProcessToJson<T>(string Json) where T: new()
        {
            var JsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            if (Json.Replace("\r\n", "").Replace(" ","").StartsWith("{"))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(Json);
                }
                catch
                {
                    return new T(); 
                }
            }

            return new T();
        }
    }
}
