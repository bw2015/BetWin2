using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using SP.Studio.Web;

namespace BW.Handler.admin
{
    /// <summary>
    /// 接口
    /// </summary>
    public class api : IHandler
    {
        /// <summary>
        /// 后台进入电竞api接口的方法
        /// </summary>
        /// <param name="context"></param>
        private void esport(HttpContext context)
        {
            context.Response.Write(WebAgent.QS("api"));
        }
    }
}
