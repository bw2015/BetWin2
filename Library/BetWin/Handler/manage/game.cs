using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using SP.Studio.Web;
using SP.Studio.Model;
using SP.Studio.Core;

namespace BW.Handler.manage
{
    /// <summary>
    /// 全局游戏设置管理
    /// </summary>
    public class game : IHandler
    {
        /// <summary>
        /// 游戏列表
        /// </summary>
        /// <param name="context"></param>
        private void gamelist(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(BDC.GameInterface, t => new
            {
                t.Type,
                t.IsOpen,
                Setting = new JsonString(t.Setting.ToJson())
            }));
            // BDC.GameInterface
        }
    }
}
