using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PhoenixEngine.EngineManagement;

namespace PhoenixEngineR.RequestManagement
{
    public class ProxyCenter
    {
        public static WebProxy CurrentProxy = null;

        public static void UsingProxy()
        {
            if (!string.IsNullOrWhiteSpace(PhoenixRConfig.ProxyUrl))
            {
                WebProxy NewProxy = new WebProxy(PhoenixRConfig.ProxyUrl);

                if (!string.IsNullOrEmpty(PhoenixRConfig.ProxyUserName) &&
               !string.IsNullOrEmpty(PhoenixRConfig.ProxyPassword))
                {
                    NewProxy.Credentials = new NetworkCredential(
                        PhoenixRConfig.ProxyUserName,
                        PhoenixRConfig.ProxyPassword
                    );
                }

                CurrentProxy = NewProxy;
            }
            else
            {
                CurrentProxy = null;
            }
        }
    }
}
