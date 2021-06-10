using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Model
{
    /// <summary>
    /// 客户端的通知
    /// </summary>
    public class Notification
    {
        public string body { get; set; }

        public string icon { get; set; }

        public string tag { get; set; }

        /// <summary>
        /// 点击要跳转的网址
        /// </summary>
        public string url { get; set; }
    }
}
