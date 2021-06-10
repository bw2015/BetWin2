using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;

using SP.Studio.Data;
using SP.Studio.Security;

namespace SP.Studio.ErrorLog
{
    /// <summary>
    /// IIS日志的导入
    /// </summary>
    public class IISLog
    {
        /// <summary>
        /// 把IIS日志导入数据库
        /// </summary>
        /// <returns></returns>
        public static int ReadLog(string dbconection, string table, string path, Action<string> action = null)
        {
            IEnumerable<string> log = File.ReadLines(path);
            string[] fields = null;
            int count = 0;
            using (LogDbAgent db = new LogDbAgent(dbconection, table))
            {
                foreach (string line in log)
                {
                    if (line.StartsWith("#Fields: "))
                    {
                        string field = line.Substring("#Fields: ".Length);
                        fields = field.Split(' ');
                        continue;
                    }
                    if (fields == null) continue;
                    string[] value = line.Split(' ');
                    if (value.Length == fields.Length)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            dic.Add(fields[i], value[i]);
                        }
                        count += db.ImportData(dic) ? 1 : 0;
                    }
                    if (action != null) action.Invoke(line);
                }
            }
            return count;
        }


        class LogDbAgent : DbAgent
        {
            private string _table;

            public LogDbAgent(string dbconection, string table) : base(dbconection, DatabaseType.SqlServer, DataConnectionMode.Instance) { this._table = table; }

            public bool ImportData(Dictionary<string, string> dic)
            {
                string key = MD5.Encryp(GetValue(dic, "date") + GetValue(dic, "date") + GetValue(dic, "c-ip") + GetValue(dic, "cs-uri-stem") + GetValue(dic, "cs-uri-query"));

                dic.Add("Key", key);

                List<string> fields = dic.Select(t => string.Format("[{0}]", t.Key)).ToList();
                List<string> param = new List<string>();
                int index = 0;
                List<DbParameter> paramList = new List<DbParameter>();
                foreach (KeyValuePair<string, string> keyValue in dic)
                {
                    param.Add(keyValue.Key == "Key" ? "@Key" : "@p" + index);
                    paramList.Add(NewParam(keyValue.Key == "Key" ? "@Key" : "@p" + index, keyValue.Value));
                    index++;
                }

                string sql = string.Format("IF NOT EXISTS(SELECT 0 FROM [{0}] WHERE [Key] = @Key) BEGIN INSERT INTO [{0}]({1}) VALUES({2}) END", _table, string.Join(",", dic.Select(t => string.Format("[{0}]", t.Key))),
                    string.Join(",", param));

                using (DbExecutor db = NewExecutor())
                {
                    return db.ExecuteNonQuery(CommandType.Text, sql,
                         paramList.ToArray()) != 0;
                }
            }

            private string GetValue(Dictionary<string, string> dic, string key)
            {
                return dic.ContainsKey(key) ? dic[key] : string.Empty;
            }
        }
    }
}
