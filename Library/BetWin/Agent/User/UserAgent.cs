using BW.Common.Admins;
using BW.Common.Sites;
using BW.Common.Users;
using BW.Framework;
using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.PageBase;
using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using BankType = BW.Common.Sites.BankType;

namespace BW.Agent
{
    /// <summary>
    /// 用户操作代理类（涉及所有usr开头的表）
    /// </summary>
    public sealed partial class UserAgent : AgentBase<UserAgent>
    {
        #region ==============  登录/注册  ===================

        /// <summary>
        /// 添加一个新用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="inviteKey">邀请码</param>
        /// <returns></returns>
        public bool AddUser(User user, string inviteKey = null)
        {
            UserInvite invite = null;
            if (!string.IsNullOrEmpty(inviteKey))
            {
                invite = this.GetUserInviteInfo(inviteKey);
                if (invite == null)
                {
                    base.Message("邀请码错误");
                    return false;
                }
                user.Type = invite.Type;
                user.AgentID = invite.UserID;
                user.Rebate = invite.Rebate;
            }
            if (!this.CheckUserRegister(user))
            {
                return false;
            }

            user.SiteID = SiteInfo.ID;
            user.CreateAt = DateTime.Now;
            user.Money = user.LockMoney = user.Wallet = decimal.Zero;
            user.RegIP = IPAgent.IP;
            user.Password = MD5.toMD5(user.Password);
            User agentUser = this.GetUserInfo(user.AgentID);
            List<int> parent = new List<int>();
            if (user.AgentID != 0)
            {
                parent = this.GetUserParentList(user.AgentID);
                user.UserLevel = parent.Count + 1;
            }

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (agentUser != null)
                    {
                        user.Lock = agentUser.Lock;
                    }

                    if (!user.Add(true, db))
                    {
                        db.Rollback();
                        return false;
                    }
                    if (user.AgentID != 0)
                    {
                        parent.Insert(0, user.AgentID);
                        for (int depth = 0; depth < parent.Count; depth++)
                        {
                            new UserDepth()
                            {
                                SiteID = SiteInfo.ID,
                                UserID = parent[depth],
                                ChildID = user.ID,
                                Depth = depth + 1
                            }.Add(db);
                        }
                    }
                    if (invite != null)
                    {
                        invite.Member++;
                        invite.Update(db, t => t.Member);
                    }
                }
                catch (Exception ex)
                {
                    base.Message(ex.Message);
                    db.Rollback();
                    return false;
                }

