﻿using System;
using System.Data;
using PhoenixEngine.ConvertManager;
using PhoenixEngine.EngineManagement;

namespace PhoenixEngine.TranslateCore
{
    public class CloudDBCache
    {
        public static void Init()
        {
            string CheckTableSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='CloudTranslation';";
            var Result = Engine.LocalDB.ExecuteScalar(CheckTableSql);

            if (Result == null || Result == DBNull.Value)
            {
                string CreateTableSql = @"
CREATE TABLE [CloudTranslation](
  [FileUniqueKey] INT, 
  [Key] TEXT, 
  [To] INT, 
  [Result] TEXT
);";
                Engine.LocalDB.ExecuteNonQuery(CreateTableSql);
            }
        }

        public static bool DeleteCache(int FileUniqueKey,string Key,Languages TargetLanguage)
        {
            try
            {
                string SqlOrder = "Delete From CloudTranslation Where [FileUniqueKey] = {0} And [Key] = '{1}' And [To] = {2}";

                int State = Engine.LocalDB.ExecuteNonQuery(string.Format(SqlOrder, FileUniqueKey, Key,(int)TargetLanguage));

                if (State!=0)
                {
                    return true;
                }

                return false;
            }
            catch { return false; }
        }
        public static string FindCache(int FileUniqueKey, string Key, Languages TargetLanguage)
        {
            try { 
            string SqlOrder = "Select Result From CloudTranslation Where [FileUniqueKey] = '{0}' And [Key] = '{1}' And [To] = {2}";

            string GetResult = ConvertHelper.ObjToStr(Engine.LocalDB.ExecuteScalar(string.Format(SqlOrder, FileUniqueKey, Key,(int)TargetLanguage)));

            if (GetResult.Trim().Length > 0)
            {
                return System.Web.HttpUtility.HtmlDecode(GetResult);
            }

            return string.Empty;
            }
            catch { return string.Empty; }
        }

        public static bool AddCache(int FileUniqueKey, string Key, int To,string Result)
        {
            try {

            int GetRowID = ConvertHelper.ObjToInt(Engine.LocalDB.ExecuteScalar(String.Format("Select Rowid From CloudTranslation Where [FileUniqueKey] = {0} And [Key] = '{1}' And [To] = {2}", FileUniqueKey, Key,To)));

            if (GetRowID < 0)
            {
                string SqlOrder = "Insert Into CloudTranslation([FileUniqueKey],[Key],[To],[Result])Values({0},'{1}',{2},'{3}')";

                int State = Engine.LocalDB.ExecuteNonQuery(string.Format(SqlOrder, FileUniqueKey, Key, To, System.Web.HttpUtility.HtmlEncode(Result)));

                if (State != 0)
                {
                    return true;
                }

                return false;
            }

            return false;
            }
            catch { return false; }
        }


        public static string FindCacheAndID(int FileUniqueKey, string Key, int To,ref int ID)
        {
            try { 
            string SqlOrder = "Select Rowid,Result From CloudTranslation Where [FileUniqueKey] = {0} And [Key] = '{1}' And [To] = {2}";

            DataTable GetResult = Engine.LocalDB.ExecuteQuery(string.Format(SqlOrder,FileUniqueKey,Key,To));

            if (GetResult.Rows.Count > 0)
            {
                string GetStr = System.Web.HttpUtility.HtmlDecode(ConvertHelper.ObjToStr(GetResult.Rows[0]["Result"]));
                ID = ConvertHelper.ObjToInt(GetResult.Rows[0]["Rowid"]);
                return GetStr;
            }

            return string.Empty;
            }
            catch {return string.Empty; }
        }

        public static bool DeleteCacheByID(int Rowid)
        {
            try {
            string SqlOrder = "Delete From CloudTranslation Where Rowid = {0}";
            int State = Engine.LocalDB.ExecuteNonQuery(string.Format(SqlOrder,Rowid));
            if (State != 0)
            {
                return true;
            }
            return false;
            }
            catch { return false; }
        }

        public static bool ClearCloudCache(int FileUniqueKey)
        {
            string SqlOrder = "Delete From CloudTranslation Where [FileUniqueKey] = " + FileUniqueKey + "";
            int State = Engine.LocalDB.ExecuteNonQuery(SqlOrder);
            if (State != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
