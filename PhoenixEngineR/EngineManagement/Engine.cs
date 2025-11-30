using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using PhoenixEngine.ConvertManager;
using PhoenixEngine.DataBaseManagement;
using PhoenixEngine.TranslateCore;
using PhoenixEngine.TranslateManagement;
using PhoenixEngineR.RequestManagement;
using PhoenixEngineR.TranslateManage;
using static PhoenixEngine.TranslateManage.TransCore;

namespace PhoenixEngine.EngineManagement
{
    public class Engine
    {
        public static string Version = "1.1.71 - Lite";
        public static string CurrentPath = "";
        /// <summary>
        /// Instance of the local SQLite database helper.
        /// Represents the pointer/reference to the current local database.
        /// </summary>
        public static SQLiteHelper LocalDB = new SQLiteHelper();

        public static void Init()
        {
            CurrentPath = GetFullPath(@"\");

            string GetFilePath = GetFullPath(@"\Engine.db");

            if (!File.Exists(GetFilePath))
            {
                SQLiteConnection.CreateFile(GetFilePath);
            }

            LocalDB.OpenSql(GetFilePath);

            AdvancedDictionary.Init();

            CloudDBCache.Init();
            LocalDBCache.Init();
            FontColorFinder.Init();

            UniqueKeyHelper.Init();

            EngineConfig.Load();
            ProxyCenter.UsingProxy();
        }

        public static void Vacuum()
        {
            LocalDB.ExecuteNonQuery("vacuum");
        }

        public static string LastLoadFileName = "";

        public static void LoadFile(string FilePath,bool CanSkipFuzzyMatching = false)
        {
            UniqueKeyItem NewKey = new UniqueKeyItem();
            var UniqueKey = UniqueKeyHelper.AddItemByReturn(ref NewKey,FilePath,CanSkipFuzzyMatching);
            LastLoadFileName = NewKey.FileName;

            ChangeUniqueKey(UniqueKey);
        }

        public static string GetFullPath(string Path)
        {
            string GetShellPath = System.Windows.Forms.Application.StartupPath;
            if (GetShellPath.EndsWith(@"\"))
            {
                if (Path.StartsWith(@"\"))
                {
                    Path = Path.Substring(1);
                }
            }
            return GetShellPath + Path;
        }

        private static BatchTranslationCore TranslationCore = null;


        public static Languages From = Languages.Auto;

        public static Languages To = Languages.Null;

        public static bool ConfigLanguage(Languages SetFrom, Languages SetTo)
        {
            if (SetFrom != Languages.Null && SetTo != Languages.Null)
            {
                Engine.From = SetFrom;
                Engine.To = SetTo;
                return true;
            }
            return false;
        }

        private static int FileUniqueKey = 0;

        public static void ChangeUniqueKey(int Rowid)
        {
            FileUniqueKey = Rowid;
            GetTranslatedCount(FileUniqueKey);
        }

        public static int TranslatedCount = 0;
        public static int GetTranslatedCount(int FileUniqueKey)
        {
            string SqlOrder = $@"SELECT COUNT(*) AS TotalCount
FROM (
    SELECT Key
    FROM LocalTranslation
    WHERE FileUniqueKey = '{FileUniqueKey}' And [To] = '{(int)Engine.To}'
    
    UNION  
    SELECT Key
    FROM CloudTranslation
    WHERE FileUniqueKey = '{FileUniqueKey}' And [To] = '{(int)Engine.To}'
) AS Combined;";

            int GetCount = ConvertHelper.ObjToInt(Engine.LocalDB.ExecuteScalar(SqlOrder));

            TranslatedCount = GetCount;

            return GetCount;
        }
        public static int GetFileUniqueKey()
        {
            return Engine.FileUniqueKey;
        }

        public static void SkipWordAnalysis(bool Check)
        {
            if (TranslationCore != null)
            {
                TranslationCore.SkipWordAnalysis = Check;
            }
        }

        public static void Start()
        {
            Start(false);
        }

        public static int Start(bool ClearCache)
        {
            if (From != Languages.Null && To != Languages.Null)
            {
                if (TranslationCore == null)
                {

                    TranslationCore = new BatchTranslationCore(Engine.From, Engine.To, new List<TranslationUnit>() { }, ClearCache);
                }

                TranslationCore.Start();
                return 1;
            }

            return -1;
        }

        public static int Stop(bool State)
        {
            if (TranslationCore != null)
            {
                if (State)
                {
                    TranslationCore.Stop();
                    return 1;
                }
                else
                {
                    TranslationCore.Keep();
                    return 0;
                }
            }
            return -1;
        }

        public static int End()
        {
            if (TranslationCore != null)
            {
                TranslationCore.Close();
                return 1;
            }
            return 0;
        }

        public static int GetThreadCount()
        {
            if (TranslationCore != null)
            {
                return TranslationCore.ThreadUsage.CurrentThreads;
            }

            return 0;
        }

        public static void InitTranslationCore(Languages From, Languages To)
        {
            TranslationCore = new BatchTranslationCore(From, To, new List<TranslationUnit>() { });
        }
        public static void ClearUnits()
        {
            if (TranslationCore != null)
            {
                TranslationCore.UnitsToTranslate.Clear();
            }
        }
        public static int GetUnitCount()
        {
            if (TranslationCore != null)
            {
                return TranslationCore.UnitsToTranslate.Count;
            }

            return -1;
        }

        private static object AddTranslationUnitLocker = new object();
        public static int AddTranslationUnit(TranslationUnit Item)
        {
            if (TranslationCore == null)
            {
                return -1;
            }

            lock (AddTranslationUnitLocker)
            {
                TranslationCore.UnitsToTranslate.Add(Item);
                return TranslationCore.UnitsToTranslate.Count;
            }
        }
        public static TranslationUnit DequeueTranslated(ref bool IsEnd)
        {
            if (TranslationCore != null)
            {
                var GetItem = TranslationCore.DequeueTranslated(out bool TranslationEnd);
                IsEnd = TranslationEnd;

                return GetItem;
            }
            else
            {
                IsEnd = true;
            }

            return null;
        }

        public static void AddAIMemory(string Original, string Translated)
        {
            EngineSelect.AIMemory.AddTranslation(Engine.From, Original, Translated);
        }

        public static string AppendDollarWrappedReplacements(string input)
        {
            // Create a regex to match text wrapped in $$...$$
            Regex OneRegex = new Regex(@"\$\$(.+?)\$\$");

            // Replace each match with {content}
            string Replaced = OneRegex.Replace(input, match => "{" + match.Groups[1].Value + "}");

            // Return the processed text only (original text is not preserved)
            return Replaced;
        }
    }
}
