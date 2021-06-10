using BW.Agent;
using BW.Common.Users;
using SP.Studio.Controls;
using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace BW.Handler.user
{
    public class account : IHandler
    {
        /// <summary>
        /// 从注册链接中注册
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void invite(HttpContext context)
        {
            string key = QF("invite");
            if (string.IsNullOrEmpty(key)) key = SiteInfo.Setting.RegisterInvite;

            UserInvite userInvite = UserAgent.Instance().GetUserInviteInfo(key);
            if (userInvite == null)
            {
                context.Response.Write(false, "邀请码错误");
            }

            User user = context.Request.Form.Fill<User>();
            this.ShowResult(context, UserAgent.Instance().AddUser(user, key), "注册成功");
        }

        /// <summary>
        /// 上级创建账户
        /// </summary>
        /// <param name="context"></param>
        private void create(HttpContext context)
        {
            if (UserInfo.Type != User.UserType.Agent)
            {
                context.Response.Write(false, "您没有推广资格");
            }
            User user = context.Request.Form.Fill<User>();
            user.AgentID = this.UserInfo.ID;
            if (string.IsNullOrEmpty(user.Password))
            {
                user.Password = SiteInfo.Setting.DefaultPassword;
            }

            this.ShowResult(context, UserAgent.Instance().AddUser(user), "创建成功");
        }

        /// <summary>
        /// 上级创建账户的默认信息
        /// </summary>
        /// <param name="context"></param>
        private void createinfo(HttpContext context)
        {
            if (string.IsNullOrEmpty(SiteInfo.Setting.DefaultPassword))
            {
                context.Response.Write(false, "未设置默认密码");
            }
            if (UserInfo.Type != User.UserType.Agent)
            {
                context.Response.Write(false, "您不是代理");
            }
            List<int> list = new List<int>();
            for (int i = SiteInfo.Setting.MinRebate; i <= (SiteInfo.Setting.IsSameRebate ? UserInfo.Rebate : Math.Max(SiteInfo.Setting.MinRebate, UserInfo.Rebate - 2)); i += 2)
            {
                list.Add(i);
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Password = SiteInfo.Setting.DefaultPassword,
                Rebate = new JsonString(string.Format("[{0}]", string.Join(",", list)))
            });
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void login(HttpContext context)
        {
            string username = QF("UserName");
            string password = QF("Password");
            string vcode = QF("Code");

            if (context.Session["login"] != null && (string)context.Session["login"] != vcode)
            {
                context.Response.Write(false, "验证码输入错误");
            }

            Guid session;
            bool success = UserAgent.Instance().Login(username, password, out session);
            if (success)
            {
                string device_model = QF("Device.Model");
                string device_uuid = QF("Device.UUID");
                string device_phone = QF("Device.Phone");
                if (!string.IsNullOrEmpty(device_model) && !string.IsNullOrEmpty(device_uuid))
                {
                    int userId = UserAgent.Instance().GetUserID(session);
                    UserAgent.Instance().SaveUserDevice(userId, device_model, device_uuid, device_phone);
                }
            }
            this.ShowResult(context, success, "登录成功", new
            {
                Session = session.ToString("N")
            });
        }

        /// <summary>
        /// 用于API的登录
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void apilogin(HttpContext context)
        {
            Guid session = QF("Session", Guid.Empty);
            string url = QF("Url");
            if (string.IsNullOrEmpty(url)) url = "/game.html";
            if (UserAgent.Instance().Login(session))
            {
                context.Response.ContentType = "text/html";
                context.Response.Write("<script> location.href='" + url + "'; </script>");
            }
        }

        /// <summary>
        /// 对账户资金进行转入/转出操作
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void transfer(HttpContext context)
        {
            string userName = QF("UserName");
            decimal money = QF("Money", decimal.Zero);
            string key = QF("Key");
            int orderID = QF("OrderID", 0);

            if (money == decimal.Zero)
            {
                context.Response.Write(false, "金额错误");
            }
            if (key != SiteInfo.Setting.APIKey)
            {
                context.Response.Write(false, "密钥错误");
            }
            int userId = UserAgent.Instance().GetUserID(userName);
            if (userId == 0)
            {
                context.Response.Write(false, "用户名不存在");
            }

            bool success = false;
            if (money > decimal.Zero)
            {
                success = UserAgent.Instance().AddMoneyLog(userId, money, MoneyLog.MoneyType.TransferIn, orderID, string.Format("转入{0}元", money.ToString("n")));
            }
            else
            {
                success = UserAgent.Instance().AddMoneyLog(userId, money, MoneyLog.MoneyType.Transfer, orderID, string.Format("转出{0}元", Math.Abs(money).ToString("n")));
            }

            this.ShowResult(context, success, "转账成功");
        }

        /// <summary>
        /// 修改会员密码
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void apipassword(HttpContext context)
        {
            string userName = QF("UserName");
            string password = QF("Password");
            string key = QF("Key");

            if (key != SiteInfo.Setting.APIKey)
            {
                context.Response.Write(false, "密钥错误");
            }

            this.ShowResult(context, UserAgent.Instance().UpdatePassword(userName, password), "密码修改成功");

        }

        /// <summary>
        /// 使用session key自动登录
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void fastlogin(HttpContext context)
        {
            Guid session = QF("Session", Guid.Empty);
            if (session == Guid.Empty)
            {
                context.Response.Write(false, "没有登录");
            }

            this.ShowResult(context, UserAgent.Instance().Login(session), "自动登录成功");
        }

        /// <summary>
        /// 使用谷歌验证码登录
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void codelogin(HttpContext context)
        {
            string userName = QF("UserName");
            int code = QF("Code", 0);

            Guid session;
            this.ShowResult(context, UserAgent.Instance().Login(userName, code, out session), "登录成功", new
            {
                Session = session.ToString("N")
            });
        }

        /// <summary>
        /// 微信登录（如果没有注册的话，使用携带的邀请码自动注册）
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void wechatlogin(HttpContext context)
        {
            string openId = WebAgent.GetParam("openid");
            string invite = WebAgent.GetParam("invite");
            string type = WebAgent.GetParam("type");
            bool result = UserAgent.Instance().LoginByWX(openId, invite);

            switch (type)
            {
                case "wx":
                    context.Response.Redirect("/wechat/start.html");
                    break;
                default:
                    this.ShowResult(context, result, "登录成功");
                    break;
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void logout(HttpContext context)
        {
            UserAgent.Instance().Logout();
            context.Response.Write(true, "退出登录");
        }

        /// <summary>
        /// 获取用户的密码提示问题
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void getquestion(HttpContext context)
        {
            string username = QF("UserName");
            int userId = string.IsNullOrEmpty(username) ? (this.UserInfo == null ? 0 : this.UserInfo.ID) : UserAgent.Instance().GetUserID(username);
            if (userId == 0)
            {
                context.Response.Write(false, "用户名不存在");
            }

            User user = UserAgent.Instance().GetUserInfo(userId);
            if (user.Question == User.QuestionType.None)
            {
                context.Response.Write(false, "未设置安全问题");
            }

            context.Response.Write(true, user.Question.GetDescription(), user.ToJson(t => t.Question));
        }

        /// <summary>
        /// 获取当前用户的安全问题设置状态
        /// </summary>
        /// <param name="context"></param>
        private void question(HttpContext context)
        {
            this.CheckUserLogin(context);
            context.Response.Write(true, this.StopwatchMessage(context), typeof(User.QuestionType).ToList().Where(t =>
            {
                return UserInfo.Question == User.QuestionType.None || UserInfo.Question == (User.QuestionType)t.ID;
            }).ToDictionary(t => t.Name, t => t.Description).ToJson());

        }

        /// <summary>
        /// 获取用户的资料信息
        /// </summary>
        /// <param name="context"></param>
        private void getinfo(HttpContext context)
        {
            this.CheckUserLogin(context);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                UserInfo.NickName,
                QQ = WebAgent.HiddenQQ(UserInfo.QQ),
                Email = WebAgent.HiddenEmail(UserInfo.Email),
                Mobile = WebAgent.HiddenMobile(UserInfo.Mobile)
            }.ToJson());
        }

        /// <summary>
        /// 保存信息（QQ\Email、手机号码第一次设定之后将不允许修改）
        /// </summary>
        /// <param name="context"></param>
        private void saveinfo(HttpContext context)
        {
            this.CheckUserLogin(context);
            if (!string.IsNullOrEmpty(QF("Mobile")) && !string.IsNullOrEmpty(UserInfo.Mobile))
            {
                context.Response.Write(false, "手机号码不能修改");
            }
            if (!string.IsNullOrEmpty(QF("QQ")) && !string.IsNullOrEmpty(UserInfo.QQ))
            {
                context.Response.Write(false, "QQ号码不能修改");
            }
            if (!string.IsNullOrEmpty(QF("Email")) && !string.IsNullOrEmpty(UserInfo.Email))
            {
                context.Response.Write(false, "电子邮箱不能修改");
            }
            if (!string.IsNullOrEmpty(QF("Mobile")) && !WebAgent.IsMobile(QF("Mobile")))
            {
                context.Response.Write(false, "手机号码格式错误");
            }
            if (!string.IsNullOrEmpty(QF("QQ")) && !WebAgent.IsQQ(QF("QQ")))
            {
                context.Response.Write(false, "QQ号码格式错误");
            }
            if (!string.IsNullOrEmpty(QF("Email")) && !WebAgent.IsEmail(QF("Email")))
            {
                context.Response.Write(false, "电子邮件输入错误");
            }
            if (QFS("NickName").Length > 20)
            {
                context.Response.Write(false, "昵称超过允许长度");
            }

            List<Expression<Func<User, object>>> fields = new List<Expression<Func<User, object>>>();

            UserInfo.NickName = QFS("NickName");
            fields.Add(t => t.NickName);
            if (WebAgent.IsEmail(QF("Email")))
            {
                UserInfo.Email = QF("Email");
                fields.Add(t => t.Email);
            }
            if (WebAgent.IsQQ(QF("QQ")))
            {
                UserInfo.QQ = QF("QQ");
                fields.Add(t => t.QQ);
            }
            if (WebAgent.IsMobile(QF("Mobile")))
            {
                UserInfo.Mobile = QF("Mobile");
                fields.Add(t => t.Mobile);
            }

            bool result = UserAgent.Instance().UpdateUserInfo(UserInfo, fields.ToArray());
            context.Response.Write(result, result ? "保存成功" : UserAgent.Instance().Message());
        }

        /// <summary>
        /// 找回密码的条件
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void findpasswordquery(HttpContext context)
        {
            string userName = QF("UserName");
            int userId = UserAgent.Instance().GetUserID(userName);
            if (userId == 0)
            {
                context.Response.Write(false, "用户名不存在");
            }

            User user = UserAgent.Instance().GetUserInfo(userId);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                UserName = userName,
                PayPassword = new JsonString(!string.IsNullOrEmpty(user.PayPassword) ? 1 : 0),
                Question = string.IsNullOrEmpty(user.Answer) ? "" : user.Question.GetDescription(),
                Mobile = new JsonString(!string.IsNullOrEmpty(user.Mobile) ? 1 : 0),
                AccountName = new JsonString(!string.IsNullOrEmpty(user.AccountName) ? 1 : 0)
            });
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void forget(HttpContext context)
        {
            string userName = QF("UserName");
            int userId = UserAgent.Instance().GetUserID(userName);
            if (userId == 0)
            {
                context.Response.Write(false, "用户名不存在");
            }
            this.ShowResult(context, UserAgent.Instance().ResetUserPassword(userId, QF("PayPassword"), QF("Answer"), QF("AccountName")), "登录密码重置成为默认密码：" + SiteInfo.Setting.DefaultPassword);

        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="context"></param>
        private void password(HttpContext context)
        {
            this.CheckUserLogin(context);
            if (QF("NewPassword") != QF("NewPassword2"))
            {
                context.Response.Write(false, "两次输入的密码不相同");
            }
            bool result = UserAgent.Instance().UpdatePassword(UserInfo.ID, QF("Password"), QF("NewPassword"));
            context.Response.Write(result, result ? "修改成功" : UserAgent.Instance().Message());
        }

        /// <summary>
        /// 修改资金密码
        /// </summary>
        /// <param name="context"></param>
        private void paypassword(HttpContext context)
        {
            this.CheckUserLogin(context);
            if (string.IsNullOrEmpty(QF("PayPassword")))
            {
                context.Response.Write(false, "请输入当前密码");
            }
            if (QF("NewPayPassword") != QF("NewPayPassword2"))
            {
                context.Response.Write(false, "两次输入的密码不相同");
            }
            bool result = UserAgent.Instance().UpdatePayPassword(UserInfo.ID, QF("PayPassword"), QF("NewPayPassword"));
            context.Response.Write(result, result ? "修改成功" : UserAgent.Instance().Message());
        }

        /// <summary>
        /// 保存安全问题
        /// </summary>
        /// <param name="context"></param>
        private void saveanswer(HttpContext context)
        {
            this.CheckUserLogin(context);
            if (QF("Answer") != QF("Answer2"))
            {
                context.Response.Write(false, "两次输入的答案不相同");
            }

            bool result = UserAgent.Instance().SaveSafeAnswer(UserInfo.ID, QF("Question").ToEnum<User.QuestionType>(), QF("Answer"));

            context.Response.Write(result, result ? "设定成功" : UserAgent.Instance().Message());
        }

        /// <summary>
        /// 保存用户的设备信息
        /// </summary>
        /// <param name="context"></param>
        private void savehost(HttpContext context)
        {
            string udid = QF("udid");
            string platform = QF("Platform");
            Guid host = Guid.Empty;
            if (!string.IsNullOrEmpty(udid))
            {
                host = WebAgent.GetGuid(udid);
            }

            this.ShowResult(context, UserAgent.Instance().SaveUserHost(UserInfo.ID, host, platform), "保存成功");
        }

        /// <summary>
        /// 删除保存的用户设备信息
        /// </summary>
        /// <param name="context"></param>
        private void deletehost(HttpContext context)
        {
            string platform = QF("Platform");
            this.ShowResult(context, UserAgent.Instance().DeleteUserHost(UserInfo.ID, platform));
        }

        /// <summary>
        /// 根据设备信息获取用户的session值
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void gethostsession(HttpContext context)
        {
            string platform = QF("Platform");
            string uuid = QF("uuid");
            Guid host = QF("host", Guid.Empty);
            if (!string.IsNullOrEmpty(uuid))
            {
                host = WebAgent.GetGuid(uuid);
            }
            int userId;
            Guid session = UserAgent.Instance().GetUserSession(host, platform, out userId);
            if (session == Guid.Empty)
            {
                context.Response.Write(false, "没有找到绑定信息");
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Session = session,
                UserName = UserAgent.Instance().GetUserName(userId)
            });
        }


        /// <summary>
        /// 通过验证资料重置登录密码
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void checkuserinfoandresetpassword(HttpContext context)
        {
            if (!UserAgent.Instance().CheckUser(QF("UserName"), QF("Mobile"), QF("AccountName"), QF("Account")))
            {
                context.Response.Write(false, UserAgent.Instance().Message());
            }

            int userId = UserAgent.Instance().GetUserID(QF("UserName"));
            User user = UserAgent.Instance().GetUserInfo(userId);
            this.ShowResult(context, UserAgent.Instance().ResetUserInfo(user, t => t.Password), "密码已重置成为" + SiteInfo.Setting.DefaultPassword + "，请登录后及时修改密码");
        }

        /// <summary>
        /// 重置资金密码
        /// </summary>
        /// <param name="context"></param>
        private void checkuserinfoandresetpaypassword(HttpContext context)
        {
            if (!UserAgent.Instance().CheckUser(UserInfo.UserName, QF("Mobile"), QF("AccountName"), QF("Account")))
            {
                context.Response.Write(false, UserAgent.Instance().Message());
            }

            this.ShowResult(context, UserAgent.Instance().ResetUserInfo(UserInfo, t => t.PayPassword), "资金密码已经重置，请重新设置您的资金密码");
        }

        #region ============  设备信息  =============

        /// <summary>
        /// 保存设备信息
        /// </summary>
        /// <param name="conetxt"></param>
        private void savedevice(HttpContext context)
        {
            string key = QF("Key");
            if (string.IsNullOrEmpty(key))
            {
                context.Response.Write(false, "没有获取到图案");
            }
            this.ShowResult(context, UserAgent.Instance().SaveUserDevice(QF("Model"), QF("UUID"), key), "保存成功");
        }

        /// <summary>
        /// 清除用户的手势
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void removedevicekey(HttpContext context)
        {
            string uuid = QF("uuid");
            int userId = QF("UserID", UserInfo == null ? 0 : UserInfo.ID);

            this.ShowResult(context, UserAgent.Instance().RemoveUserDeviceKey(userId, uuid));
        }

        /// <summary>
        /// 保存用户的登录设备
        /// </summary>
        /// <param name="context"></param>
        private void saveuserdevice(HttpContext context)
        {
            string model = QF("Model");
            string uuid = QF("uuid");
            this.ShowResult(context, UserAgent.Instance().SaveUserDevice(model, uuid));
        }

        /// <summary>
        /// 获取当前用户的设备列表
        /// </summary>
        /// <param name="context"></param>
        private void devicelist(HttpContext context)
        {
            List<UserDevice> list = UserAgent.Instance().GetUserDeviceList(UserInfo.ID);


            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Model,
                t.UpdateAt,
                t.UUID
            }));
        }

        /// <summary>
        /// 获取在这个设备上登录的用户列表
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void deviceuserlist(HttpContext context)
        {
            string uuid = QF("UUID");
            if (string.IsNullOrEmpty(uuid))
            {
                context.Response.Write(false, "未获取到设备编号");
            }
            List<User> list = UserAgent.Instance().GetUserDeviceList(uuid);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.UserName,
                t.FaceShow
            }));
        }

        /// <summary>
        /// 设备登录
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void devicelogin(HttpContext context)
        {
            string uuid = QF("UUID");
            int userId = QF("UserID", 0);
            string key = QF("Key");
            Guid sessionKey;
            bool result = UserAgent.Instance().Login(userId, uuid, key, out sessionKey);
            this.ShowResult(context, result, "登录成功", new
            {
                Session = sessionKey.ToString("N")
            });
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="context"></param>
        private void deletedevice(HttpContext context)
        {
            Guid uuid = QF("UUID", Guid.Empty);

            this.ShowResult(context, UserAgent.Instance().DeleteDevice(UserInfo.ID, uuid));
        }




        #endregion
    }
}
