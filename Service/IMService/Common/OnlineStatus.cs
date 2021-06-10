using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMService.Common
{
    public class OnlineStatus
    {
        public OnlineStatus()
        {
            this.ActiveTime = DateTime.Now;
        }

        /// <summary>
        /// 用户标识
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 在线时间
        /// </summary>
        public DateTime ActiveTime { get; set; }
    }
}
