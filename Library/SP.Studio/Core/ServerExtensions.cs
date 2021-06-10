using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SP.Studio.Core
{
    /// <summary>
    /// 服务端操作对象的扩展方法
    /// </summary>
    public static class ServerExtensions
    {
        /// <summary>
        /// 当前的web服务器是否是IIS
        /// </summary>
        public static bool IsIIS(this HttpServerUtility server)
        {
            return HttpContext.Current.Request.ServerVariables["Server_Software"].StartsWith("Microsoft-IIS");
        }
    }
}
