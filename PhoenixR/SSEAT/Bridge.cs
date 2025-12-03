using System.IO;
using Newtonsoft.Json;

namespace PhoenixEngineR.SSEAT
{
    public class Bridge
    {
        public static string SkyrimPath = "";
        public static bool FindPapyrusCompilerPath(ref string CompilerPathPtr)
        {
            if (File.Exists(Bridge.GetFullPath(@"Tool\Original Compiler\PapyrusAssembler.exe")))
            {
                CompilerPathPtr = Bridge.GetFullPath(@"Tool\Original Compiler\PapyrusAssembler.exe");
                return true;
            }
            if (Directory.Exists(SkyrimPath))
            {
                if (!SkyrimPath.EndsWith(@"\"))
                {
                    SkyrimPath += @"\";
                }
                string SetPapyrusAssemblerPath = SkyrimPath + "Papyrus Compiler" + @"\PapyrusAssembler.exe";
                if (File.Exists(SetPapyrusAssemblerPath))
                {
                    CompilerPathPtr = SetPapyrusAssemblerPath;
                    return true;
                }
            }
            return false;
        }

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

        public static string StartupPath = "";
        public static string GetFullPath(string Path)
        {
            string GetShellPath = StartupPath;
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