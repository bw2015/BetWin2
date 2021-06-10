using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BW.IM.Common
{
    /// <summary>
    /// 对话类型
    /// </summary>
    public enum TalkType : byte
    {
        [Description("未指定类型")]
        None = 0,
        [Description("用户与客服")]
        Admin2User = 1,
        [Description("游客与客服")]
        Admin2Guest = 2,
        [Description("用户与用户")]
        User2User = 3,
        [Description("管理员与管理员")]
        Admin2Admin = 4,
        /// <summary>
        /// 群聊
        /// </summary>
        [Description("群聊")]
        Group = 5
    }

    /// <summary>
    /// 用户类型
    /// </summary>
    public enum ChatType : byte
    {
        /// <summary>
        /// 游客
        /// </summary>
        GUEST,
        /// <summary>
        /// 会员
        /// </summary>
        USER,
        /// <summary>
        /// 管理员
        /// </summary>
        ADMIN,
        /// <summary>
        /// 群
        /// </summary>
        GROUP
    }
}
