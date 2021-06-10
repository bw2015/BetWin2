using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using BW.Agent;
using BW.Common.Admins;
using BW.Common.Users;
using BW.Common.Sites;
using BW.Common.Lottery;

using BW.Common.IM;
using SP.Studio.Model;
using BW.Framework;
using SP.Studio.Core;
using SP.Studio.Data;

namespace BW.Handler.admin
{
    /// <summary>
    /// 客服工作平台
    /// </summary>
    public class im : IHandler
    {
        /// <summary>
        /// 检查是否拥有客服的权限
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void check(HttpContext context)
        {
            context.Response.Write(true, "检查成功", new
            {
                ID = "staff" + AdminInfo.ID,
                AdminInfo.SiteID,
                AdminInfo.Name
            });
        }

        /// <summary>
        /// 客服初始化数据
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void init(HttpContext context)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("\"code\": 0,")
                .Append("\"msg\": \"\",")
                .Append("\"data\": {    \"mine\": ")
                .Append(new IMUser(AdminInfo).ToString("type", UserAgent.IM_ADMIN))
                .Append(",\"group\":[");

            List<string> wechat = new List<string>();
            foreach (ChatTalk.GroupType type in Enum.GetValues(typeof(ChatTalk.GroupType)))
            {
                if (Enum.IsDefined(typeof(LotteryType), (byte)type))
                {
                    LotteryType lottery = type.ToEnum<LotteryType>();
                    wechat.Add(string.Format("{{\"groupname\":\"{0}\",\"id\": \"{1}\",\"avatar\": \"images/im-group-{2}.png\" }}",
                       LotteryAgent.Instance().GetLotteryName(lottery),
                       UserAgent.Instance().GetTalkKey(AdminInfo.IMID, type),
                       type.ToString().ToLower()));
                }
            }
            sb.Append(string.Join(",", wechat));
            sb.Append(" ]  }   }");

            context.Response.Write(sb);
        }

