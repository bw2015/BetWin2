using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using SP.Studio.Data;
using BW.Common.Sites;

namespace BW.Agent
{
    /// <summary>
    /// 站点的任务执行（注意在非web环境下的运行状态）
    /// </summary>
    partial class SiteAgent
    {
        /// <summary>
        /// 获取任务执行状态
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public TaskStatus GetTaskStatus(int siteId, TaskStatus.TaskType type, int sourceId)
        {
            return BDC.TaskStatus.Where(t => t.SiteID == siteId && t.Type == type && t.SourceID == sourceId).FirstOrDefault();
        }

        /// <summary>
        /// 创建一条任务
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public TaskStatus AddTaskStatus(int siteId, TaskStatus.TaskType type, int sourceId, int total)
        {
            using (DbExecutor db = NewExecutor())
            {
                TaskStatus task = new TaskStatus() { SiteID = siteId, Type = type, SourceID = sourceId }.Info(db, t => t.SiteID, t => t.Type, t => t.SourceID);
                if (task != null) return task;
                task = new TaskStatus()
                {
                    SiteID = siteId,
                    Type = type,
                    SourceID = sourceId,
                    Total = total,
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now,
                    Count = 0
                };

                task.Add(db);

                return task;
            }
        }

        /// <summary>
        /// 更新任务进度
        /// </summary>
        /// <param name="status"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool UpdateTaskStatus(DbExecutor db, TaskStatus status)
        {
            status.EndAt = DateTime.Now;
            return status.Update(db, t => t.EndAt, t => t.Count) != 0;
        }

        /// <summary>
        /// 更新任务进度（自动加1)
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool UpdateTaskStatus(TaskStatus status)
        {
            using (DbExecutor db = NewExecutor())
            {
                status.Count++;
                return this.UpdateTaskStatus(db, status);
            }
        }
    }
}
