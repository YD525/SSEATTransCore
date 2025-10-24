using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.TranslateCore;
using PhoenixEngine.TranslateManage;
using PhoenixEngineR.TranslateManage;
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
        public Thread ServerThread = null;
        public HttpListener Listener;

        public static HttpCallBack CallBack = null;
        public delegate string HttpCallBack(HttpListenerRequest Request, HttpListenerResponse Response);

        public NameValueCollection GetPostData(HttpListenerRequest Request)
        {
            using (var Reader = new StreamReader(Request.InputStream, Request.ContentEncoding))
            {
                string Body = Reader.ReadToEnd();
                var FormData = HttpUtility.ParseQueryString(Body);

                return FormData;
            }
            return new NameValueCollection();
        }
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
                    ReturnObj = JsonCore.JsonHelper.GetJson(CallBack.Invoke(Request, Response));
                }
                else
                    {
                    ReturnObj = JsonCore.JsonHelper.GetJson(CallBack.Invoke(Request, Response));
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
    }
}
