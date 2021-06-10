using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using SP.Studio.Model;
using SP.Studio.Core;
using BW.IM.Common;
using BW.IM.Framework;
using BW.IM.Agent;
using SP.Studio.Web;

namespace BW.IM.Handler
{
    public class layim : IHandler
    {
        /// <summary>
        /// 输出初始化layim所需要的json数据
        /// </summary>
        /// <param name="context"></param>
        private void init(HttpContext context)
        {
            if (this.UserInfo != null)
            {
                Utils.output(context, UserInfo.Init());
            }
            Utils.showerror(context, "没有登录");
        }

        private void groupinfo(HttpContext context)
        {
            if (this.UserInfo == null)
            {
                Utils.showerror(context, "没有登录");
            }

            string type = WebAgent.QF("Type");
            if (string.IsNullOrEmpty(type))
            {
                Utils.showerror(context, "未指定游戏类型");
            }

            GroupType game = type.ToEnum<GroupType>();

            Utils.output(context, UserInfo.Init(game));
        }

        /// <summary>
        /// 查看当前的socket链接信息
        /// </summary>
        /// <param name="context"></param>
        private void socket(HttpContext context)
        {
            context.Response.Write(true, "链接数量：" + Utils.SOCKETLIST.Count, new
            {
                SocketList = new JsonString("{", string.Join(",", Utils.SOCKETLIST.Select(t =>
                {
                    string state = null;
                    try
                    {
                        Utils.Send(t.Key, new BW.IM.Factory.Message.Ping());
                        state = t.Value.State.ToString();
                    }
                    catch (Exception ex)
                    {
                        state = ex.Message;
                    }
                    return string.Format("\"{0}\":\"{1}\"", t.Key, state);
                })),
                    "}")
            });
        }

        /// <summary>
        /// 查看当前在线的会员
        /// </summary>
        /// <param name="context"></param>
        private void users(HttpContext context)
        {
            context.Response.Write(true, "用户数量：" + Utils.USERLIST.Count, new
            {
                SocketList = new JsonString("[", string.Join(",", Utils.USERLIST.Select(t =>
                {
                    return t.ToJson(p => p.SiteID, p => p.ID, p => p.Name, p => p.KEY, p => p.Group, p => p.Error);
                })),
                    "]")
            });
        }

        /// <summary>
        /// 查看当前缓存中的群参数设置
        /// </summary>
        /// <param name="context"></param>
        private void groupsetting(HttpContext context)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<int, GroupSetting> item in Utils.GROUPSETTING)
            {
                list.Add(string.Format("\"{0}\":{1}", item.Key, item.Value.ToJson(t => t.BetMessage, t => t.Chat)));
            }
            context.Response.Write(true, Utils.GROUPSETTING.Count.ToString(), string.Concat("{", string.Join(",", list), "}"));
        }

        /// <summary>
        /// 检测当前投注的开启状态
        /// </summary>
        /// <param name="context"></param>
        private void stopstatus(HttpContext context)
        {
            context.Response.Write(true, Utils.STOPSTATUS.Count.ToString(), Utils.STOPSTATUS.ToJson());
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="context"></param>
        private void uploadimage(HttpContext context)
        {
            string msg;
            bool success = base.upload(context, "upload", out msg);
            if (!success)
            {
                this.layimerror(context, 1, msg);
            }
            else
            {
                this.layimerror(context, 0, "图片上传成功", SysSetting.GetSetting().imgServer + msg);
            }
        }

        /// <summary>
        /// 查看当前群组内的成员
        /// </summary>
        /// <param name="context"></param>
        private void member(HttpContext context)
        {
            string id = WebAgent.QF("id");
            User admin = UserAgent.Instance().GetAdminInfo(context);
            string key = UserAgent.Instance().GetTalkKey(id, this.UserInfo.KEY);
            string type = Utils.GetChatID(key);
            GroupType group = type.ToEnum<GroupType>();
            if (group == GroupType.None)
            {
                Utils.showerror(context, "类型错误");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"code\": 0,\"msg\":\"\",\"data\":{\"list\":[");
            sb.Append(string.Join(",",
            Utils.USERLIST.FindAll(t => t.Group == group && t.SiteID == admin.SiteID).Select(t =>
            {
                return string.Format("{{\"username\":\"{0}\",\"id\":\"{1}\",\"avatar\":\"{2}\",\"sign\":\"{3}\"}}",
                    HttpUtility.JavaScriptStringEncode(t.Name),
                    Utils.GetTalkKey(this.UserInfo.KEY, t.KEY),
                    t.Face, HttpUtility.JavaScriptStringEncode(t.Sign)
                    );
            })));
            sb.Append("]}}");
            Utils.output(context, sb.ToString());
        }
    }
}
