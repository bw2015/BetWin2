using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Model
{
    /// <summary>
    /// 标记当前方法可被游客访问
    /// </summary>
    public class GuestAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记当前方法只能管理员访问
    /// </summary>
    public class AdminAttribute : Attribute
    {

    }

    /// <summary>
    /// 标记当前方法只有登录用户才能访问
    /// </summary>
    public class UserAttribute : Attribute
    {

    }

    /// <summary>
    /// 系统管理员标记
    /// </summary>
    public class SystemAdminAttribute : Attribute { }
}
