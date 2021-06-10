using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;

using SP.Studio.Data;
using SP.Studio.Model;
using SP.Studio.ErrorLog;

using BW.Common.Systems;
using BW.Common.Logs;
using BW.Common.Games;
using BW.GateWay.Games;
using BW.Framework;

using System.Web;
using SP.Studio.Web;

namespace BW.Agent
{
    /// <summary>
    /// 超级管理员管理
    /// </summary>
    public partial class SystemAgent : AgentBase<SystemAgent>
    {
        /// <summary>
        /// 获取系统通用的邀请链接
        /// </summary>
        /// <returns></returns>
        public List<InviteDomain> GetInviteDomain()
        {
            return BDC.InviteDomain.ToList();
        }

        /// <summary>
        /// 保存第三方游戏通信过程中的日志信息
        /// </summary>
        public void SaveGameGatewayLog(int userId, GameType type, string message, params string[] args)
        {
            int siteId = UserAgent.Instance().GetSiteID(userId);
            new GameGatewayLog()
            {
                SiteID = siteId,
                UserID = userId,
                Content = message,
                CreateAt = DateTime.Now,
                Type = type,
                LogData = WebAgent.GetPostLog(args)
            }.Add();
        }

        /// <summary>
        /// 插入系统日志
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="content"></param>
        public void AddSystemLog(int siteId, string content)
        {
            SNAPAgent.Instance().AddSystemLog(siteId, content);
        }


        /// <summary>
        /// 添加一条错误日志
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="ex"></param>
        /// <param name="title"></param>

        public void AddErrorLog(int siteId, Exception ex, string title = null)
        {
            string errorId;
            int httpCode;
            string content = ErrorAgent.CreateDetail(ex, out errorId, out httpCode);
            if (string.IsNullOrEmpty(title)) title = ex.Message;

            SNAPAgent.Instance().AddErrorLog(siteId, errorId, httpCode, content, title);
        }

        /// <summary>
        /// 通过查询存储过程，通用的报表结果
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerable<object[]> GetReportData(string procName, string database, params object[] args)
        {
            using (DbExecutor db = NewExecutor())
            {
                return this.GetReportDataInfo(db, procName, database, args).ToList();
            }
        }

        public IEnumerable<object[]> GetReportDataInfo(DbExecutor db, string procName, string database, params object[] args)
        {
            List<DbParameter> param = new List<DbParameter>();
            List<EnumObject> objs = this.GetReportInfo(db, procName, database);
            for (int i = 0; i < objs.Count; i++)
            {
                param.Add(NewParam("@" + objs[i].Name, args[i]));
            }
            if (!string.IsNullOrEmpty(database)) procName = database + procName;
            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, procName,
                param.ToArray());

            object[] header = new object[ds.Tables[0].Columns.Count];
            for (int i = 0; i < header.Length; i++)
            {
                header[i] = ds.Tables[0].Columns[i].ColumnName;
            }
            yield return header;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                object[] item = new object[ds.Tables[0].Columns.Count];
                for (int i = 0; i < header.Length; i++)
                {
                    item[i] = dr[i];
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取报表存储过程的参数列表
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public List<EnumObject> GetReportInfo(string procName, string database = null)
        {
            using (DbExecutor db = NewExecutor())
            {
                return this.GetReportInfo(db, procName, database);
            }
        }

        /// <summary>
        /// 指定数据库来源的报表存储过程参数获取
        /// </summary>
        /// <param name="db"></param>
        /// <param name="procName"></param>
        /// <returns></returns>
        public List<EnumObject> GetReportInfo(DbExecutor db, string procName, string database = null)
        {
            string helptext = "sp_helptext";
            if (!string.IsNullOrEmpty(database)) helptext = database + helptext;
            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, helptext,
                   NewParam("@objname", procName));

            List<string> list = new List<string>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                list.Add((string)dr[0]);
            }

            List<EnumObject> result = new List<EnumObject>();
            //@开始时间 DATE = '1900-1-1',

            Regex regex = new Regex(@"\@(?<Name>.+) (?<Type>.+?) \= '(?<DefaultValue>.{0,})'");
            foreach (Match match in regex.Matches(string.Join("\n", list)))
            {
                result.Add(new EnumObject()
                {
                    Name = match.Groups["Name"].Value,
                    Picture = match.Groups["Type"].Value,
                    Description = match.Groups["DefaultValue"].Value
                });
            }

            return result;
        }

        /// <summary>
        /// 从POST中获取参数值
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public object[] GetReportParameber(List<EnumObject> list)
        {
            return list.Select(t =>
            {
                if (t.Name == "SiteID") return (object)SiteInfo.ID;
                if (t.Picture == "DATE") return (object)WebAgent.QF(t.Name, DateTime.Now.Date);
                return (object)WebAgent.QF(t.Name);
            }).ToArray();
        }

        /// <summary>
        /// 执行存储过程，并且返回一个dataset
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public DataSet GetProcReport(string procName, params object[] param)
        {
            using (DbExecutor db = NewExecutor())
            {
                List<DbParameter> paramlist = new List<DbParameter>();
                for (int i = 0; i < param.Length; i += 2)
                {
                    paramlist.Add(NewParam("@" + param[i], param[i + 1]));
                }
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, procName, paramlist.ToArray());

                return ds;
            }
        }

        /// <summary>
        /// 获取游戏设置
        /// </summary>
        /// <param name="type"></param>
        /// <param name="settingString">自定义的参数</param>
        /// <returns></returns>
        public GameInterface GetGameInterfaceInfo(GameType type, string settingString)
        {
            GameInterface game = BDC.GameInterface.Where(t => t.Type == type).FirstOrDefault();
            if (game == null)
            {
                game = new GameInterface()
                {
                    Type = type,
                    IsOpen = false
                };
            }
            if (!string.IsNullOrEmpty(settingString) && string.IsNullOrEmpty(game.SettingString)) game.SettingString = settingString;
            return game;
        }

        /// <summary>
        /// 获取系统开放的第三方游戏列表
        /// </summary>
        /// <returns></returns>
        public List<GameInterface> GetGameInterfaceList()
        {
            return BDC.GameInterface.ToList();
        }

        /// <summary>
        /// 获取上次导入资金历史记录的时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetMoneyUpdateAt()
        {
            return BDC.SystemMark.Select(t => t.MoneyUpdateAt).FirstOrDefault();
        }

        /// <summary>
        /// 获取系统键值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SystemKeyValue GetSystemValue(string key)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT * FROM sys_KeyValue WHERE [Key] = @Key",
                    NewParam("@Key", key));
                if (ds.Tables[0].Rows.Count == 0) return null;
                return new SystemKeyValue(ds.Tables[0].Rows[0]);
            }
        }

        /// <summary>
        /// 添加或者更新系统键值
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public void SaveSystemKeyValue(SystemKeyValue keyValue)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "sys_UpdateSystemKeyValue",
                    NewParam("@Key", keyValue.Key),
                    NewParam("@Value", keyValue.Value));
            }
        }

        public void SaveSystemKeyValue(string key, string value)
        {
            this.SaveSystemKeyValue(new SystemKeyValue()
            {
                Key = key,
                Value = value
            });
        }
    }
}
