using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using PhoenixEngine.ConvertManager;
using PhoenixEngine.TranslateManage;
using SSELex.SkyrimManage;
using static PhoenixEngine.SSELexiconBridge.NativeBridge;

namespace PhoenixEngineR.SSEAT
{
    public class Bridge
    {
        public class Empty
        {

        }

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

        public static object Return(int Code)
        {
            return Return<Empty>(Code, string.Empty, new Empty());
        }
        public static object Return(int Code, string Message)
        {
            return Return<Empty>(Code, Message, new Empty());
        }
        public static object Return<T>(int Code, string Message, T Data) where T : new()
        {
            return new Result<T>(Code, Message, Data);
        }
        public static string GetJson(object Any)
        {
            return JsonConvert.SerializeObject(Any);
        }
        public static string GetFullPath(string Path)
        {
            string GetShellPath = System.Windows.Forms.Application.StartupPath;
            if (GetShellPath.EndsWith(@"\"))
            {
                GetShellPath = GetShellPath.Substring(0, GetShellPath.Length - 1);
            }
            if (!Path.StartsWith(@"\"))
            {
                Path = @"\" + Path;
            }
            return GetShellPath + Path;
        }

    }
}
