using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Common.Users;

namespace BW.Common
{
    partial class BetDataContext
    {
        /// <summary>
        /// 会话记录（来自视图）
        /// </summary>
        public Table<ChatTalk> ChatTalk
        {
            get
            {
                return this.GetTable<ChatTalk>();
            }
        }

    }
}
