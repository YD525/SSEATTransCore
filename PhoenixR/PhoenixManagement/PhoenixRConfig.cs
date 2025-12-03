using System.IO;
using System.Text;

namespace PhoenixEngine.EngineManagement
{
    public class ThreadUsageInfo
    {
        public int CurrentThreads { get; set; } = 0;
        public int MaxThreads { get; set; } = 0;
    }

    public class PhoenixRConfig
    {
        #region RequestConfig

        /// <summary>
        /// Configured http proxy or local proxy for network requests.
        /// </summary>
        public static string ProxyUrl { get; set; } = "";

        public static string ProxyUserName { get; set; } = "";

        public static string ProxyPassword { get; set; } = "";

        /// <summary>
        /// Global maximum timeout duration (in milliseconds) for network requests.
        /// </summary>
        public static int GlobalRequestTimeOut { get; set; } = 8000;

        #endregion

        #region Platform Enable State

        /// <summary>
        /// Flags indicating whether each AI or translation platform is enabled.
        /// Multiple platforms can be enabled simultaneously, and the system will perform load balancing among them.
        /// </summary>

        public static bool LMLocalAIEnable { get; set; } = false;

        #endregion

        #region ApiKey Set

        /// <summary>
        /// LM Studio
        /// </summary>
        public static string LMHost { get; set; } = "http://localhost";
        public static int LMPort { get; set; } = 1234;
        public static string LMModel { get; set; } = "google/gemma-3-12b";

        #endregion

        #region EngineSetting

        /// <summary>
        /// Indicates whether to enable context-based generation.
        /// If true, the process will consider contextual information;  
        /// if false, it will only handle the current string without any context.
        /// </summary>
        public static bool ContextEnable { get; set; } = true;

        /// <summary>
        /// Specifies the maximum number of context characters to include during generation.
        /// For example, if set to 200, the total character count of all context lines will not exceed 200.
        /// </summary>
        public static int ContextLimit { get; set; } = 150;

        /// <summary>
        /// User-defined custom prompt sent to the AI model.
        /// This prompt can be used to guide the AI's behavior or translation style.
        /// </summary>
        public static string UserCustomAIPrompt { get; set; } = "";
      

        private static readonly byte[] XorKey = Encoding.UTF8.GetBytes("PhoenixR");

        private static byte[] XOREncrypt(byte[] data)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ XorKey[i % XorKey.Length]);
            }
            return result;
        }

        private static byte[] XORDecrypt(byte[] data)
        {
            return XOREncrypt(data);
        }

        //Use Xor to easily encrypt and store user API keys to ensure security
        public static void Save()
        {
            using (var Ms = new MemoryStream())
            using (var Writer = new BinaryWriter(Ms, Encoding.UTF8, true))
            {
                Writer.Write(ProxyUrl ?? "");
                Writer.Write(ProxyUserName ?? "");
                Writer.Write(ProxyPassword ?? "");

                Writer.Write(GlobalRequestTimeOut);

                Writer.Write(LMLocalAIEnable);

                Writer.Write(LMHost ?? "");
                Writer.Write(LMPort);
                Writer.Write(LMModel ?? "");

                Writer.Write(ContextEnable);
                Writer.Write(ContextLimit);
                Writer.Write(UserCustomAIPrompt ?? "");

                Writer.Flush();

                var PlainBytes = Ms.ToArray();
                var EncryptedBytes = XOREncrypt(PlainBytes);
                File.WriteAllBytes(PhoenixR.CurrentPath + "EngineConfig.data", EncryptedBytes);
            }
        }

        public static void Load()
        {
            string SetFullPath = PhoenixR.CurrentPath + "EngineConfig.data";
            if (!File.Exists(SetFullPath))
            {
                Save();
                return;
            }

            try
            {
                var EncryptedBytes = File.ReadAllBytes(SetFullPath);
                var PlainBytes = XORDecrypt(EncryptedBytes);

                using (var Ms = new MemoryStream(PlainBytes))
                using (var Reader = new BinaryReader(Ms, Encoding.UTF8, true))
                {
                    ProxyUrl = Reader.ReadString();
                    ProxyUserName = Reader.ReadString();
                    ProxyPassword = Reader.ReadString();
                    
                    GlobalRequestTimeOut = Reader.ReadInt32();

                    LMLocalAIEnable = Reader.ReadBoolean();

                    LMHost = Reader.ReadString();
                    LMPort = Reader.ReadInt32();
                    LMModel = Reader.ReadString();

                    ContextEnable = Reader.ReadBoolean();
                    ContextLimit = Reader.ReadInt32();
                    UserCustomAIPrompt = Reader.ReadString();
                }

            }
            catch
            {
                Save();
            }

            PhoenixRConfig.LMModel = "(Auto)";
        }

        #endregion
    }
}
