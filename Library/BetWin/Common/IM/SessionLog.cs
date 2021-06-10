using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

using BW.Common.Users;

namespace BW.Common.IM
{
    /// <summary>
    /// 聊天记录（会话记录，来自视图）
    /// </summary>
    [Table(Name = "IM_SessionLog")]
    public class SessionLog
    {
        /// <summary>
        /// 站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 上次会话时间
        /// </summary>
        [Column(Name = "LastAt")]
        public DateTime LastAt { get; set; }

        /// <summary>
        /// 会话Key值
        /// </summary>
        [Column(Name = "Key")]
        public string Key { get; set; }

        /// <summary>
        /// 会话数量
        /// </summary>
        [Column(Name = "iCount")]
        public int Count { get; set; }

        /// <summary>
        /// 会话类型
        /// </summary>
        public ChatLog.ChatType Type
        {
            get
            {
                return (ChatLog.ChatType)byte.Parse(Key.Substring(0, 1));
            }
        }
    }
}
