using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSEATTransCore.SkyrimManagement
{
    public class SkyrimHelper
    {
        public static string SkyrimPath = "";
        public static bool FindPapyrusCompilerPath(ref string CompilerPathPtr)
        {
            if (File.Exists(DeFine.GetFullPath(@"Tool\Original Compiler\PapyrusAssembler.exe")))
            {
                CompilerPathPtr = DeFine.GetFullPath(@"Tool\Original Compiler\PapyrusAssembler.exe");
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
    }
}