        /// <summary>
        /// 查看群组成员
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void groupmember(HttpContext context)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{")
                .Append("\"code\":0,\"msg\":\"\",\"data\":{");

            string id = QF("id");
            switch (id)
            {
                case "group0":
                    // 管理员内部沟通群
                    List<Admin> list = AdminAgent.Instance().GetAdminList().FindAll(t => t.Status == Admin.AdminStatus.Normal);
                    Admin admin = list.Find(t => t.GroupID == 0);
                    sb.Append("\"list\":[");
                    sb.Append(string.Join(",",
                        list.Where(t => t.IsOnline).OrderBy(t => t.ID).Select(t =>
                        {
                            return string.Concat("{",
                                string.Format("\"username\": \"{0}\",\"id\":\"staff{1}\",\"avatar\":\"{2}\",\"sign\":\"{3}\"", t.Name, t.ID, t.FaceShow, t.GroupInfo.Name),
                                "}");
                        })));
                    sb.Append("] ");
                    break;
            }
            sb.Append("} }");
            context.Response.Write(sb);
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="context"></param>
        [Admin]
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
        /// 聊天会话记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.聊天记录.Value)]
        private void sessionlog(HttpContext context)
        {
            IQueryable<ChatTalk> list = BDC.ChatTalk.Where(t => t.SiteID == SiteInfo.ID && t.Count > 0);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.LastAt), t =>
            {
                int user1 = UserAgent.Instance().GetChatUserID(t.User1);
                int user2 = UserAgent.Instance().GetChatUserID(t.User2);

                return new
                {
                    User1 = user1,
                    UserName1 = UserAgent.Instance().GetUserName(user1),
                    User2 = user2,
                    UserName2 = UserAgent.Instance().GetUserName(user2),
                    t.LastAt,
                    Type = t.Type.GetDescription(),
                    t.Count,
                    t.Key
                };
            }));
        }

        /// <summary>
        /// 聊天记录详情
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.聊天记录.Value)]
        private void loglist(HttpContext context)
        {
            string key = QF("Key");
            ChatTalk talk = BDC.ChatTalk.Where(t => t.SiteID == SiteInfo.ID && t.Key == key).FirstOrDefault();
            IQueryable<ChatLog> list = BDC.ChatLog.Where(t => t.SiteID == SiteInfo.ID && t.Key == key);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderBy(t => t.ID), t =>
            {
                return new
                {
                    Mine = t.UserID == talk.User1 ? 1 : 0,
                    Face = t.SendAvatar,
                    t.SendID,
                    t.CreateAt,
                    Name = t.SendName,
                    t.Content,
                    IsRead = t.IsRead ? 1 : 0
                };
            }));
        }

        /// <summary>
        /// 机器人信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.系统设置.Value)]
        private void rebotinfo(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                SiteInfo.Rebot.IsOpen,
                SiteInfo.Rebot.Name,
                SiteInfo.Rebot.FaceShow,
                SiteInfo.Rebot.Face,
                SiteInfo.Rebot.Sign
            });
        }

        /// <summary>
        /// 保存机器人信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.系统设置.Value)]
        private void saverebot(HttpContext context)
        {
            string name = QF("Name");
            if (string.IsNullOrEmpty(name))
            {
                context.Response.Write(false, "请输入机器人名字");
            }
            SiteInfo.Rebot.IsOpen = QF("IsOpen", 0) == 1;
            SiteInfo.Rebot.Name = name;
            SiteInfo.Rebot.Sign = QF("Sign");

            this.ShowResult(context, SiteAgent.Instance().SaveRebotInfo(SiteInfo.Rebot, context.Request.Files["face"]), "保存成功");
        }


        /// <summary>
        /// 返回与会员之间的对话key
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.在线客服)]
        private void gettalkkey(HttpContext context)
        {
            int userId = QF("ID", 0);
            if (userId == 0)
            {
                context.Response.Write(false, "用户ID错误");
            }
            User user = UserAgent.Instance().GetUserInfo(userId);
            if (user == null)
            {
                context.Response.Write(false, "用户ID错误");
            }
            string key = UserAgent.Instance().GetTalkKey(AdminInfo.IMID, user.IMID);
            if (string.IsNullOrEmpty(key))
            {
                context.Response.Write(false, "生成对话错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Name = user.Name,
                FaceShow = user.FaceShow,
                Key = key
            });
        }

        #region ===========  常用语设置  =============

        /// <summary>
        /// 常用语分类
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.常用语设置.Value)]
        private void replycategory(HttpContext context)
        {
            IQueryable<ReplyCategory> list = BDC.ReplyCategory.Where(t => t.SiteID == SiteInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.Name
            }));
        }

        /// <summary>
        /// 修改常用语分类名称
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.常用语设置.Value)]
        private void replycategoryupdate(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().UpdateReplyCategory(QF("ID", 0), QFS("Value")));
        }

        /// <summary>
        /// 删除常用语分类
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.常用语设置.Value)]
        private void replycategorydelete(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteReplyCategory(QF("ID", 0)));
        }


        /// <summary>
        /// 新增常用语分类
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.常用语设置.Value)]
        private void replycategoryadd(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().AddReplyCategory(QFS("Name")));
        }

        /// <summary>
        /// 常用语列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.常用语设置.Value)]
        private void replylist(HttpContext context)
        {
            IQueryable<Reply> list = BDC.Reply.Where(t => t.SiteID == SiteInfo.ID);
            Dictionary<int, string> category = BDC.ReplyCategory.Where(t => t.SiteID == SiteInfo.ID).ToDictionary(t => t.ID, t => t.Name);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Category = category[t.CateID],
                t.Content,
                t.CreateAt
            }));
        }

        /// <summary>
        /// 客服端获取常用语列表
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void getreplylist(HttpContext context)
        {
            List<Reply> list = BDC.Reply.Where(t => t.SiteID == SiteInfo.ID).ToList();
            Dictionary<int, string> category = BDC.ReplyCategory.Where(t => t.SiteID == SiteInfo.ID).ToDictionary(t => t.ID, t => t.Name);
            List<string> json = new List<string>();
            foreach (KeyValuePair<int, string> cate in category)
            {
                json.Add(string.Format("\"{0}\":[{1}]", cate.Value, string.Join(",",
                    list.Where(t => t.CateID == cate.Key).Select(t => string.Format("\"{0}\"", HttpUtility.JavaScriptStringEncode(t.Content))))));
            }
            context.Response.Write(true, this.StopwatchMessage(context), string.Concat("{", string.Join(",", json), "}"));
        }

        /// <summary>
        /// 保存常用语
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.常用语设置.Value)]
        private void replysave(HttpContext context)
        {
            Reply reply = SiteAgent.Instance().GetReplyInfo(QF("ID", 0)) ?? new Reply();
            reply = context.Request.Form.Fill(reply);

            this.ShowResult(context, SiteAgent.Instance().SaveReplyInfo(reply), "常用语保存成功");
        }


        /// <summary>
        /// 获取常用语信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.常用语设置.Value)]
        private void replyinfo(HttpContext context)
        {
            Reply reply = SiteAgent.Instance().GetReplyInfo(QF("ID", 0)) ?? new Reply();

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                reply.ID,
                reply.CateID,
                reply.Content
            });
        }

        /// <summary>
        /// 删除常用语
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.常用语设置.Value)]
        private void replydelete(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteReply(QF("ID", 0)), "常用语删除成功");
        }

        #endregion

        #region =========  关键词设置  ===============

        /// <summary>
        /// 保存关键词设置
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.关键词设置.Value)]
        private void savekeyword(HttpContext context)
        {
            ReplyKeyword keyword = SiteAgent.Instance().GetReplyKeywordInfo(QF("ID", 0)) ?? new ReplyKeyword();
            keyword = context.Request.Form.Fill(keyword);
            this.ShowResult(context, SiteAgent.Instance().SaveReplyKeywordInfo(keyword), "保存成功");
        }


        /// <summary>
        /// 查看关键词信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.关键词设置.Value)]
        private void getkeywordinfo(HttpContext context)
        {
            ReplyKeyword keyword = SiteAgent.Instance().GetReplyKeywordInfo(QF("ID", 0)) ?? new ReplyKeyword();
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                keyword.ID,
                keyword.Keyword,
                keyword.Content
            });
        }

        /// <summary>
        /// 关键词列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.关键词设置.Value)]
        private void keywordlist(HttpContext context)
        {
            IQueryable<ReplyKeyword> list = BDC.ReplyKeyword.Where(t => t.SiteID == SiteInfo.ID);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                t.Keyword,
                t.Content
            }));
        }

        /// <summary>
        /// 删除关键词
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.关键词设置.Value)]
        private void deletekeyword(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteKeyword(QF("ID", 0)), "删除成功");
        }

        #endregion

        #region ============== 聊天工具 ==============

        /// <summary>
        /// 管理员读取未读信息列表
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void list(HttpContext context)
        {
            bool isService = AdminInfo.HasPermission(AdminPermission.客服管理.在线客服);
            string adminId = string.Concat(UserAgent.IM_ADMIN, "-", AdminInfo.ID);
            string serviceId = string.Concat(UserAgent.IM_ADMIN, "-", 0);
            var list = BDC.ChatLog.Where(t => t.SiteID == SiteInfo.ID && !t.IsRead);
            if (isService)
            {
                list = list.Where(t => (t.UserID == adminId || t.UserID == serviceId));
            }
            else
            {
                list = list.Where(t => t.UserID == adminId);
            }

            List<ChatLog> chatlog = list.OrderBy(t => t.CreateAt).ToList();
            UserAgent.Instance().UpdateChatLogRead(chatlog.Select(t => t.ID).ToArray());
            context.Response.Write(true, this.StopwatchMessage(context), string.Format("[{0}]", string.Join(",", chatlog.Select(t => t.ToString()))));
        }

        /// <summary>
        /// 管理员发送信息
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void send(HttpContext context)
        {
            string adminId = string.Concat(UserAgent.IM_ADMIN, "-", AdminInfo.ID);
            ChatLog chatlog = new ChatLog()
            {
                SiteID = SiteInfo.ID,
                Key = QF("to"),
                Content = QF("content"),
                SendID = adminId,
                SendName = QF("name"),
                SendAvatar = QF("avatar")
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

        #endregion
    }
}
