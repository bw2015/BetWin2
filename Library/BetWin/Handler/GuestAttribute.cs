using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Handler
{
    /// <summary>
    /// 设定游客可访问，否则默认的方法均需要登录才可以访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class GuestAttribute : Attribute
    {
    }
}
