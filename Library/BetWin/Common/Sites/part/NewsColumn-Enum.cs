using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Common.Sites
{
    partial class NewsColumn
    {
        /// <summary>
        /// 栏目类型
        /// </summary>
        public enum ContentType : byte
        {
            /// <summary>
            /// 新闻公告
            /// </summary>
            News = 0,
            /// <summary>
            /// 帮助中心
            /// </summary>
            Help = 1,
            /// <summary>
            /// 活动
            /// </summary>
            Activity = 2
        }
    }
}
