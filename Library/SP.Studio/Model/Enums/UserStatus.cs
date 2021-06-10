using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Model.Enums
{
    /// <summary>
    /// 用户的通用状态
    /// </summary>
    public enum UserStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("锁定")]
        Lock = 1,
        [Description("删除")]
        Deleted = 10
    }
}
