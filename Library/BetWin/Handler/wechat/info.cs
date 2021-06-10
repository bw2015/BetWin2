using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using BW.Agent;
using BW.Common.Wechat;

using SP.Studio.Core;
using SP.Studio.Model;
using BW.Common.Sites;

namespace BW.Handler.wechat
{
    public class info : IHandler
    {
        /// <summary>
        /// 获取微信公共号的设置信息
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void get(HttpContext context)
        {
            WechatSetting setting = WechatAgent.Instance().GetWechatSetting(SiteInfo.ID);
            string url = QF("redirect");
            if (string.IsNullOrEmpty(url)) url = context.Request.UrlReferrer.ToString();

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                setting.FaceShow,
                setting.Name,
                setting.SiteID,
                AuthorizeUrl = setting.IsWechat() ? setting.Setting.GetAuthorizeUrl(url) : string.Empty
            });
        }
    }
}
