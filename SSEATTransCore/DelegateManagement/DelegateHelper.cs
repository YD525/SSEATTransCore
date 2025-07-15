using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSEATTransCore.DelegateManagement
{
    public class DelegateHelper
    {
        public delegate void Log(string Str, DateTime Time);

        public static Log SetLog = null;
    }
}
