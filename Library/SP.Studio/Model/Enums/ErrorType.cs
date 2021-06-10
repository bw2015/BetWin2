using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Model.Enums
{
    public enum ErrorType
    {
        [Description("请先登录")]
        Login,
        [Description("没有权限")]
        Permission,
        [Description("地区限制")]
        IP
    }
}