                db.Commit();
                return true;
            }
        }


        /// <summary>
        /// 检查用户注册的资料是否正确
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool CheckUserRegister(User user)
        {
            ///1. 判断数据是否正确
            if (!user.UserName.StartsWith("wx-"))
            {
                if (!WebAgent.IsUserNameByEnglish(user.UserName))
                {
                    base.Message("用户名只能是英文字母加数字，4～16位之间");
                    return false;
                }
            }
            if (user.Password.Length < 5 || user.Password.Length > 16)
            {
                base.Message("密码长度应在5～16位之间");
                return false;
            }
            if (!string.IsNullOrEmpty(user.Mobile) && !WebAgent.IsMobile(user.Mobile))
            {
                base.Message("手机号码错误");
                return false;
            }
            if (!string.IsNullOrEmpty(user.Email) && !WebAgent.IsEmail(user.Email))
            {
                base.Message("电子邮箱错误");
                return false;
            }

            ///2. 先判断用户名是否存在
            if (this.GetUserID(user.UserName) != 0)
            {
                base.Message("用户名已存在");
                return false;
            }

            ///3. 判断返点
            if (user.AgentID != 0)
            {
                ///4. 判断上级
                User agent = this.GetUserInfo(user.AgentID);
                if (agent == null || agent.Type != User.UserType.Agent)
                {
                    base.Message("上级代理不可用");
                    return false;
                }

                if (!SiteInfo.Setting.IsSameRebate && user.Rebate == agent.Rebate && user.Rebate != SiteInfo.Setting.MinRebate)
                {
                    base.Message("系统不允许开同级号");
                    return false;
                }

                // 临时解决方案
                //if (user.Rebate > 1990 && user.Rebate == agent.Rebate)
                //{
                //    base.Message("1992或者以上帐号不允许开设同级帐号");
                //    return false;
                //}

                if (user.Rebate > agent.Rebate)
                {
                    base.Message("返点不能大于上级");
                    return false;
                }

                //判断配额
                QuotaSetting quota = this.GetUserQuotaList(user.AgentID).Where(t => t.MinRebate <= user.Rebate && t.MaxRebate >= user.Rebate).FirstOrDefault();
                if (quota != null && quota.Count >= quota.Number)
                {
                    base.Message("配额不足");
                    return false;
                }

                user.IsTest = agent.IsTest;
                if (user.IsTest) user.Lock = agent.Lock;
            }
            else
            {
                // 管理员开号 需判断权限

                if (user.Rebate > SiteInfo.Setting.MaxRebate)
                {
                    base.Message("返点不能大于{0}", SiteInfo.Setting.MaxRebate);
                    return false;
                }
            }

            if (user.Rebate < SiteInfo.Setting.MinRebate)
            {
                base.Message("返点不能小于{0}", SiteInfo.Setting.MinRebate);
                return false;
            }

            if (user.Rebate % 2 != 0)
            {
                base.Message("返点不能是单数");
                return false;
            }


            return true;
        }

        /// <summary>
        /// 使用sessionKey直接登录
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool Login(Guid session)
        {
            UserSession userSession = BDC.UserSession.Where(t => t.SiteID == SiteInfo.ID && t.Session == session).FirstOrDefault();
            if (userSession == null) return false;
            User user = this.GetUserInfo(userSession.UserID);
            if (user.Lock.HasFlag(User.LockStatus.Login))
            {
                base.Message("您被锁定登录");
                return false;
            }
            this.SetLoginCookie(userSession.Session, userSession.UserID);
            this.UpdateUserGroup(userSession.UserID);
            return true;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="key">返回session Key值</param>
        /// <returns></returns>
        public bool Login(string userName, string password, out Guid sessionKey)
        {
            sessionKey = Guid.Empty;

            //1、判断用户名是否存在
            int userId = this.GetUserID(userName);
            if (userId == 0)
            {
                base.Message("用户名不存在");
                return false;
            }

            //2、检查密码
            if (!this.CheckPassword(userId, password))
            {
                return false;
            }

            return this.Login(userId, out sessionKey);
        }

        /// <summary>
        /// 设置session缓存,并且保存入数据库
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionKey"></param>
        private void _loginSaveSession(int userId, out Guid sessionKey)
        {
            PlatformType platform = WebAgent.IsMobile() ? (WebAgent.IsWechat() ? PlatformType.Wechat : PlatformType.Mobile) : PlatformType.PC;

            sessionKey = this.GetUserSession(userId, platform);
            if (sessionKey != Guid.Empty && this._userSession.ContainsKey(sessionKey))
            {
                this._userSession.Remove(sessionKey);
            }

            UserSession session = new UserSession() { UserID = userId, SiteID = SiteInfo.ID, Platform = platform, Session = Guid.NewGuid(), UpdateAt = DateTime.Now };
            sessionKey = session.Session;
            if (session.Exists())
            {
                session.Update();
            }
            else
            {
                session.Add();
            }
        }

        /// <summary>
        /// 使用谷歌验证码快捷登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="code"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public bool Login(string userName, int code, out Guid sessionKey)
        {
            int userId = this.GetUserID(userName);
            sessionKey = Guid.Empty;
            if (userId == 0)
            {
                base.Message("用户名不存在");
                return false;
            }

            User user = this.GetUserInfo(userId);

            if (user.SecretKey == Guid.Empty)
            {
                base.Message("您未设定谷歌验证码");
                return false;
            }

            if (!new GoogleAuthenticator().ValidateTwoFactorPIN(user.SecretKey.ToString("N"), code.ToString().PadLeft(6, '0')))
            {
                base.Message("验证码错误");
                return false;
            }

            return this.Login(user.ID, out sessionKey);

        }

        /// <summary>
        /// 使用设备号快捷登录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="uuid"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Login(int userId, string uuid, string key, out Guid sessionKey)
        {
            sessionKey = Guid.Empty;
            if (string.IsNullOrEmpty(uuid))
            {
                base.Message("未获取到设备编号");
                return false;
            }
            if (string.IsNullOrEmpty(key))
            {
                base.Message("请输入设备解锁密钥");
                return false;
            }
            Guid deviceId = WebAgent.GetGuid(uuid);
            key = MD5.toMD5(key);

            UserDevice device = BDC.UserDevice.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.UUID == deviceId && t.Key == key).FirstOrDefault();
            if (device == null)
            {
                base.Message("密钥错误");
                return false;
            }
            return this.Login(userId, out sessionKey);
        }

        /// <summary>
        /// 微信的快捷登录
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="invite">邀请码(如果openid为0，则自动注册到该邀请码下）</param>
        /// <returns>返回对应的用户ID 为0表示获取到资料</returns>
        public bool LoginByWX(string openId, string invite = null)
        {
            UserWechat wx = this.GetUserWechat(openId);
            if (wx == null)
            {
                base.Message("没有绑定微信帐号");
                return false;
            }

            Guid session;
            if (wx.UserID != 0)
            {
                return this.Login(wx.UserID, out session);
            }

            if (string.IsNullOrEmpty(invite)) invite = SiteInfo.Setting.RegisterInvite;

            if (string.IsNullOrEmpty(invite))
            {
                base.Message("未注册");
                return false;
            }

            SP.Studio.GateWay.WeChat.SNSInfo info = new SP.Studio.GateWay.WeChat.SNSInfo(wx.UserInfo);

            User user = new User()
            {
                UserName = "wx-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                NickName = info.nickname,
                Face = this.UploadImage(info.headimgurl, "face", "jpg"),
                Password = SiteInfo.Setting.DefaultPassword,
            };

            if (!this.AddUser(user, invite)) return false;

            wx.UserID = user.ID;
            wx.Update(null, t => t.UserID);

            return this.Login(user.ID, out session);
        }

        /// <summary>
        /// 设置登录后的cookies
        /// </summary>
        /// <param name="session"></param>
        /// <param name="userId"></param>
        private void SetLoginCookie(Guid session, int userId)
        {
            HttpContext.Current.Response.Cookies[BetModule.USERKEY].Value = session.ToString("N");
            HttpContext.Current.Response.Cookies[BetModule.USERINFO].Value = userId.ToString();
            this.SaveLog(userId, "登录成功，浏览器：{0}", HttpContext.Current.Request.UserAgent);
            this.SaveUserInfoLog(userId, UserInfoLog.UserInfoLogType.LastLoginAt, null, DateTime.Now, IPAgent.IP);
            new User()
            {
                ID = userId,
                LoginIP = IPAgent.IP,
                LoginAt = DateTime.Now
            }.Update(null, t => t.LoginIP, t => t.LoginAt);
            this.RemoveUserCache(userId);
        }

        /// <summary>
        /// 验证成功之后用户登录的步骤
        /// </summary>
        /// <param name="userId"></param>

        private bool Login(int userId, out Guid sessionKey)
        {
            sessionKey = Guid.Empty;
            if (userId == 0)
            {
                base.Message("用户ID错误");
                return false;
            }

            User user = this.GetUserInfo(userId);
            if (user.Lock.HasFlag(User.LockStatus.Login))
            {
                base.Message("您被锁定登录");
                return false;
            }
            //4、插入或者修改SessionKey值（对应 usr_Session表）
            this._loginSaveSession(userId, out sessionKey);

            //5、把Session写入Cookie
            this.SetLoginCookie(sessionKey, userId);

            //6、检查是否有未处理的契约
            this.CheckContackLockStatus(userId);

            //7、自动变更用户所在分组
            this.UpdateUserGroup(userId);

            //8、检查需要自动注册的第三方游戏
            foreach (Common.Games.GameSetting setting in GameAgent.Instance().GetGameSetting().Where(t => t.IsOpen && t.IsRegister))
            {
                setting.Setting.CreateUser(userId);
            }

            return true;
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public void Logout()
        {
            if (UserInfo != null)
            {
                this.SetOffline(UserInfo.ID);
            }
            HttpContext.Current.Response.Cookies[BetModule.USERKEY].Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies[BetModule.USERINFO].Expires = DateTime.Now.AddDays(-1);
        }

        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="type"></param>
        /// <param name="payPassword"></param>
        /// <param name="answer"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool Forget(int userId, User.ForgetType type, string payPassword, string answer, string newPassword)
        {
            if (newPassword.Length < 5 || newPassword.Length > 16)
            {
                base.Message("密码长度应在5～16位之间");
                return false;
            }
            User user = new User() { ID = userId, SiteID = SiteInfo.ID, Password = MD5.toMD5(newPassword) };

            //1. 通过资金密码找回
            switch (type)
            {
                case User.ForgetType.PayPassword:
                    payPassword = MD5.Encrypto(payPassword);

                    if (BDC.User.Where(t => t.ID == userId && t.SiteID == SiteInfo.ID && t.PayPassword == payPassword).Count() == 0)
                    {
                        base.Message("资金密码错误");
                        return false;
                    }

                    break;
                case User.ForgetType.Answer:
                    //2. 通过问题答案找回
                    answer = MD5.toMD5(answer);

                    if (BDC.User.Where(t => t.ID == userId && t.SiteID == SiteInfo.ID && t.Answer == answer).Count() == 0)
                    {
                        base.Message("密保答案错误");
                        return false;
                    }

                    break;
            }
            if (this.UpdateUserInfo(user, t => t.Password))
            {
                this.SaveLog(user.ID, "通过{0}找回密码", type.GetDescription());
            }
            return false;
        }

        /// <summary>
        /// 获取/设置访问用户的唯一浏览器编号
        /// </summary>
        /// <returns></returns>
        public Guid GetBowserID()
        {
            if (this.context == null) return Guid.Empty;

            string id = WebAgent.QC(BetModule.BOWSER);
            Guid bowserId = Guid.Empty;
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out bowserId))
            {
                bowserId = Guid.NewGuid();
                //this.context.Response.Cookies[BetModule.BOWSER].Value = bowserId.ToString("N");
                //this.context.Response.Cookies[BetModule.BOWSER].Expires = DateTime.Now.AddYears(1);
            }
            return bowserId;
        }

        /// <summary>
        /// 获取当前用户的sessionKey（数据库中获取）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="create">如果没有登录记录，是否创建一个</param>
        /// <returns></returns>
        public Guid GetUserSession(int userId, PlatformType? platform = null, bool create = false)
        {
            int siteId = this.GetSiteID(userId);
            IQueryable<UserSession> userSession = BDC.UserSession.Where(t => t.SiteID == siteId && t.UserID == userId);
            if (platform != null)
            {
                userSession = userSession.Where(t => t.Platform == platform.Value);
            }
            Guid? session = userSession.Select(t => (Guid?)t.Session).FirstOrDefault();
            if (create && session == null)
            {
                using (DbExecutor db = NewExecutor())
                {
                    if (platform == null) platform = PlatformType.PC;
                    session = Guid.NewGuid();
                    new UserSession()
                    {
                        Platform = platform.Value,
                        Session = session.Value,
                        SiteID = siteId,
                        UpdateAt = DateTime.Now,
                        UserID = userId
                    }.Add(db);
                }
            }
            return session == null ? Guid.Empty : session.Value;
        }

        /// <summary>
        /// 根据设备绑定信息获取已经登陆过的session
        /// </summary>
        /// <param name="host"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public Guid GetUserSession(Guid host, string platform, out int userId)
        {
            userId = 0;
            UserHost user = BDC.UserHost.Where(t => t.SiteID == SiteInfo.ID && t.Platform == platform && t.Host == host).FirstOrDefault();
            if (user == null) return Guid.Empty;

            userId = user.UserID;
            return this.GetUserSession(user.UserID, PlatformType.Mobile, true);
        }

        /// <summary>
        /// 检查用户是否在授权主机上登录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public bool CheckUserHost(int userId, Guid host)
        {
            if (userId == 0) return false;
            return BDC.UserHost.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.Host == host).Count() != 0;
        }

        public bool CheckUserHost(string userName, Guid host)
        {
            return this.CheckUserHost(this.GetUserID(userName), host);
        }

        /// <summary>
        /// 保存主机授权
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="host">主机</param>
        /// <param name="platform">平台类型</param>
        public bool SaveUserHost(int userId, Guid host, string platform)
        {
            if (host == Guid.Empty)
            {
                base.Message("客户端标识获取失败");
                return false;
            }
            if (string.IsNullOrEmpty(platform))
            {
                base.Message("客户端类型获取失败");
                return false;
            }

            if (new UserHost() { SiteID = SiteInfo.ID, Host = host }.Exists(null, t => t.SiteID, t => t.Host))
            {
                new UserHost() { SiteID = SiteInfo.ID, Host = host }.Delete(t => t.SiteID, t => t.Host);
            }
            UserHost userHost = new UserHost()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                Platform = platform
            }.Info();
            if (userHost == null)
            {
                return new UserHost()
                {
                    SiteID = SiteInfo.ID,
                    UserID = userId,
                    Platform = platform,
                    CreateAt = DateTime.Now,
                    Host = host
                }.Add();
            }
            else
            {
                userHost.Host = host;
                userHost.CreateAt = DateTime.Now;
                return userHost.Update(null, t => t.Host, t => t.CreateAt) != 0;
            }
        }

        /// <summary>
        /// 删除保存的客户端标识
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public bool DeleteUserHost(int userId, string platform)
        {
            if (string.IsNullOrEmpty(platform))
            {
                base.Message("客户端类型获取失败");
                return false;
            }

            new UserHost()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                Platform = platform
            }.Delete();
            return true;
        }

        #endregion

        #region ================ 获取/设置用户资料  ===============

        /// <summary>
        /// 获取用户资料（适用于非web程序）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetUserInfo(int userId)
        {
            int siteId = this.GetSiteID(userId);
            return BDC.User.Where(t => t.ID == userId && t.SiteID == siteId).FirstOrDefault();
        }

        /// <summary>
        /// 只在缓存中获取用户账户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetUserInfoByCache(int userId)
        {
            string key = string.Concat(BetModule.USERINFO, "_", userId);
            return (User)HttpRuntime.Cache[key];
        }

        /// <summary>
        /// 获取当前登录用户（有缓存）
        /// 2017.6.9 修改为不依赖站点缓存
        /// </summary>
        /// <returns></returns>
        public User GetUserInfo()
        {
            if (context == null) return null;
            string userSession = string.IsNullOrEmpty(context.Request.Headers[BetModule.USERKEY]) ?
                WebAgent.QC(BetModule.USERKEY) : context.Request.Headers[BetModule.USERKEY];
            Guid sessionId;
            if (string.IsNullOrEmpty(userSession) || !Guid.TryParse(userSession, out sessionId)) return null;
            int userId = this.GetUserID(sessionId);
            if (userId == 0)
            {
                context.Request.Cookies.Remove(BetModule.USERKEY);
                return null;
            }
            string key = string.Concat(BetModule.USERINFO, "_", userId);
            User user = (User)HttpRuntime.Cache[key];
            if (user != null && user.SiteID == SiteInfo.ID)
            {
                this.SetOnlineStatus(user.ID, true);
                return user;
            }
            lock (key)
            {
                this.UpdateUserActive(userId);
                user = this.GetUserInfo(userId);
                if (user == null) return null;
                HttpRuntime.Cache.Insert(key, user, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10), CacheItemPriority.Default, (removeKey, removeValue, reason) =>
                {
                    User removeUser = (User)removeValue;
                    if (removeUser != null)
                    {
                        this.SetOnlineStatus(removeUser.ID, false);
                    }
                });
                this.SetOnlineStatus(user.ID, true);
                return user;
            }
        }

        /// <summary>
        /// 清除用户缓存
        /// </summary>
        /// <param name="userId"></param>
        public void RemoveUserCache(int userId)
        {
            string key = string.Concat(BetModule.USERINFO, "_", userId);
            if (HttpRuntime.Cache[key] != null)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }

        /// <summary>
        /// 用户的登录状态session
        /// </summary>
        private Dictionary<Guid, int> _userSession = new Dictionary<Guid, int>();

        private const string _LOCK_USERSESSION = "_LOCK_USERSESSION";
        /// <summary>
        /// 通过用户登录Key获取用户ID（适用于非Web）
        /// </summary>
        public int GetUserID(Guid session)
        {
            if (session == Guid.Empty) return 0;
            if (_userSession.ContainsKey(session)) return _userSession[session];

            lock (_LOCK_USERSESSION)
            {
                if (_userSession.ContainsKey(session)) return _userSession[session];
                using (DbExecutor db = NewExecutor())
                {
                    object userId = db.ExecuteScalar(CommandType.Text, "SELECT UserID FROM usr_Session WHERE Session = @Session",
                            NewParam("@Session", session));

                    if (userId == null) return 0;
                    try
                    {
                        _userSession.Add(session, (int)userId);
                        return (int)userId;
                    }
                    catch (Exception ex)
                    {
                        SystemAgent.Instance().AddErrorLog(SiteInfo.ID, ex, "GetUserID 方法插入数据时候错误,Count = " + _userSession.Count);
                        return (int)userId;
                    }
                }
            }
        }

        /// <summary>
        /// 修改用户指定字段资料
        /// 注意：只能修改当前用户
        /// </summary>
        /// <param name="user">用户资料</param>
        /// <returns></returns>
        public bool UpdateUserInfo(User user, params Expression<Func<User, object>>[] fields)
        {
            if (user.ID == 0 || user.SiteID != SiteInfo.ID)
            {
                base.Message("未指定要操作的账户");
                return false;
            }

            try
            {
                if (user.Update(null, fields) != 0)
                {
                    this.SaveLog(user.ID, "修改资料", user.ToJson(fields));
                    string[] infoType = Enum.GetNames(typeof(UserInfoLog.UserInfoLogType));

                    foreach (Expression<Func<User, object>> field in fields)
                    {
                        PropertyInfo property = field.GetPropertyInfo();
                        if (infoType.Contains(property.Name))
                        {
                            this.SaveUserInfoLog(user.ID, property.Name.ToEnum<UserInfoLog.UserInfoLogType>());
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                base.Message(ex.Message);
            }
            finally
            {
                this.RemoveUserCache(user.ID);
            }
            return false;
        }

        /// <summary>
        /// 重置用户资料
        /// </summary>
        /// <param name="user"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool ResetUserInfo(User user, Expression<Func<User, object>> field, object value = null)
        {
            if (user.ID == 0 || user.SiteID != SiteInfo.ID)
            {
                base.Message("未指定要操作的账户");
                return false;
            }
            PropertyInfo property = field.ToPropertyInfo();
            switch (property.Name)
            {
                case "AccountName":
                    if (this.GetBankAccountList(user.ID).Count != 0)
                    {
                        base.Message("用户名下已绑定银行，无法重置提现账户名");
                        return false;
                    }
                    break;
                case "PayPassword":
                case "Mobile":
                case "Email":
                case "QQ":
                    if (value == null)
                        value = string.Empty;
                    break;
                case "Password":
                    if (string.IsNullOrEmpty(SiteInfo.Setting.DefaultPassword))
                    {
                        base.Message("没有设置默认密码");
                    }
                    value = MD5.toMD5(SiteInfo.Setting.DefaultPassword);
                    break;
                case "SecretKey":
                    value = Guid.Empty;
                    break;
            }
            if (value == null)
            {
                base.Message("没有指定要重置的类型");
                return false;
            }
            property.SetValue(user, value, null);
            if (user.Update(null, field) == 1)
            {
                if (AdminInfo != null) AdminInfo.Log(AdminLog.LogType.User, "重置用户{0}的{1}", user.UserName, property.Name);
                user.Log("重置用户{0}的{1}", user.UserName, property.Name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 通过手机验证码设置手机（一个手机号码只能绑定给一个用户）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool SaveUserMobile(int userId, string mobile, string code)
        {
            if (!SiteAgent.Instance().CheckCode(mobile, code)) return false;
            if (!this.CheckMobile(userId, mobile)) return false;

            return this.UpdateUserInfo(new User() { ID = userId, SiteID = SiteInfo.ID, Mobile = mobile }, t => t.Mobile);
        }

        /// <summary>
        /// 检查手机号码是否已经被其他用户绑定
        /// 已被绑定返回false
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public bool CheckMobile(int userId, string mobile)
        {
            if (BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.Mobile == mobile && t.ID != userId).Count() != 0)
            {
                base.Message("该手机号码已被其他用户绑定");
                return false;
            }
            return true;
        }

        private Dictionary<int, string> _userName = new Dictionary<int, string>();
        /// <summary>
        /// 通过用户ID获取用户名
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetUserName(int userId)
        {
            if (userId == 0) return null;
            lock (_userName)
            {
                if (_userName.ContainsKey(userId)) return _userName[userId];
                var list = BDC.User.Where(t => t.ID == userId);
                if (SiteInfo != null) list = list.Where(t => t.SiteID == SiteInfo.ID);
                string userName = list.Select(t => t.UserName).FirstOrDefault();
                if (string.IsNullOrEmpty(userName)) return null;

                _userName.Add(userId, userName);
                return userName;
            }
        }

        /// <summary>
        /// 使用数据库事务获取用户名
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetUserName(DbExecutor db, int userId)
        {
            if (userId == 0) return null;
            lock (_userName)
            {
                if (_userName.ContainsKey(userId)) return _userName[userId];
                string userName = (string)db.ExecuteScalar(CommandType.Text,
                    "SELECT UserName FROM Users WHERE SiteID = @SiteID AND UserID = @UserID",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@UserID", userId));

                if (string.IsNullOrEmpty(userName)) return null;

                _userName.Add(userId, userName);
                return userName;
            }
        }

        /// <summary>
        /// 获取用户的头像
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetUserFace(int userId)
        {
            string face = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.ID == userId).Select(t => t.Face).FirstOrDefault();
            if (string.IsNullOrEmpty(face)) face = "/images/user.png";
            return SysSetting.GetSetting().GetImage(face);
        }

        /// <summary>
        /// 用户名对应用户ID缓存对象
        /// </summary>
        private Dictionary<string, int> _userIDCache = new Dictionary<string, int>();

        /// <summary>
        /// 通过用户名获取用户ID
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public int GetUserID(string userName)
        {
            string key = string.Concat(SiteInfo.ID, "_", userName);
            if (_userIDCache.ContainsKey(key)) return _userIDCache[key];

            int? userId = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.UserName == userName).Select(t => (int?)t.ID).FirstOrDefault();
            lock (_userIDCache)
            {
                if (_userIDCache.ContainsKey(key)) return _userIDCache[key];
                if (userId != null)
                {
                    _userIDCache.Add(key, userId.Value);
                    return userId.Value;
                }
            }
            return 0;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="newpassword"></param>
        /// <returns></returns>
        public bool UpdatePassword(int userId, string password, string newPassword)
        {
            if (string.IsNullOrEmpty(password))
            {
                base.Message("请输入当前密码");
                return false;
            }
            if (newPassword.Length < 5 || newPassword.Length > 16)
            {
                base.Message("密码长度应在5～16位之间");
                return false;
            }

            if (!this.CheckPassword(userId, password))
            {
                base.MessageClean("原始登录密码输入错误");
                return false;
            }

            UserInfo.Password = MD5.toMD5(newPassword);
            this.UpdateUserInfo(UserInfo, t => t.Password);

            return true;
        }

        /// <summary>
        /// 直接修改密码
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool UpdatePassword(string userName, string password)
        {
            int userId = this.GetUserID(userName);
            if (userId == 0)
            {
                base.Message("用户名不存在");
                return false;
            }
            if (password.Length < 5 || password.Length > 16)
            {
                base.Message("密码长度应在5～16位之间");
                return false;
            }
            User user = new User()
            {
                ID = userId,
                SiteID = SiteInfo.ID,
                Password = MD5.toMD5(password)
            };
            return this.UpdateUserInfo(user, t => t.Password);
        }

        /// <summary>
        /// 设置或者修改资金密码
        /// 如果是第一次设置资金密码，则资金密码等于登录密码
        /// </summary>
        /// <param name="paypassword"></param>
        /// <param name="newpaypassword"></param>
        /// <returns></returns>
        public bool UpdatePayPassword(int userId, string payPassword, string newPayPassword)
        {
            if (!this.CheckLogin(userId)) return false;

            if (newPayPassword.Length < 5 || newPayPassword.Length > 16)
            {
                base.Message("密码长度应在5～16位之间");
                return false;
            }

            string userPayPassword = BDC.User.Where(t => t.ID == userId && t.SiteID == SiteInfo.ID).Select(t => t.PayPassword).FirstOrDefault().Trim();
            if (string.IsNullOrEmpty(userPayPassword))
            {
                if (!this.CheckPassword(userId, payPassword))
                {
                    base.MessageClean("请输入您的登录密码作为原始密码");
                    return false;
                }
            }
            else if (!this.CheckPayPassword(userId, payPassword))
            {
                base.MessageClean("原始资金密码输入错误");
                return false;
            }

            if (this.CheckPassword(userId, newPayPassword))
            {
                base.Message("资金密码不能与登录密码相同");
                return false;
            }

            this.UserInfo.PayPassword = MD5.Encrypto(newPayPassword);

            if (!this.UpdateUserInfo(UserInfo, t => t.PayPassword))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查资金密码是否正确(自带消息返回）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="payPassword"></param>
        /// <returns></returns>
        public bool CheckPayPassword(int userId, string payPassword)
        {
            string userPayPassword = BDC.User.Where(t => t.ID == userId && t.SiteID == SiteInfo.ID).Select(t => t.PayPassword).FirstOrDefault();
            if (string.IsNullOrEmpty(userPayPassword))
            {
                base.Message("暂未设置资金密码");
                return false;
            }
            if (userPayPassword != MD5.Encrypto(payPassword))
            {
                base.Message("资金密码错误");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查密码回答问题是否正确
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public bool CheckAnswer(int userId, string answer)
        {
            string userAnswer = BDC.User.Where(t => t.ID == userId && t.SiteID == SiteInfo.ID).Select(t => t.Answer).FirstOrDefault();
            if (string.IsNullOrEmpty(userAnswer))
            {
                base.Message("暂未设置安全问题");
                return false;
            }
            if (userAnswer != MD5.toMD5(answer))
            {
                base.Message("安全问题错误");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查登录密码是否正确
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CheckPassword(int userId, string password)
        {
            string userPassword = BDC.User.Where(t => t.ID == userId && t.SiteID == SiteInfo.ID).Select(t => t.Password).FirstOrDefault();
            if (string.IsNullOrEmpty(userPassword))
            {
                base.Message("用户不存在");
                return false;
            }
            if (!userPassword.Equals(MD5.toMD5(password), StringComparison.CurrentCultureIgnoreCase))
            {
                base.Message("密码错误");
                return false;
            }
            return true;
        }


        /// <summary>
        /// 设置密保答案
        /// </summary>
        /// <returns></returns>
        public bool SaveSafeAnswer(int userID, User.QuestionType question, string answer)
        {
            if (!this.CheckLogin(userID)) return false;

            if (!string.IsNullOrEmpty(UserInfo.Answer))
            {
                base.Message("您已经设置了安全答案，如需修改请联系客服");
                return false;
            }
            if (question == User.QuestionType.None)
            {
                base.Message("请选择安全问题");
                return false;
            }
            if (string.IsNullOrEmpty(answer))
            {
                base.Message("请输入安全问题答案");
                return false;
            }

            UserInfo.Question = question;
            UserInfo.Answer = MD5.toMD5(answer);
            this.UpdateUserInfo(UserInfo, t => t.Question, t => t.Answer);
            return true;
        }


        /// <summary>
        /// 获取当前所有在线的用户
        /// </summary>
        /// <returns></returns>
        public int[] GetOnlineUser()
        {
            return BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.IsOnline).Select(t => t.ID).ToArray();
        }

        /// <summary>
        /// 获取当前在线用户的数量
        /// </summary>
        /// <returns></returns>
        public int GetUserOnlineCount()
        {
            return BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.IsOnline).Count();
        }

        /// <summary>
        /// 获取用户上一次操作的时间记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserInfoLog> GetUserInfoLog(int userId)
        {
            return BDC.UserInfoLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).ToList();
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool UpdateUserFace(int userId, HttpPostedFile file)
        {
            if (userId == 0)
            {
                base.Message("用户授权信息错误");
                return false;
            }

            User user = this.GetUserInfoByCache(userId) ?? new User() { ID = userId, SiteID = SiteInfo.ID };
            string face = this.UploadImage(file, "face");
            if (string.IsNullOrEmpty(face)) return false;
            user.Face = face;
            return this.UpdateUserInfo(user, t => t.Face);
        }

        /// <summary>
        /// 上传头像（使用二进制文件直接上传）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool UpdateUserFace(int userId, byte[] file, string ext)
        {

            if (userId == 0)
            {
                base.Message("用户授权信息错误");
                return false;
            }
            User user = this.GetUserInfoByCache(userId) ?? new User() { ID = userId, SiteID = SiteInfo.ID };
            string face = this.UploadImage(file, "face", ext);
            if (string.IsNullOrEmpty(face)) return false;
            user.Face = face;
            return this.UpdateUserInfo(user, t => t.Face);
        }

        /// <summary>
        /// 设置用户在线或者离线（兼容非web程序）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="online"></param>
        public void SetOnlineStatus(int userId, bool online)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET IsOnline = @Online WHERE SiteID = @SiteID AND UserID = @UserID AND IsOnline != @Online",
                    NewParam("@Online", online),
                    NewParam("@SiteID", this.GetSiteID(userId, db)),
                    NewParam("@UserID", userId));
            }
        }

        /// <summary>
        /// 踢出用户下线
        /// </summary>
        /// <param name="userId"></param>
        public bool SetOffline(int userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                if (db.ExecuteNonQuery(CommandType.StoredProcedure, "tool_SetUserOffline",
                       NewParam("@SiteID", SiteInfo.ID),
                       NewParam("@UserID", userId)) == 0)
                {
                    return false;
                }

                foreach (Guid session in _userSession.Where(t => t.Value == userId).Select(t => t.Key).ToArray())
                {
                    _userSession.Remove(session);
                }

                return true;
            }
        }

        /// <summary>
        /// 更新当前用户的在线时间
        /// </summary>
        /// <param name="userId"></param>
        public void UpdateUserActive(int userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET IsOnline = 1, ActiveAt = GETDATE() WHERE UserID = @UserID",
                    NewParam("@UserID", userId));
            }
        }

        /// <summary>
        /// 用户重置登录密码（找回密码处调用）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="payPassword"></param>
        /// <param name="answer"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public bool ResetUserPassword(int userId, string payPassword, string answer, string accountName)
        {
            User user = this.GetUserInfo(userId);
            bool isPassword = false;
            if (!string.IsNullOrEmpty(user.PayPassword))
            {
                if (!this.CheckPayPassword(userId, payPassword))
                {
                    return false;
                }
                isPassword = true;
            }
            if (!string.IsNullOrEmpty(user.Answer))
            {
                if (!this.CheckAnswer(userId, answer))
                {
                    return false;
                }
                isPassword = true;
            }
            if (!string.IsNullOrEmpty(user.AccountName))
            {
                if (accountName != user.AccountName)
                {
                    base.Message("银行卡账户名错误");
                    return false;
                }
                isPassword = true;
            }
            if (!isPassword)
            {
                base.Message("当前账户没有密码安全信息");
                return false;
            }

            return this.ResetUserInfo(user, t => t.Password);
        }

        /// <summary>
        /// 更新用户的锁定状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <param name="isLock"></param>
        /// <returns></returns>
        public bool UpdateUserLockStatus(int userId, User.LockStatus status, bool isLock)
        {
            using (DbExecutor db = NewExecutor())
            {
                int siteId = this.GetSiteID(userId, db);

                User.LockStatus? lockStatus = BDC.User.Where(t => t.SiteID == siteId && t.ID == userId).Select(t => (User.LockStatus?)t.Lock).FirstOrDefault();
                if (lockStatus == null)
                {
                    base.Message("用户ID错误");
                    return false;
                }

                switch (isLock)
                {
                    case false:
                        if (lockStatus.Value.HasFlag(status))
                        {
                            lockStatus = (User.LockStatus)((byte)lockStatus - (byte)status);
                        }
                        break;
                    case true:
                        if (!lockStatus.Value.HasFlag(status))
                        {
                            lockStatus = (User.LockStatus)((byte)lockStatus + (byte)status);
                        }
                        break;
                }

                if (db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET Status = @Status,Lock = @Lock WHERE SiteID = @SiteID AND UserID = @UserID",
                      NewParam("@Status", (byte)lockStatus == 0 ? User.UserStatus.Normal : User.UserStatus.Lock),
                      NewParam("@Lock", lockStatus.Value),
                      NewParam("@SiteID", siteId),
                      NewParam("@UserID", userId)) != 0)
                {
                    if (AdminInfo != null)
                        AdminInfo.Log(AdminLog.LogType.User, string.Format("用户{0}的{1}状态进行{2}", this.GetUserName(userId), status.GetDescription(), isLock ? "锁定" : "解锁"));
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 更改用户功能的开放状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <param name="isOpen"></param>
        /// <returns></returns>
        public bool UpdateUserFunctionStatus(int userId, User.FunctionType status, bool isOpen)
        {
            using (DbExecutor db = NewExecutor())
            {
                int siteId = this.GetSiteID(userId, db);

                User.FunctionType? function = BDC.User.Where(t => t.SiteID == siteId && t.ID == userId).Select(t => (User.FunctionType?)t.Function).FirstOrDefault();
                if (function == null)
                {
                    base.Message("用户ID错误");
                    return false;
                }

                // 一条线上只允许一个总代分红
                if (isOpen && status == User.FunctionType.Dividends)
                {
                    //#1 检查我的下级有没有人开放总代分红
                    int childId = this.GetDividendsByChild(userId);
                    if (childId != 0)
                    {
                        base.Message("下级用户{0}已经设置成为了总代分红", this.GetUserName(childId));
                        return false;
                    }

                    //#2 检查上级有没有开放总代分红
                    int parentId = this.GetDividendsByParent(userId);
                    if (parentId != 0)
                    {
                        base.Message("上级用户{0}已经设置成为了总代分红", this.GetUserName(parentId));
                        return false;
                    }

                }

                switch (isOpen)
                {
                    case false:
                        if (function.Value.HasFlag(status))
                        {
                            function = (User.FunctionType)((int)function - (int)status);
                        }
                        break;
                    case true:
                        if (!function.Value.HasFlag(status))
                        {
                            function = (User.FunctionType)((int)function + (int)status);
                        }
                        break;
                }

                if (db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET [Function] = @Function WHERE SiteID = @SiteID AND UserID = @UserID",
                      NewParam("@Function", function.Value),
                      NewParam("@SiteID", siteId),
                      NewParam("@UserID", userId)) != 0)
                {
                    if (AdminInfo != null)
                        AdminInfo.Log(AdminLog.LogType.User, string.Format("用户{0}的{1}功能进行{2}", this.GetUserName(userId), status.GetDescription(), isOpen ? "开放" : "关闭"));
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 检查下级是否有设置总代分红
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetDividendsByChild(int userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                string sql = "SELECT Users.UserID FROM usr_Depth JOIN Users ON usr_Depth.ChildID = Users.UserID WHERE usr_Depth.UserID = @UserID AND [Function] & @Status = @Status";
                int? uid = (int?)db.ExecuteScalar(CommandType.Text, sql,
                    NewParam("@UserID", userId),
                    NewParam("@Status", User.FunctionType.Dividends));

                if (uid == null) return 0;
                return uid.Value;
            }
        }

        /// <summary>
        /// 检查上级是否设置了总代分红
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetDividendsByParent(int userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                string sql = "SELECT Users.UserID FROM usr_Depth JOIN Users ON usr_Depth.UserID = Users.UserID WHERE usr_Depth.ChildID = @UserID AND [Function] & @Status = @Status";
                int? uid = (int?)db.ExecuteScalar(CommandType.Text, sql,
                    NewParam("@UserID", userId),
                    NewParam("@Status", User.FunctionType.Dividends));

                if (uid == null) return 0;
                return uid.Value;
            }
        }

        /// <summary>
        /// 检查用户是否被锁定（适用于非web程序）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool CheckUserLockStatus(int userId, params User.LockStatus[] status)
        {
            int siteId = this.GetSiteID(userId, null);
            User.LockStatus? lockStatus = BDC.User.Where(t => t.SiteID == siteId && t.ID == userId).Select(t => (User.LockStatus?)t.Lock).FirstOrDefault();
            if (lockStatus == null) return false;
            foreach (User.LockStatus t in status)
            {
                if (lockStatus.Value.HasFlag(t)) return true;
            }
            return false;
        }

        /// <summary>
        /// 更改用户的测试帐号状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public bool UpdateUserTest(int userId, bool isTest)
        {
            using (DbExecutor db = NewExecutor())
            {
                bool success = db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET IsTest = @IsTest WHERE SiteID = @SiteID AND UserID = @UserID AND IsTest != @IsTest",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@UserID", userId),
                    NewParam("@IsTest", isTest)) > 0;
                if (success)
                {
                    AdminInfo.Log(AdminLog.LogType.User, "标记用户{0}的测试字段为{1}", this.GetUserName(userId), isTest);
                }
                else
                {
                    base.Message("保存失败");
                }
                return success;
            }
        }

        /// <summary>
        /// 修改用户的返点
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="rebate"></param>
        /// <returns></returns>
        public bool UpdateUserRebate(int userId, int rebate)
        {
            if (rebate <= 0)
            {
                base.Message("返点输入错误");
                return false;
            }
            if (rebate % 2 != 0)
            {
                base.Message("返点必须为偶数");
                return false;
            }

            if (rebate < SiteInfo.Setting.MinRebate)
            {
                base.Message("返点不能小于系统最低值");
                return false;
            }

            int? maxChildRebate = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.AgentID == userId).Max(t => (int?)t.Rebate);
            if (maxChildRebate != null)
            {
                if (rebate < maxChildRebate.Value)
                {
                    base.Message("不能小于下级的点位{0}", maxChildRebate.Value);
                    return false;
                }
                if (!SiteInfo.Setting.IsSameRebate && rebate == maxChildRebate.Value)
                {
                    base.Message("不能等于下级的点位{0}", maxChildRebate.Value);
                    return false;
                }
            }

            int agentID = this.GetAgentID(userId);
            int maxRebate = SiteInfo.Setting.MaxRebate;
            if (agentID != 0)
            {
                maxRebate = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.ID == agentID).Select(t => t.Rebate).FirstOrDefault();
            }

            if (rebate > maxRebate)
            {
                base.Message("返点超过上级");
                return false;
            }

            if (agentID != 0 && !SiteInfo.Setting.IsSameRebate && rebate == maxRebate)
            {
                base.Message("系统不允许返点与上级相同");
                return false;
            }



            //判断配额
            QuotaSetting quota = this.GetUserQuotaList(agentID).Where(t => t.MinRebate <= rebate && t.MaxRebate >= rebate).FirstOrDefault();
            if (quota != null && quota.Count >= quota.Number)
            {
                base.Message("配额不足");
                return false;
            }

            if (this.UpdateUserInfo(new User() { ID = userId, SiteID = SiteInfo.ID, Rebate = rebate }, t => t.Rebate))
            {
                if (AdminInfo != null)
                {
                    AdminInfo.Log(AdminLog.LogType.User, "修改用户{0}的返点为{1}", this.GetUserName(userId), rebate);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查用户的资料是否正确
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="mobile"></param>
        /// <param name="cardNo"></param>
        /// <returns></returns>
        public bool CheckUser(string userName, string mobile, string accountName, string cardNo)
        {
            int userId = this.GetUserID(userName);
            if (userId == 0)
            {
                base.Message("用户名不存在");
                return false;
            }

            User user = this.GetUserInfo(userId);
            if (user.Mobile != mobile)
            {
                base.Message("手机号码错误");
                return false;
            }

            if (user.AccountName != accountName)
            {
                base.Message("姓名错误");
                return false;
            }

            List<BankAccount> list = this.GetBankAccountList(userId);
            if (list.Count != 0 && !this.GetBankAccountList(userId).Exists(t => t.Account == cardNo))
            {
                base.Message("银行卡号错误");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 保存用户的谷歌验证码
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool SaveSecretKey(int userId, string key, string code)
        {
            User user = this.GetUserInfo(userId);
            if (user.SecretKey != Guid.Empty)
            {
                base.Message("您已经设定了谷歌验证码，如需取消请联系客服");
                return false;
            }

            if (!new GoogleAuthenticator().ValidateTwoFactorPIN(key, code))
            {
                base.Message("验证码错误");
                return false;
            }

            user.SecretKey = Guid.Parse(key);
            user.Update(null, t => t.SecretKey);

            this.SaveUserInfoLog(userId, UserInfoLog.UserInfoLogType.SecretKey);

            return true;
        }

        /// <summary>
        /// 修改所属代理
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public bool UpdateUserAgent(int userId, int agentId)
        {
            User user = this.GetUserInfo(userId);
            if (user == null)
            {
                base.Message("用户ID错误");
                return false;
            }
            using (DbExecutor db = NewExecutor())
            {
                System.Data.Common.DbParameter success = NewParam("@Success", false, DbType.Boolean, 1, ParameterDirection.Output);
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "UpdateUserAgent",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@UserID", userId),
                    NewParam("@AgentID", agentId),
                    success);
                if (success.Value == DBNull.Value || !(bool)success.Value)
                {
                    base.Message("修改失败，可能上下级关系错误");
                    return false;
                }
                if (AdminInfo != null)
                {
                    AdminInfo.Log(AdminLog.LogType.User, "修改用户{0}的上下级关系，旧上级：{1}，新上级：{2}", user.UserName, this.GetUserName(user.AgentID), this.GetUserName(agentId));
                }
                return true;
            }
        }

        /// <summary>
        /// 保存用户的设备信息
        /// </summary>
        /// <returns></returns>
        public bool SaveUserDevice(string model, string uuid, string key = null)
        {
            if (string.IsNullOrEmpty(model))
            {
                base.Message("型号错误");
                return false;
            }
            if (string.IsNullOrEmpty(uuid))
            {
                base.Message("设备编号错误");
                return false;
            }
            if (!string.IsNullOrEmpty(key)) key = MD5.toMD5(key);
            UserDevice device = new UserDevice()
            {
                SiteID = SiteInfo.ID,
                UserID = UserInfo.ID,
                Model = model,
                UUID = WebAgent.GetGuid(uuid),
                Key = key,
                UpdateAt = DateTime.Now
            };

            if (!device.Exists())
            {
                return device.Add();
            }
            else
            {
                if (string.IsNullOrEmpty(key))
                {
                    return device.Update(null, t => t.Model, t => t.UpdateAt) == 1;
                }
                else
                {
                    return device.Update(null, t => t.Key) == 1;
                }
            }
        }

        /// <summary>
        /// 保存用户的设备信息以及型号
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="model"></param>
        /// <param name="uuid"></param>
        /// <param name="phone">电话号码</param>
        /// <returns></returns>
        public bool SaveUserDevice(int userId, string model, string uuid, string phone)
        {
            if (string.IsNullOrEmpty(model))
            {
                base.Message("型号错误");
                return false;
            }
            if (string.IsNullOrEmpty(uuid))
            {
                base.Message("设备编号错误");
                return false;
            }
            UserDevice device = new UserDevice()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                Model = model,
                UUID = WebAgent.GetGuid(uuid),
                Phone = phone,
                Key = string.Empty,
                UpdateAt = DateTime.Now
            };
            if (!device.Exists()) return device.Add();

            device.Update(null, t => t.UpdateAt);
            if (!string.IsNullOrEmpty(device.Model)) device.Update(null, t => t.Model);
            if (!string.IsNullOrEmpty(device.Phone)) device.Update(null, t => t.Phone);
            return true;
        }

        /// <summary>
        /// 清除用户的手势
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public bool RemoveUserDeviceKey(int userId, string uuid)
        {
            UserDevice device = new UserDevice()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                UUID = WebAgent.GetGuid(uuid),
                Key = string.Empty
            };

            return device.Update(null, t => t.Key) == 1;
        }

        /// <summary>
        /// 获取用户登录的设备列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserDevice> GetUserDeviceList(int userId)
        {
            return BDC.UserDevice.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID).OrderByDescending(t => t.UpdateAt).ToList();
        }

        /// <summary>
        /// 获取在设备上登录过的用户ID
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public List<User> GetUserDeviceList(string uuid)
        {
            Guid deviceId = WebAgent.GetGuid(uuid);
            return BDC.UserDevice.Where(t => t.SiteID == SiteInfo.ID && t.UUID == deviceId && t.Key != "").Join(BDC.User.Where(t => t.SiteID == SiteInfo.ID),
                t => t.UserID, t => t.ID, (device, user) => user).ToList();
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public bool DeleteDevice(int userId, Guid uuid)
        {
            UserDevice device = new UserDevice()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                UUID = uuid
            };

            return device.Delete() == 1;
        }

        #endregion

        #region =============  注册链接相关  ==============

        /// <summary>
        /// 获取注册链接信息
        /// </summary>
        public UserInvite GetUserInviteInfo(string inviteId)
        {
            return BDC.UserInvite.Where(t => t.SiteID == SiteInfo.ID && t.ID == inviteId).FirstOrDefault();
        }

        /// <summary>
        /// 获取用户设置的邀请链接列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserInvite> GetInviteList(int userId)
        {
            return BDC.UserInvite.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).OrderByDescending(t => t.CreateAt).ToList();
        }


        /// <summary>
        /// 保存邀请链接
        /// </summary>
        /// <param name="invite"></param>
        /// <returns></returns>
        public bool SaveUserInvite(UserInvite invite)
        {
            if (invite.Rebate % 2 != 0)
            {
                base.Message("返点必须是双数");
                return false;
            }
            if (!SiteInfo.Setting.IsSameRebate && invite.Rebate == UserInfo.Rebate)
            {
                base.Message("不允许同级账号");
                return false;
            }
            if (invite.Rebate < SiteInfo.Setting.MinRebate || invite.Rebate > UserInfo.Rebate)
            {
                base.Message("返点应在{0}～{1}", SiteInfo.Setting.MinRebate, UserInfo.Rebate);
                return false;
            }
            if (UserInfo.Type != User.UserType.Agent)
            {
                base.Message("您不是代理");
                return false;
            }

            invite.SiteID = SiteInfo.ID;
            invite.UserID = UserInfo.ID;
            invite.CreateAt = DateTime.Now;
            invite.ID = WebAgent.NumberToShort(WebAgent.GetRandom()).PadLeft(4, '0');
            return invite.Add();
        }

        /// <summary>
        /// 如果没有邀请链接则自动创建一个邀请链接
        /// 如果已经创建则返回最近一个创建的邀请链接
        /// </summary>
        /// <param name="userId">当前用户</param>
        /// <returns></returns>
        public UserInvite SaveUserInvite(int userId)
        {
            UserInvite invite = BDC.UserInvite.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).OrderByDescending(t => t.CreateAt).FirstOrDefault();
            if (invite != null) return invite;

            User user = this.GetUserInfo(userId);
            if (user == null)
            {
                base.Message("用户ID错误");
                return null;
            }
            if (UserInfo.Type != User.UserType.Agent)
            {
                base.Message("当前用户不是代理");
                return null;
            }

            invite = new UserInvite()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                CreateAt = DateTime.Now,
                ID = WebAgent.NumberToShort(WebAgent.GetRandom()),
                Member = 0,
                Type = User.UserType.Agent,
                Rebate = SiteInfo.Setting.IsSameRebate ? user.Rebate : Math.Max(SiteInfo.Setting.MinRebate, user.Rebate - 2)
            };

            if (invite.Add())
            {
                return invite;
            }
            return null;
        }

        /// <summary>
        /// 删除邀请链接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteInvite(string id)
        {
            UserInvite invite = this.GetUserInviteInfo(id);
            if (invite == null)
            {
                base.Message("编号错误");
                return false;
            }

            if (!this.CheckLogin(invite.UserID)) return false;

            return invite.Delete() != 0;
        }

        #endregion

        #region ============  用户日志  ===============

        /// <summary>
        /// 用户操作日志
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void SaveLog(int userId, string format, params object[] args)
        {
            string content = args.Length == 0 ? format : string.Format(format, args);

            new UserLog()
            {
                Content = content,
                CreateAt = DateTime.Now,
                IP = IPAgent.IP,
                SiteID = SiteInfo.ID,
                UserID = userId,
                BowserID = this.GetBowserID(),
                AdminID = AdminInfo == null ? 0 : AdminInfo.ID
            }.Add();
        }

        /// <summary>
        /// 保存上一次修改资料的时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public void SaveUserInfoLog(int userId, UserInfoLog.UserInfoLogType type, string description = null, DateTime? updateAt = null, string ip = null)
        {
            if (string.IsNullOrEmpty(description)) description = this.context.Request.Url.ToString();

            UserInfoLog log = new UserInfoLog()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                Type = type,
                Description = description,
                IP = string.IsNullOrEmpty(ip) ? IPAgent.IP : ip,
                UpdateAt = updateAt == null ? DateTime.Now : updateAt.Value
            };
            if (log.Exists())
            {
                log.Update(null, t => t.UpdateAt, t => t.IP, t => t.Description);
            }
            else
            {
                log.Add();
            }
        }

        #endregion

        #region ================ 用户层级关系  ==================

        /// <summary>
        /// 获取用户的上级列表（不包括自己）从近到远
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<int> GetUserParentList(int userId)
        {
            int siteId = this.GetSiteID(userId);
            return BDC.UserDepth.Where(t => t.SiteID == siteId && t.ChildID == userId).OrderBy(t => t.Depth).Select(t => t.UserID).ToList();
        }

        /// <summary>
        /// 获取用户上级（包括自己） 从近到远
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="parentId">截止的上级</param>
        /// <returns></returns>
        public List<int> GetUserParentList(int userId, int parentId)
        {
            int siteId = this.GetSiteID(userId);
            List<int> list = new List<int>() { userId };
            if (userId == parentId) return list;
            foreach (int id in BDC.UserDepth.Where(t => t.SiteID == siteId && t.ChildID == userId).OrderBy(t => t.Depth).Select(t => t.UserID))
            {
                list.Add(id);
                if (id == parentId) break;
            }
            return list;
        }

        /// <summary>
        /// 获取用户所有的子集
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<int> GetUserChild(int userId)
        {
            return BDC.UserDepth.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).Select(t => t.ChildID).ToList();
        }

        /// <summary>
        /// 判断是否是下级
        /// </summary>
        /// <param name="agentId">上级</param>
        /// <param name="userId">下级</param>
        /// <returns></returns>
        public bool IsUserChild(int agentId, int userId)
        {
            if (agentId == userId) return true;
            return BDC.UserDepth.Where(t => t.SiteID == SiteInfo.ID && t.UserID == agentId && t.ChildID == userId).Count() != 0;
        }

        /// <summary>
        /// 获取用户下级的数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetChildCount(int userId)
        {
            return BDC.UserDepth.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).Count();
        }

        /// <summary>
        /// 获取用户的直属下级
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<User> GetChildList(int userId)
        {
            return BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.AgentID == userId).ToArray().OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// 获取用户的上级和下级（可聊天的对象）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int[] GetUserFriends(int userId)
        {
            return BDC.User.Where(t => t.SiteID == SiteInfo.ID && (t.AgentID == userId ||
                BDC.User.Where(p => p.SiteID == SiteInfo.ID && p.ID == userId).Select(p => p.AgentID).Contains(t.ID)
                )).Select(t => t.ID).ToArray();
        }

        /// <summary>
        /// 获取用户的配额数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<QuotaSetting> GetUserQuotaList(int userId)
        {
            List<QuotaSetting> list = SiteAgent.Instance().GetQuotaSettingList();
            List<UserQuota> userList = BDC.UserQuota.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).ToList();
            list.ForEach(t =>
            {
                UserQuota userQuota = userList.Where(p => p.ID == t.ID).FirstOrDefault();
                if (userQuota != null) t.Number = userQuota.Number;
            });

            foreach (var member in BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.AgentID == userId).GroupBy(t => t.Rebate).Select(t => new
            {
                Rebate = t.Key,
                Count = t.Count()
            }))
            {
                QuotaSetting setting = list.Where(p => p.MinRebate <= member.Rebate && p.MaxRebate >= member.Rebate).FirstOrDefault();
                if (setting != null)
                {
                    setting.Count = member.Count;
                }
            }
            return list;
        }

        /// <summary>
        /// 修改单个代理的配额数量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public bool UpdateUserQuotaNumber(int userId, int quotaId, int number)
        {
            UserQuota quota = BDC.UserQuota.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.ID == quotaId).FirstOrDefault();
            if (quota == null)
            {
                return new UserQuota()
                {
                    SiteID = SiteInfo.ID,
                    ID = quotaId,
                    UserID = userId,
                    Number = number
                }.Add();
            }
            quota.Number = number;
            return quota.Update(null, t => t.Number) == 1;
        }

        /// <summary>
        /// 获取用户的直属上级（如果没有上级返回0）
        /// 2016-9-23 修改为支持非web程序
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetAgentID(int userId)
        {
            int siteId;
            if (SiteInfo == null)
            {
                using (DbExecutor db = NewExecutor())
                {
                    siteId = this.GetSiteID(userId, db);
                }
            }
            else
            {
                siteId = SiteInfo.ID;
            }
            return BDC.User.Where(t => t.SiteID == siteId && t.ID == userId).Select(t => t.AgentID).FirstOrDefault();
        }


        /// <summary>
        /// 获取用户所属的团队的用户ID（如果没有上级则是自己）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetTeamID(int userId)
        {
            int? teamId = BDC.UserDepth.Where(t => t.SiteID == SiteInfo.ID && t.ChildID == userId).OrderByDescending(t => t.Depth).Select(t => (int?)t.UserID).FirstOrDefault();
            return teamId == null ? userId : teamId.Value;
        }

        #endregion

        #region ==============  管理员相关  ===============


        /// <summary>
        /// 获取用户的备注信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserRemark> GetUserRemarkList(int userId)
        {
            return BDC.UserRemark.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).OrderByDescending(t => t.ID).ToList();
        }

        /// <summary>
        /// 保存备注信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool SaveRemarkInfo(int userId, string content)
        {
            if (userId == 0 || string.IsNullOrEmpty(this.GetUserName(userId)))
            {
                base.Message("用户错误");
                return false;
            }
            if (string.IsNullOrEmpty(content))
            {
                base.Message("请输入备注内容");
                return false;
            }
            return new UserRemark()
            {
                AdminID = AdminInfo.ID,
                Content = content,
                CreateAt = DateTime.Now,
                SiteID = SiteInfo.ID,
                UserID = userId
            }.Add();
        }

        #endregion

        #region ==============  提现银行卡相关  =============

        /// <summary>
        /// 提现帐号
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetUserAccountName(int userId)
        {
            User user = this.GetUserInfoByCache(userId);
            if (user != null) return user.AccountName;
            return BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.ID == userId).Select(t => t.AccountName).FirstOrDefault();
        }

        /// <summary>
        /// 获取用户的提现银行卡列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<BankAccount> GetBankAccountList(int userId)
        {
            return BDC.BankAccount.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).ToList();
        }

        /// <summary>
        /// 获取提现银行信息
        /// </summary>
        /// <param name="bankId"></param>
        /// <returns></returns>
        public BankAccount GetBankAccountInfo(int bankId)
        {
            return BDC.BankAccount.Where(t => t.SiteID == SiteInfo.ID && t.ID == bankId).FirstOrDefault();
        }

        /// <summary>
        /// 添加一个提现银行卡号
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="accountName">提现名字</param>
        /// <param name="account">卡号</param>
        /// <param name="type">银行类型</param>
        /// <returns></returns>
        public bool AddUserAccount(int userId, string accountName, string account, BankType type, string payPassword, string bank = null)
        {
            if (!this.CheckLogin(userId)) return false;

            if (!this.CheckPayPassword(userId, payPassword))
            {
                return false;
            }

            bool isNew = string.IsNullOrEmpty(UserInfo.AccountName);

            if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(UserInfo.AccountName))
            {
                base.Message("不能修改银行卡名字");
                return false;
            }
            if (!string.IsNullOrEmpty(UserInfo.AccountName)) accountName = UserInfo.AccountName;
            if (string.IsNullOrEmpty(accountName))
            {
                base.Message("请输入银行卡姓名");
                return false;
            }

            switch (type)
            {
                case BankType.Alipay:
                    if (!WebAgent.IsMobile(account) && !WebAgent.IsEmail(account))
                    {
                        base.Message("支付宝账号错误");
                        return false;
                    }
                    break;
                case BankType.Wechat:
                    if (string.IsNullOrEmpty(account))
                    {
                        base.Message("微信帐号错误");
                        return false;
                    }
                    break;
                case BankType.BANK:
                    if (string.IsNullOrEmpty(bank))
                    {
                        base.Message("请输入银行名称");
                        return false;
                    }
                    break;
                default:
                    if (!WebAgent.IsBankCard(account))
                    {
                        base.Message("银行帐号错误");
                        return false;
                    }
                    if ((int)type == 0)
                    {
                        type = WebAgent.GetBankCard(account).ToEnum<BankType>();
                    }
                    if (WebAgent.GetBankCard(account) != type.ToString())
                    {
                        base.Message("银行帐号不属于“{0}”", type.GetDescription());
                        return false;
                    }
                    break;
            }

            if (!SiteInfo.Setting.WithdrawBankList.Contains(type))
            {
                base.Message("系统不支持银行卡类型：{0}", type.GetDescription());
                return false;
            }

            int cardCount = this.GetBankAccountList(userId).Count;
            if (cardCount >= SiteInfo.Setting.MaxCard)
            {
                base.Message("系统最多允许绑定{0}个账户，您已绑定{1}个", SiteInfo.Setting.MaxCard, cardCount);
                return false;
            }

            if (BDC.BankAccount.Where(t => t.SiteID == SiteInfo.ID && t.Account == account).Count() != 0)
            {
                base.Message("提现帐号“{0}”已被其他会员绑定", account);
                return false;
            }

            if (string.IsNullOrEmpty(UserInfo.AccountName))
            {
                if (SiteInfo.Setting.SameAccountName != 0 && BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.AccountName == accountName).Count() >= SiteInfo.Setting.SameAccountName)
                {
                    base.Message("姓名“{0}”已被超过{1}个会员绑定", accountName, SiteInfo.Setting.SameAccountName);
                    return false;
                }
            }

            if (string.IsNullOrEmpty(UserInfo.AccountName))
            {
                UserInfo.AccountName = accountName.Trim();
                if (!this.UpdateUserInfo(UserInfo, t => t.AccountName)) return false;
            }
            BankAccount bankAccount = new BankAccount()
            {
                SiteID = SiteInfo.ID,
                CreateAt = DateTime.Now,
                Account = account,
                UserID = userId,
                Type = type,
                Bank = string.IsNullOrEmpty(bank) ? type.GetDescription() : bank
            };

            if (bankAccount.Add())
            {
                this.SaveUserInfoLog(userId, UserInfoLog.UserInfoLogType.BankAccount, bankAccount.ToString());

                // 第一次绑定银行卡
                if (isNew)
                {
                    SiteAgent.Instance()._bankAccount(userId);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除绑定的银行卡账户
        /// </summary>
        /// <param name="bankId"></param>
        /// <returns></returns>
        public bool DeleteBankAccount(int bankId)
        {
            if (AdminInfo == null)
            {
                base.Message("没有权限");
                return false;
            }

            BankAccount bank = this.GetBankAccountInfo(bankId);
            if (bank == null)
            {
                base.Message("编号错误");
                return false;
            }

            if (bank.Delete() != 0)
            {
                AdminInfo.Log(AdminLog.LogType.User, "删除用户{0}绑定的银行卡。{1}", this.GetUserName(bank.UserID), bank.ToString());
                return true;
            }

            return false;
        }

        #endregion

        #region ============ 分组有关 =============

        /// <summary>
        /// 获取所有的分组（按排序从大到小）
        /// </summary>
        /// <returns></returns>
        public List<UserGroup> GetGroupList(int siteId = 0)
        {
            if (siteId == 0) siteId = SiteInfo.ID;
            return BDC.UserGroup.Where(t => t.SiteID == SiteInfo.ID).OrderByDescending(t => t.Sort).ToList();
        }

        /// <summary>
        /// 获取分组信息（为0返回默认值）
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public UserGroup GetGroupInfo(int groupId)
        {
            if (groupId == 0) return new UserGroup();
            return BDC.UserGroup.Where(t => t.SiteID == SiteInfo.ID && t.ID == groupId).FirstOrDefault();
        }

        /// <summary>
        /// 保存分组
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool SaveUserGroupInfo(UserGroup group)
        {
            if (string.IsNullOrEmpty(group.Name))
            {
                base.Message("请输入分组名");
                return false;
            }
            group.SiteID = SiteInfo.ID;
            group.CreateAt = DateTime.Now;

            UserGroup defaultGroup = BDC.UserGroup.Where(t => t.SiteID == SiteInfo.ID && t.IsDefault).FirstOrDefault();
            if (defaultGroup == null || defaultGroup.ID == group.ID)
            {
                group.IsDefault = true;
            }

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                if (defaultGroup != null && defaultGroup.ID != group.ID && group.IsDefault)
                {
                    defaultGroup.IsDefault = false;
                    defaultGroup.Update(db, t => t.IsDefault);
                }

                if (group.ID == 0)
                {
                    group.Add(db);
                }
                else
                {
                    group.Update(db);
                }

                db.Commit();
            }

            SiteInfo.UserGroup = null;
            return true;
        }

        /// <summary>
        /// 修改用户所在的分组
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public bool UpdateUserGroup(int userId, int groupId)
        {
            if (!SiteInfo.UserGroup.ContainsKey(groupId))
            {
                base.Message("分组编号错误");
                return false;
            }

            User user = this.GetUserInfo(userId);
            if (user == null)
            {
                base.Message("编号错误");
                return false;
            }

            user.GroupID = groupId;
            user.Update(null, t => t.GroupID);

            AdminInfo.Log(AdminLog.LogType.User, "修改用户{0}的分组为{1}({2})", user.UserName, groupId, SiteInfo.UserGroup[groupId].Name);
            return true;
        }

        /// <summary>
        /// 自动修改用户的分组
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public void UpdateUserGroup(int userId)
        {
            int siteId = this.GetSiteID(userId);
            try
            {
                List<UserGroup> groups = this.GetGroupList(siteId);
                int groupId = BDC.User.Where(t => t.SiteID == siteId && t.ID == userId).Select(t => t.GroupID).FirstOrDefault();
                if (groups.Count(t => t.ConditionID != 0 && t.ID != groupId) == 0) return;
                UserGroup group = groups.Where(t => t.ID == groupId || (groupId == 0 && t.IsDefault)).FirstOrDefault();
                // 如果当前所在分组是手动分组
                if (group == null || group.Sort < 0) return;
                int newGroupId = 0;
                Dictionary<int, string> condition = BDC.GroupCondition.ToDictionary(t => t.ID, t => t.SQL);
                using (DbExecutor db = NewExecutor())
                {
                    foreach (UserGroup g in groups.Where(t => t.Sort >= 0 && t.ConditionID != 0))
                    {
                        if (!condition.ContainsKey(g.ConditionID)) continue;
                        string sql = string.Format("SELECT 0 WHERE {0}", condition[g.ConditionID]);
                        object result = db.ExecuteScalar(CommandType.Text, sql,
                            NewParam("@SiteID", SiteInfo.ID),
                            NewParam("@UserID", userId));
                        if (result != null && result != DBNull.Value)
                        {
                            newGroupId = g.ID;
                            break;
                        }
                    }
                    if (newGroupId != 0 && newGroupId != groupId)
                    {
                        db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET GroupID = @GroupID WHERE SiteID = @SiteID AND UserID = @UserID",
                            NewParam("@SiteID", siteId),
                            NewParam("@UserID", userId),
                            NewParam("@GroupID", newGroupId));
                    }
                }
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(siteId, ex, "用户分组变更失败");
            }
        }

        #endregion

        /// <summary>
        /// IP的缓存库
        /// </summary>
        private Dictionary<string, string> _ip = new Dictionary<string, string>();
        public string GetIPAddress(string ip)
        {
            lock (_ip)
            {
                if (_ip.ContainsKey(ip)) return _ip[ip];
                string ipAddress = IPAgent.GetAddress(ip);
                _ip.Add(ip, ipAddress);
                return ipAddress;
            }
        }

        /// <summary>
        /// 获取用户最新的一条备注信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetRemarkInfo(int userId)
        {
            UserRemark remark = BDC.UserRemark.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).OrderByDescending(t => t.CreateAt).FirstOrDefault();
            return remark == null ? "" : remark.ToString();
        }

        /// <summary>
        /// 上传图片至图片服务器
        /// </summary>
        /// <param name="file"></param>
        /// <param name="type">face | upload</param>
        /// <returns></returns>
        public string UploadImage(HttpPostedFile file, string type)
        {
            if (file == null)
            {
                base.Message("没有选择要上传的文件");
                return null;
            }
            string fileType = file.ContentType;
            Regex regex = new Regex(@"^image/(?<Type>jpg|gif|png|jpeg)$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(fileType))
            {
                fileType = "image/jpg";
            }
            string ext = regex.Match(fileType).Groups["Type"].Value.ToLower();

            BinaryReader b = new BinaryReader(file.InputStream);
            byte[] data = b.ReadBytes((int)file.InputStream.Length);

            return this.UploadImage(data, type, ext);
        }

        /// <summary>
        /// 上传二进制图片
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        private string UploadImage(byte[] data, string type, string ext)
        {
            string url = SysSetting.GetSetting().imgServer + "/imageupload.ashx?type=" + type + "&ext=" + ext;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    string result = Encoding.UTF8.GetString(wc.UploadData(url, "POST", data));
                    if (string.IsNullOrEmpty(result))
                    {
                        base.Message("上传失败");
                        return null;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    base.Message(ex.Message);
                    StreamReader reader = new StreamReader(((WebException)ex).Response.GetResponseStream(), Encoding.UTF8);
                    if (reader != null)
                    {
                        base.Message(reader.ReadToEnd());
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// 下载远程图片并且上传
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="type"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        private string UploadImage(string uri, string type, string ext)
        {
            if (string.IsNullOrEmpty(uri)) return null;
            byte[] data = SP.Studio.Net.NetAgent.DownloadFile(uri);
            return this.UploadImage(data, type, ext);
        }

        /// <summary>
        /// 用于缓存用户所属站点ID，非web程序专用
        /// </summary>
        private Dictionary<int, int> userSiteID = new Dictionary<int, int>();
        /// <summary>
        /// 根据用户ID获取当前站点的ID（适用于非web程序）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetSiteID(int userId, DbExecutor db)
        {
            if (userId == 0) return 0;
            try
            {
                if (SiteInfo != null) return SiteInfo.ID;
                lock (userSiteID)
                {
                    if (userSiteID.ContainsKey(userId)) return userSiteID[userId];
                    bool isNewExecutor = db == null;
                    try
                    {
                        if (isNewExecutor) db = NewExecutor();
                        int siteId = (int)db.ExecuteScalar(CommandType.Text, "SELECT SiteID FROM Users WHERE UserID = @UserID",
                            NewParam("@UserID", userId));
                        userSiteID.Add(userId, siteId);
                        return siteId;
                    }
                    finally
                    {
                        if (isNewExecutor) db.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 上下级转账
        /// </summary>
        /// <param name="userId">当前用户</param>
        /// <param name="sourceId">目标用户</param>
        /// <param name="money">转账金额</param>
        /// <param name="payPassword">资金密码</param>
        /// <param name="transferDepth">可往下级转账的层数（如果为0则检查是否开通转账权限）</param>
        /// <returns></returns>
        public bool SaveTransferInfo(int userId, int sourceId, decimal money, string payPassword, int transferDepth = 0)
        {
            if (userId == sourceId)
            {
                base.Message("编号错误");
                return false;
            }
            if (!this.CheckLogin(userId))
            {
                base.Message("请先登录");
                return false;
            }
            if (money <= decimal.Zero)
            {
                base.Message("金额错误");
                return false;
            }

            if (!this.CheckPayPassword(userId, payPassword))
            {
                return false;
            }


            User user = this.GetUserInfo(userId);
            User source = this.GetUserInfo(sourceId);

            if (user == null || source == null)
            {
                base.Message("参数错误");
                return false;
            }

            UserDepth userDepth = BDC.UserDepth.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.ChildID == sourceId).FirstOrDefault();

            if (source.AgentID != user.ID && user.ID != user.AgentID && transferDepth == 0)
            {
                base.Message("该用户不是您的直属上下级");
                return false;
            }
            else if (userDepth != null)
            {
                if (transferDepth == 0)
                {
                    if (!user.Function.HasFlag(User.FunctionType.TransferDown))
                    {
                        base.Message("您没有向下级转账的权限");
                        return false;
                    }

                    if (user.Lock.HasFlag(User.LockStatus.Contract))
                    {
                        base.Message("您有契约转账尚未完成");
                        return false;
                    }
                }
                else
                {
                    if (userDepth.Depth > transferDepth)
                    {
                        base.Message("转账层级超过{0}层", transferDepth);
                        return false;
                    }
                }
            }
            else if (source.ID == user.AgentID)
            {
                if (!user.Function.HasFlag(User.FunctionType.TransferUp))
                {
                    base.Message("您没有向上级转账的权限");
                    return false;
                }
            }
            else
            {
                base.Message("用户层级错误");
                return false;
            }

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                long id = WebAgent.GetTimeStamp() % int.MaxValue;
                //#1 减去资金
                if (!this.AddMoneyLog(db, userId, money * -1, MoneyLog.MoneyType.Transfer, (int)id, string.Format("转账给{0}", source.UserName)))
                {
                    db.Rollback();
                    return false;
                }

                //#2 增加资金
                if (!this.AddMoneyLog(db, sourceId, money, MoneyLog.MoneyType.TransferIn, (int)id, string.Format("{0}转账收入", user.UserName)))
                {
                    db.Rollback();
                    return false;
                }

                db.Commit();
                return true;
            }
        }

        /// <summary>
        /// 根据用户ID获取站点ID（适用于非web程序）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetSiteID(int userId)
        {
            if (userId == 0) return 0;
            using (DbExecutor db = NewExecutor())
            {
                return this.GetSiteID(userId, db);
            }
        }
    }
}
