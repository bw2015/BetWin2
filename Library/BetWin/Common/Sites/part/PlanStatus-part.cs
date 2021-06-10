using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Common.Sites
{
    /// <summary>
    /// 活动的运行状态
    /// </summary>
    partial class PlanStatus
    {
        /// <summary>
        /// 总共耗时
        /// </summary>
        public double Time
        {
            get
            {
                if (this.EndAt < this.CreateAt) return 0;
                return ((TimeSpan)(this.EndAt - this.CreateAt)).TotalSeconds;
            }
        }

        /// <summary>
        /// 当前的进度
        /// </summary>
        public double Progress
        {
            get
            {
                if (this.Total == 0) return 0D;

                return ((double)this.Count / (double)this.Total);
            }
        }
    }
}
