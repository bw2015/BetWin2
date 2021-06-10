using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SP.Studio.Web
{
    /// <summary>
    /// 手机归属地查询
    /// </summary>
    public class MobileAgent
    {
        private readonly static string file;

        /// <summary>
        /// 静态构造 设置数据库位置
        /// </summary>
        static MobileAgent()
        {
            file = HttpContext.Current.Server.MapPath("~/Bin/Mobile.Dat");
        }
    }
}
