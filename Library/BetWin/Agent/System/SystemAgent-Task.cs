using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using SP.Studio.Data;

using BW.Common.Sites;
using BW.Common.Users;

namespace BW.Agent
{
    /// <summary>
    /// 系统自动执行的任务（均在非web环境下运行）
    /// </summary>
    partial class SystemAgent
    {
        private static object _lockNobankUser = new object();

        /// <summary>
        /// 执行锁定任务
        /// </summary>
        public void TaskLockNoBankUser()
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now > 60)
            {
                SystemAgent.Instance().AddSystemLog(0,"TaskLockNoBankUser 锁定未绑卡用户 未到执行时间");
                return;
            }

            int sourceId = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
            try
            {
                lock (_lockNobankUser)
                {
                    List<Site> siteList = BDC.Site.ToList();

                    foreach (Site site in siteList.Where(t => t.Setting.LockNoBank > 0))
                    {
                        TaskStatus task = SiteAgent.Instance().GetTaskStatus(site.ID, TaskStatus.TaskType.LockNoBank, sourceId);
                        if (task != null && task.Total == task.Count)
                        {
                            continue;
                        }
                        DateTime date = DateTime.Now.Date.AddDays(site.Setting.LockNoBank * -1);

                        using (DbExecutor db = NewExecutor())
                        {
                            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetNoBankUser",
                                NewParam("@SiteID", site.ID),
                                NewParam("@Date", date));
                            List<int> userlist = ds.ToList<int>();

                            if (task == null)
                            {
                                task = SiteAgent.Instance().AddTaskStatus(site.ID, TaskStatus.TaskType.LockNoBank, sourceId, userlist.Count);
                            }

                            List<int> success = new List<int>();
                            List<int> faild = new List<int>();

                            foreach (int userId in userlist)
                            {
                                if (UserAgent.Instance().UpdateUserLockStatus(userId, User.LockStatus.Login, true))
                                {
                                    success.Add(userId);
                                }
                                else
                                {
                                    faild.Add(userId);
                                }
                                task.Count++;
                                SiteAgent.Instance().UpdateTaskStatus(db, task);
                            }
                            this.AddSystemLog(site.ID, string.Format("锁定未绑卡用户任务执行完毕，成功：{0}，失败：{1}", string.Join(",", success), string.Join(",", faild)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(0, ex);
            }
        }
    }
}
