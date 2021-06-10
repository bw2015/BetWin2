using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BW.Common.Sites
{
    /// <summary>
    /// 任务类型
    /// </summary>
    partial class TaskStatus
    {
        /// <summary>
        /// 任务类型
        /// </summary>
        public enum TaskType : byte
        {
            /// <summary>
            /// 锁定未绑卡且没有流水记录的用户
            /// </summary>
            [Description("未绑卡锁定")]
            LockNoBank = 1
        }

        /// <summary>
        /// 当前进度
        /// </summary>
        public decimal Progress
        {
            get
            {
                if (this.Total == 0) return decimal.Zero;
                return (decimal)this.Count / (decimal)this.Total;
            }
        }

        /// <summary>
        /// 总耗时（秒）
        /// </summary>
        public double Time
        {
            get
            {
                if (this.EndAt.Year < 2000) return 0;
                return ((TimeSpan)(this.EndAt - this.StartAt)).TotalSeconds;
            }
        }
    }
}
