using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

namespace BW.Common.Users
{
    partial class ChatLog
    {
        public enum UserType : byte
        {
            [Description("用户")]
            User = 0,
            [Description("管理员")]
            Admin = 1,
            [Description("游客")]
            Guest = 2,
            [Description("机器人")]
            Rebot = 3
        }

        /// <summary>
        /// 对话类型
        /// </summary>
        public enum ChatType : byte
        {
            /// <summary>
            /// 错误的类型
            /// </summary>
            None = 0,
            [Description("用户与用户")]
            User = 1,
            [Description("用户与管理员")]
            UserAdmin = 2,
            [Description("游客与管理员")]
            GuestAdmin = 3,
            [Description("管理员与管理员")]
            Admin = 4
        }
    }
}
