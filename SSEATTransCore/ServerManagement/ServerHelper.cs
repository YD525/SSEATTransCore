using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PhoenixEngine.EngineManagement;
using SSEATTransCore.DelegateManagement;

namespace SSEATTransCore.ServerManagement
{
    public class Result<T> where T : new()
    {
        public int code = 0;
        public string message = "";
        public T data = default(T);
        public Result(int Code, string Message, T Data)
        {
            this.code = Code;
            this.message = Message;
            this.data = Data;
        }
    }

    public class ServerHelper
    {
        private class Empty
        {

        }

        public object Return(int Code, string Message)
        {
            return new Result<Empty>(Code, Message, new Empty());
        }

        public Thread ServerThread = null;
        public HttpListener Listener;

        public void Init(int Port)
        {
            if (ServerThread == null)
            {
                ServerThread = new Thread(() =>
                {
                    Listener = new HttpListener();
                    Listener.Prefixes.Add(string.Format("http://localhost:{0}/SSEAT/", Port));

                    Listener.Start();
                    Listener.BeginGetContext(Result, null);
                });
            }
            ServerThread.Start();
        }

        /// <summary>
        /// Post Required
        /// </summary>
        /// <param name="ar"></param>
        private void Result(IAsyncResult ar)
        {
            try
            {
                Listener.BeginGetContext(Result, null);
                var Context = Listener.EndGetContext(ar);
                var Request = Context.Request;
                var Response = Context.Response;
                Context.Response.ContentType = "text/json;charset=UTF-8";
                Context.Response.AddHeader("Content-type", "text/plain");
                Context.Response.ContentEncoding = Encoding.UTF8;
                string ReturnObj = null;

                DelegateHelper.SetLog("Recv:" + Context.Request.RawUrl.ToString(),DateTime.Now);

                if (Request.HttpMethod == "POST" && Request.InputStream != null)
                {
                    ReturnObj = JsonCore.JsonHelper.GetJson(HandleRequest(Request, Response));
                }
                else
                    {
                    ReturnObj = JsonCore.JsonHelper.GetJson(HandleRequest(Request, Response));
                }

                var ReturnByteArr = Encoding.UTF8.GetBytes(ReturnObj);

                try
                {
                    using (var stream = Response.OutputStream)
                    {
                        stream.Write(ReturnByteArr, 0, ReturnByteArr.Length);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        using (var stream = Response.OutputStream)
                        {
                            byte[] Content = Encoding.UTF8.GetBytes(ex.Message);
                            stream.Write(Content, 0, Content.Length);
                        }

                    }
                    catch
                    {
                        //Request Error
                    }
                }
            }
            catch { }
        }

        //Test Args -Debug -SetPort 11152
        //Test http://localhost:11152/SSEAT?Type=CloseService HttpGet 
        private object HandleRequest(HttpListenerRequest Request, HttpListenerResponse Response)
        {
            try
            {
                string GetType = Request.QueryString.Get("Type");

                switch (GetType)
                {
                    case "InitEngine":
                        {
                            Engine.Init();
                        }
                        break;
                    case "CloseService":
                        {
                            if (Listener != null)
                            {
                                try
                                {
                                    //Close Service
                                    Listener.Close();
                                    GC.SuppressFinalize(this);
                                }
                                catch { }
                            }
                            DeFine.ExitAny();
                        }
                        break;
                }


            }
            catch { return Return(-2, "Http Server Error!"); }

            return Return(-1, "RouteNotFound");
        }
    }
}
