using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BW.Agent;
using SP.Studio.Model;

namespace BW.Handler.user
{
    /// <summary>
    /// 微信接口
    /// </summary>
    public class wechat : IHandler
    {
        /// <summary>
        /// 获取绑定微信帐号的随机验证码
        /// </summary>
        /// <param name="context"></param>
        private void getguid(HttpContext context)
        {
            Guid key = UserAgent.Instance().GetWechatKey(UserInfo.ID);
            if (key == Guid.Empty)
            {
                context.Response.Write(false, "已绑定微信号");
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Key = key.ToString("N")
            });
        }
    }
}
