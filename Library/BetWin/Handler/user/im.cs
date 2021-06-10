using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using BW.Common.Users;
using BW.Common.IM;
using BW.Framework;
using SP.Studio.Model;
using SP.Studio.Core;

using SP.Studio.Web;
using BW.Agent;
using BW.Common.Admins;

using BW.GateWay.IM;

namespace BW.Handler.user
{
    /// <summary>
    /// 即时通讯工具
    /// </summary>
    public class im : IHandler
    {
        /// <summary>
        /// 获得我的信息、好友列表、群组列表
        /// </summary>
        /// <param name="context"></param>
        private void init(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            StringBuilder sb = new StringBuilder();

            string data = null;
            if (this.UserInfo == null)
            {
                data = this.init_guest();
            }
            else
            {
                data = this.init_user();
            }
            sb.Append("{")
             .Append("\"code\": 0,")
             .AppendFormat("\"msg\": \"{0}\",", this.StopwatchMessage(context))
             .AppendFormat("\"data\":{0}", data)
             .Append("}");

            context.Response.Write(sb);
        }

        /// <summary>
        /// 游客初始化
        /// </summary>
        private string init_guest()
        {
            return new
            {
                mine = new JsonString(new IMUser(UserInfo).ToString("type", UserAgent.IM_USER, "rebot", SiteInfo.Rebot.IsOpen ? 1 : 0)),
                friend = new JsonString(string.Format("[{0}]", new
                {
                    groupname = "在线客服",
                    id = 1,
                    list = new JsonString(string.Format("[{0}]", new IMUser(ChatLog.UserType.Admin).ToString()))
                }.ToJson()))
            }.ToJson();
        }

        /// <summary>
        /// 用户初始化
        /// </summary>
        private string init_user()
        {
            User agent = UserAgent.Instance().GetUserInfo(UserInfo.AgentID);
            List<User> child = UserAgent.Instance().GetChildList(UserInfo.ID);
            List<Admin> service = AdminAgent.Instance().GetServiceAdminList();

            List<string> list = new List<string>();
            list.Add(new
                {
                    groupname = "在线客服",
                    id = 1,
                    list = new JsonString(string.Format("[{0}]", string.Join(",", service.Select(t => new IMUser(t).ToString(UserInfo.IMID)))))
                }.ToJson());

            if (agent != null)
            {
                list.Add(new
                {
                    groupname = "上级",
                    id = 2,
                    list = new JsonString(string.Format("[{0}]", new IMUser(agent, false, "我的上级").ToString(UserInfo.IMID)))
                }.ToJson());
            }

            list.Add(new
                {
                    groupname = "下级",
                    id = 3,
                    list = new JsonString(string.Format("[{0}]", string.Join(",", child.Select(t => new IMUser(t, false).ToString(UserInfo.IMID)))))
                }.ToJson());

            return new
            {
                mine = new JsonString(new IMUser(UserInfo).ToString("type", UserAgent.IM_USER, "rebot", SiteInfo.Rebot.IsOpen ? 1 : 0)),
                friend = new JsonString(string.Format("[{0}]", string.Join(",", list)))
            }.ToJson();
        }

        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="context"></param>
        private void info(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Mine = new JsonString(new IMUser(UserInfo).ToString()),
                Service = new JsonString(new IMUser(ChatLog.UserType.Admin).ToString())
            });
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="context"></param>
        private void uploadimage(HttpContext context)
        {
            //            {
            //  "code": 0 //0表示成功，其它表示失败
            //  ,"msg": "" //失败信息
            //  ,"data": {
            //    "src": "http://cdn.xxx.com/upload/images/a.jpg" //图片url
            //  }
            //}      
            string msg = string.Empty;
            if (UserInfo == null)
            {
                this.layimerror(context, 1, "无此权限", null);
            }
            HttpPostedFile file = context.Request.Files["file"];
            if (file.ContentLength == 0 || file.ContentLength > 1024 * 1024)
            {
                this.layimerror(context, 1, "图片最大尺寸不能超过1M", null);
            }

            string result = UserAgent.Instance().UploadImage(file, "upload");
            if (string.IsNullOrEmpty(result))
            {
                this.layimerror(context, 1, UserAgent.Instance().Message());
            }

            this.layimerror(context, 0, "", SysSetting.GetSetting().imgServer + result);
        }


        /// <summary>
        /// 获取好友的在线状态
        /// </summary>
        /// <param name="context"></param>
        private void online(HttpContext context)
        {
            this.CheckUserLogin(context);
            List<string> list = BDC.User.Where(t => t.SiteID == SiteInfo.ID && (t.ID == UserInfo.AgentID || t.AgentID == UserInfo.ID)).Select(t => new { t.ID, t.IsOnline }).ToArray().Select(t =>
                string.Concat("\"", t.ID, "\":", t.IsOnline ? 1 : 0)).ToList();

            list.AddRange(AdminAgent.Instance().GetServiceList().Select(t => string.Concat("\"staff", t.ID, "\":", t.IsOnline ? 1 : 0, "")));

            context.Response.Write(true, "会员在线状态", string.Concat("{", string.Join(",", list), "}"));
        }

        /// <summary>
        /// 歷史聊天記錄
        /// </summary>
        /// <param name="context"></param>
        private void loglist(HttpContext context)
        {
            string id = QF("ID");

            var list = BDC.ChatLog.Where(t => t.SiteID == SiteInfo.ID && t.Key == id).OrderByDescending(t => t.ID).Take(10).OrderBy(t => t.ID).ToList();

            string user = string.Concat(UserAgent.IM_USER, "-", UserInfo.ID);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                Mine = t.SendID == user ? 1 : 0,
                Face = t.SendAvatar,
                t.SendID,
                t.CreateAt,
                Name = t.SendName,
                t.Content,
                IsRead = t.IsRead ? 1 : 0
            }));
        }

        /// <summary>
        /// 获取未读的聊天记录
        /// </summary>
        /// <param name="context"></param>
        private void list(HttpContext context)
        {
            string userId = string.Concat(UserAgent.IM_USER, "-", UserInfo.ID);
            List<ChatLog> list = BDC.ChatLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && !t.IsRead).OrderBy(t => t.CreateAt).ToList();

            UserAgent.Instance().UpdateChatLogRead(userId, list.Select(t => t.ID).ToArray());

            context.Response.Write(true, this.StopwatchMessage(context), string.Format("[{0}]", string.Join(",", list.Select(t => t.ToString()))));

        }

        /// <summary>
        /// 接收信息
        /// </summary>
        /// <param name="context"></param>
        private void send(HttpContext context)
        {
            string action = QF("action");
            if (action != "Receive") return;

            string userId = string.Concat(UserAgent.IM_USER, "-", UserInfo.ID);
            ChatLog chatlog = new ChatLog()
            {
                SiteID = SiteInfo.ID,
                Key = QF("to"),
                Content = QF("content"),
                SendID = userId,
                SendName = QF("name"),
                SendAvatar = QF("avatar"),
                IsRead = QF("rebot", 0) == 1
            };

            chatlog.ID = UserAgent.Instance().SaveChatLog(chatlog);
            if (chatlog.ID == 0)
            {
                context.Response.Write(false, UserAgent.Instance().Message());
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                ID = chatlog.ID
            });
        }


        /// <summary>
        /// 群搜索
        /// </summary>
        /// <param name="context"></param>
        private void groupsearch(HttpContext context)
        {

        }
    }
}
