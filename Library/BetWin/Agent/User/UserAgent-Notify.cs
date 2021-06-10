using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using SP.Studio.Data;
using BW.Framework;
using BW.Common.Users;

namespace BW.Agent
{
    partial class UserAgent
    {
        /// <summary>
        /// 增加一个系统通知
        /// </summary>
        public void AddNotify(int userId, UserNotify.NotifyType type, string message, params object[] args)
        {
            if (args.Length != 0) message = string.Format(message, args);
            using (DbExecutor db = NewExecutor())
            {
                new UserNotify()
                {
                    SiteID = this.GetSiteID(userId, db),
                    UserID = userId,
                    Type = type,
                    Message = message,
                    CreateAt = DateTime.Now
                }.Add(db);
            }
        }

        public void AddNotify(DbExecutor db, int userId, UserNotify.NotifyType type, string message, params object[] args)
        {
            if (args.Length != 0) message = string.Format(message, args);
            new UserNotify()
            {
                SiteID = this.GetSiteID(userId, db),
                UserID = userId,
                Type = type,
                Message = message,
                CreateAt = DateTime.Now
            }.Add(db);
        }

        /// <summary>
        /// 获取未读的系统通知
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<UserNotify> GetNotifyList(int siteId)
        {
            return BDC.UserNotify.Where(t => t.SiteID == siteId && t.CreateAt > DateTime.Now.AddMinutes(-10) && !t.IsRead).ToList();
        }

        /// <summary>
        /// 获取10分钟内未读的通知信息
        /// </summary>
        /// <returns></returns>
        public List<UserNotify> GetNotifyList()
        {
            return BDC.UserNotify.Where(t => t.CreateAt > DateTime.Now.AddMinutes(-10) && !t.IsRead).ToList();
        }

        /// <summary>
        /// 根据用户ID获取系统通知（获取之后设置为已读）
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<UserNotify> GetNotifyList(int siteId, int userId)
        {
            List<UserNotify> list = new List<UserNotify>();
            using (DbExecutor db = NewExecutor())
            {
                foreach (DataRow dr in db.GetDataSet(CommandType.StoredProcedure, "IM_GetNotify",
                    NewParam("@UserID", userId)).Tables[0].Rows)
                {
                    list.Add(new UserNotify(dr));
                }
            }
            this.UpdateNotifyRead(list);
            return list;
        }

        /// <summary>
        ///  删除已读的系统通知
        /// </summary>
        /// <param name="list"></param>
        public void UpdateNotifyRead(List<UserNotify> list)
        {
            if (list == null || list.Count == 0) return;
            IEnumerable<int> readlist = list.Select(t => t.ID);
            string sql = "DELETE FROM usr_Notify WHERE NotifyID IN ({0})";
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, string.Format(sql, string.Join(",", readlist)));
            }
        }
    }
}
