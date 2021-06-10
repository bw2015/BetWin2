using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Web.Caching;
using System.Xml.Linq;
using System.Resources;
using BW.Framework;

using BW.Common.Admins;
using BW.Common.Sites;
using BW.Common.Users;
using BW.Resources;
using BW.Common.Permission;

using SP.Studio.Data;
using SP.Studio.Data.Linq;
using SP.Studio.Security;
using SP.Studio.Web;
using SP.Studio.Xml;

namespace BW.Agent
{
    /// <summary>
    /// 管理员操作基类
    /// </summary>
    public sealed class AdminAgent : AgentBase<AdminAgent>
    {
        /// <summary>
        /// 管理员登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="code">谷歌验证码</param>
        /// <returns></returns>
        public bool Login(string userName, string password, string code)
        {
            if (string.IsNullOrEmpty(userName))
            {
                base.Message("用户名为空");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                base.Message("密码为空");
                return false;
            }

            password = MD5.toMD5(password);

            Admin admin = BDC.Admin.Where(t => t.SiteID == SiteInfo.ID && t.AdminName == userName && t.Password == password).FirstOrDefault();
            if (admin == null)
            {
                base.Message("用户名或者密码错误");
                return false;
            }

            if (admin.SecretKey != Guid.Empty && !new GoogleAuthenticator().ValidateTwoFactorPIN(admin.SecretKey.ToString("N"), code))
            {
                base.Message("验证码错误");
                return false;
            }

            if (admin.Status != Admin.AdminStatus.Normal)
            {
                base.Message("当前的状态是“{0}”,无法登录");
                return false;
            }

            Guid session = Guid.NewGuid();
            this.SaveAdminSession(admin.ID, session);
            this.context.Response.Cookies[BetModule.AMDINKEY].Value = session.ToString("N");

            admin.LoginAt = DateTime.Now;
            admin.LoginIP = IPAgent.IP;
            admin.Update(null, t => t.LoginAt, t => t.LoginIP);

            this.AddLog(admin.ID, AdminLog.LogType.Login, "管理员登录");
            return true;
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public void Logout()
        {
            if (AdminInfo != null)
            {
                this.SetOnlineStatus(AdminInfo.ID, false);
                AdminInfo.Log(AdminLog.LogType.Login, "退出登录");
            }
            HttpContext.Current.Response.Cookies[BetModule.AMDINKEY].Expires = DateTime.MinValue;
        }
        /// <summary>
        /// 获取管理员资料
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public Admin GetAdminInfo(int adminId)
        {
            return BDC.Admin.Where(t => t.SiteID == SiteInfo.ID && t.ID == adminId).FirstOrDefault();
        }

        /// <summary>
        /// 新建或者修改管理员资料
        /// </summary>
        /// <param name="admin"></param>
        /// <returns></returns>
        public bool SaveAdminInfo(Admin admin)
        {
            if (string.IsNullOrEmpty(admin.AdminName))
            {
                base.Message("请输入管理员用户名");
                return false;
            }
            if (!this.GetAdminGroupList().Select(t => t.ID).Contains(admin.GroupID))
            {
                base.Message("角色选择错误");
                return false;
            }
            if (BDC.Admin.Where(t => t.SiteID == SiteInfo.ID && t.AdminName == admin.AdminName && t.ID != admin.ID).Count() != 0)
            {
                base.Message("该用户名已经存在");
                return false;
            }
            if (admin.ID == 0)
            {
                if (string.IsNullOrEmpty(admin.Password))
                {
                    base.Message("请输入密码");
                    return false;
                }
                admin.SiteID = SiteInfo.ID;
                admin.CreateAt = DateTime.Now;
                admin.Password = MD5.toMD5(admin.Password);
                this.AddLog(AdminInfo.ID, AdminLog.LogType.Site, "新建管理员:{0}", admin.AdminName);
                return admin.Add();
            }
            else
            {
                if (!string.IsNullOrEmpty(admin.Password))
                {
                    admin.Password = MD5.toMD5(admin.Password);
                }
                else
                {
                    admin.Password = BDC.Admin.Where(t => t.ID == admin.ID).Select(t => t.Password).FirstOrDefault();
                }

                this.AddLog(AdminInfo.ID, AdminLog.LogType.Site, "修改管理员信息:{0}", admin.AdminName);
                return admin.Update(null, t => t.AdminName, t => t.Password, t => t.NickName, t => t.GroupID, t => t.Status) != 0;
            }
        }

        public bool CleanAdminSecretKey(int adminId)
        {
            Admin admin = this.GetAdminInfo(adminId);
            if (admin == null)
            {
                base.Message("编号错误");
                return false;
            }
            admin.SecretKey = Guid.Empty;
            if (admin.Update(null, t => t.SecretKey) == 1)
            {
                AdminInfo.Log(AdminLog.LogType.Info, "清除{0}的谷歌验证码", admin.AdminName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取管理员用户名
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public string GetAdminName(int adminId)
        {
            return BDC.Admin.Where(t => t.SiteID == SiteInfo.ID && t.ID == adminId).Select(t => new { t.NickName, t.AdminName }).ToList().Select(t => string.IsNullOrEmpty(t.NickName) ? t.AdminName : t.NickName).FirstOrDefault();
        }

        /// <summary>
        /// 获取管理员头像
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public string GetAdminFace(int adminId)
        {
            string face = BDC.Admin.Where(t => t.SiteID == SiteInfo.ID && t.ID == adminId).Select(t => t.Face).FirstOrDefault();
            if (string.IsNullOrEmpty(face)) face = "/images/admin.jpg";
            return SysSetting.GetSetting().GetImage(face);
        }

        public bool UpdateAdminInfo(int adminID, string nickName, string password)
        {
            if (!string.IsNullOrEmpty(password) && (password.Length < 6 || password.Length > 16))
            {
                base.Message("密码长度应在6～16位之间");
                return false;
            }

            if (AdminInfo.ID != adminID)
            {
                base.Message("您不能修改别人的账户");
                return false;
            }

            AdminInfo.NickName = nickName;
            if (string.IsNullOrEmpty(password))
            {
                AdminInfo.Log(AdminLog.LogType.Info, string.Format("修改昵称为：{0}", nickName));
                return AdminInfo.Update(null, t => t.NickName) > 0;
            }
            else
            {
                AdminInfo.Password = MD5.toMD5(password);
                AdminInfo.Log(AdminLog.LogType.Info, string.Format("修改昵称为：{0}，修改密码为：{1}", nickName, WebAgent.HiddenName(password)));
                return AdminInfo.Update(null, t => t.NickName, t => t.Password) > 0;
            }
        }

        /// <summary>
        /// 设定谷歌验证码
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool UpdateAdminInfo(int adminId, Guid code)
        {
            if (code == Guid.Empty)
            {
                base.Message("密钥为空");
                return false;
            }
            Admin admin = this.GetAdminInfo(adminId);
            if (admin == null)
            {
                base.Message("ID错误");
                return false;
            }
            if (admin.SecretKey != Guid.Empty)
            {
                base.Message("已经设定过谷歌验证码");
                return false;
            }
            admin.SecretKey = code;
            if (admin.Update(null, t => t.SecretKey) == 1)
            {
                AdminInfo.Log(AdminLog.LogType.Info, "设定谷歌验证码");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool UpdateFace(int adminId, HttpPostedFile file)
        {
            if (adminId == 0)
            {
                base.Message("当前账户信息获取失败");
                return false;
            }

            Admin admin = this.GetAdminInfo(adminId);
            if (admin == null)
            {
                base.Message("当前账户信息获取失败");
                return false;
            }
            admin.Face = UserAgent.Instance().UploadImage(file, "face");
            if (string.IsNullOrEmpty(admin.Face)) return false;

            if (admin.Update(null, t => t.Face) != 0)
            {
                this.RemoveAdminCache(adminId);
                return true;
            }
            return false;
        }

        private Dictionary<Guid, int> _adminSession = new Dictionary<Guid, int>();

        /// <summary>
        /// 当前登录的管理员
        /// </summary>
        /// <returns></returns>
        public Admin GetAdminInfo()
        {
            string adminKey = WebAgent.QC(BetModule.AMDINKEY);
            Guid session;
            if (string.IsNullOrEmpty(adminKey) || !Guid.TryParse(adminKey, out session)) return null;
            int? adminId = null;
            lock (_adminSession)
            {
                if (_adminSession.ContainsKey(session))
                {
                    adminId = _adminSession[session];
                }
                else
                {
                    adminId = BDC.AdminSession.Where(t => t.Session == session && t.SiteID == SiteInfo.ID).Select(t => (int?)t.AdminID).FirstOrDefault();
                    if (adminId == null)
                    {
                        //this.context.Response.Cookies.Remove(BetModule.AMDINKEY);
                        return null;
                    }
                    this._adminSession.Add(session, adminId.Value);
                }
            }

            string key = string.Concat(BetModule.AMDINKEY, "_", adminId.Value);
            Admin admin = (Admin)HttpRuntime.Cache[key];
            if (admin != null) return admin;

            admin = this.GetAdminInfo(adminId.Value);
            if (admin == null) return null;

            HttpRuntime.Cache.Insert(key, admin, BetModule.SiteCacheDependency, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));
            return admin;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="adminId"></param>
        public void RemoveAdminCache(int adminId)
        {
            string key = string.Concat(BetModule.AMDINKEY, "_", adminId);
            if (HttpRuntime.Cache[key] != null)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }

        /// <summary>
        /// 获取登录管理员的当前session
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public Guid GetAdminSession(int adminId)
        {
            Guid? id = _adminSession.Where(t => t.Value == adminId).Select(t => (Guid?)t.Key).FirstOrDefault();
            if (id == null)
            {
                id = BDC.AdminSession.Where(t => t.SiteID == SiteInfo.ID && t.AdminID == adminId).Select(t => (Guid?)t.Session).FirstOrDefault();
            }
            return id == null ? Guid.Empty : id.Value;
        }

        /// <summary>
        /// 根据在线session获取管理员ID
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public int GetAdminID(Guid session)
        {
            int? adminId = BDC.AdminSession.Where(t => t.Session == session).Select(t => (int?)t.AdminID).FirstOrDefault();
            return adminId == null ? 0 : adminId.Value;
        }

        public bool DeleteAdmin(int adminId)
        {
            Admin admin = AdminAgent.Instance().GetAdminInfo(adminId);
            if (admin == null)
            {
                base.Message("编号错误");
                return false;
            }
            if (admin.GroupID == 0)
            {
                base.Message("不能删除超级管理员");
                return false;
            }
            if (BDC.AdminLog.Where(t => t.SiteID == SiteInfo.ID && t.AdminID == adminId).Count() != 0)
            {
                base.Message("该管理员已经有操作记录，无法删除，如需禁用请修改状态为停止");
                return false;
            }

            return admin.Delete() != 0;
        }

        /// <summary>
        /// 保存管理员的在线状态
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="session"></param>
        private void SaveAdminSession(int adminId, Guid session)
        {
            AdminSession adminSession = new AdminSession()
            {
                AdminID = adminId,
                SiteID = SiteInfo.ID,
                Session = session,
                UpdateAt = DateTime.Now
            };

            foreach (Guid key in this._adminSession.Where(t => t.Value == adminId).Select(t => t.Key).ToArray())
            {
                this._adminSession.Remove(key);
            }

            if (adminSession.Exists(null, t => t.SiteID, t => t.AdminID))
            {
                BDC.AdminSession.Update(null, adminSession, t => t.AdminID == adminSession.AdminID && t.SiteID == adminSession.SiteID, t => t.Session, t => t.UpdateAt);
            }
            else
            {
                adminSession.Add();
            }
        }


        /// <summary>
        /// 管理员操作日志
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="args"></param>
        internal void AddLog(int adminId, AdminLog.LogType type, string content, params object[] args)
        {
            new AdminLog()
            {
                AdminID = adminId,
                SiteID = SiteInfo.ID,
                Content = WebAgent.LeftString(args.Length == 0 ? content : string.Format(content, args), 1000),
                CreateAt = DateTime.Now,
                ExtendXML = WebAgent.GetPostLog(),
                IP = IPAgent.IP,
                Type = type
            }.Add();
        }

        /// <summary>
        /// 管理员权限的列表（包括站点的管理扩展）
        /// </summary>
        /// <returns></returns>
        public XElement GetAdminPermission()
        {
            ResourceManager rm = new ResourceManager(typeof(Res));
            string xml = (string)rm.GetObject("AdminPermission");
            if (string.IsNullOrEmpty(xml)) return null;
            XElement root = XElement.Parse(xml);
            if (!string.IsNullOrEmpty(SiteInfo.ConfigString))
            {
                XElement extendXml = XElement.Parse(SiteInfo.ConfigString);
                foreach (XElement menu in extendXml.Elements("menu"))
                {
                    root.Add(menu);
                }
            }
            return root;
        }

        public List<AdminMenu> GetAdminPermission(int adminId, XElement root = null)
        {
            if (AdminInfo == null || adminId != AdminInfo.ID)
            {
                base.Message("当前管理员未登录");
                return null;
            }
            if (root == null) root = this.GetAdminPermission();
            List<AdminMenu> list = new List<AdminMenu>();
            foreach (XElement item in root.Elements().Where(t => t.Name == "menu" && AdminInfo.HasPermission(t.GetAttributeValue("ID", string.Empty))))
            {
                AdminMenu menu = new AdminMenu(item, AdminInfo, this.GetAdminPermission(adminId, item));
                list.Add(menu);
            }
            return list;
        }

        /// <summary>
        /// 管理员代客充值
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="payId"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool AddRechargeOrder(int userId, decimal money, int payId, string description)
        {
            long orderId = UserAgent.Instance().CreateRechargeOrder(userId, payId, money, description);
            if (orderId != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Money, "代客充值，用户名：{0}，金额：{1}，充值订单号：{2}", UserAgent.Instance().GetUserName(userId), money.ToString("n"), orderId);
                return true;
            }
            return false;
        }


        /// <summary>
        /// 获取系统中的管理员列表
        /// </summary>
        /// <param name="isAll"></param>
        /// <returns></returns>
        public List<Admin> GetAdminList()
        {
            return BDC.Admin.Where(t => t.SiteID == SiteInfo.ID).OrderByDescending(t => t.ID).ToList();
        }

        /// <summary>
        /// 获取客服列表
        /// </summary>
        /// <returns></returns>
        public List<Admin> GetServiceList()
        {
            IEnumerable<int> groupList = this.GetAdminGroupList().Where(t => t.PermissionList.Contains(AdminPermission.客服管理.在线客服)).Select(t => t.ID);
            return this.GetAdminList().Where(t => t.Status == Admin.AdminStatus.Normal && groupList.Contains(t.GroupID)).ToList();
        }

        /// <summary>
        /// 获取系统中的权限设定选项
        /// </summary>
        /// <returns></returns>
        public List<Permission> GetAdminPermissionList()
        {
            XElement root = this.GetAdminPermission();
            return root.Elements().Select(t => new Permission(t)).ToList();
        }

        /// <summary>
        /// 保存用户分组
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool SaveAdminGroupInfo(AdminGroup group)
        {
            if (string.IsNullOrEmpty(group.Name))
            {
                base.Message("请输入角色名称");
                return false;
            }
            group.SiteID = SiteInfo.ID;

            if (group.ID == 0)
            {
                return group.Add();
            }
            else
            {
                AdminGroup _group = this.GetAdminGroupInfo(group.ID);
                if (_group == null)
                {
                    base.Message("角色编号错误");
                    return false;
                }
                return group.Update() != 0;
            }
        }

        /// <summary>
        /// 获取系统中的角色列表
        /// </summary>
        /// <returns></returns>
        public List<AdminGroup> GetAdminGroupList()
        {
            return BDC.AdminGroup.Where(t => t.SiteID == SiteInfo.ID).ToList();
        }

        /// <summary>
        /// 获取管理员角色信息
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public AdminGroup GetAdminGroupInfo(int groupId)
        {
            return BDC.AdminGroup.Where(t => t.SiteID == SiteInfo.ID && t.ID == groupId).FirstOrDefault();
        }

        /// <summary>
        /// 设置客服上线或者下线（兼容非web程序）
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="online"></param>
        public void SetOnlineStatus(int adminId, bool online)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE site_Admin SET IsOnline = @Online WHERE AdminID = @AdminID AND IsOnline != @Online",
                    NewParam("@AdminID", adminId),
                    NewParam("@Online", online));
            }
        }

        /// <summary>
        /// 获取拥有客服权限的管理员列表（超级管理员除外）
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<int> GetServiceAdmin(int siteId)
        {
            using (DbExecutor db = NewExecutor())
            {
                string sql = string.Format(@"SELECT AdminID FROM site_Admin JOIN site_AdminGroup ON site_Admin.GroupID = site_AdminGroup.GroupID WHERE 
                        site_Admin.SiteID = @SiteID AND site_Admin.Status = @Status AND Permission LIKE '%{0}%'", AdminPermission.客服管理.在线客服);
                DataSet ds = db.GetDataSet(CommandType.Text, sql,
                    NewParam("@SiteID", siteId),
                    NewParam("@Status", Admin.AdminStatus.Normal));
                return ds.ToList<int>();
            }
        }

        /// <summary>
        /// 获取所有具有客服权限的管理员列表（超级管理员除外）
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public Dictionary<int, List<int>> GetServiceAdmin()
        {
            using (DbExecutor db = NewExecutor())
            {
                string sql = string.Format(@"SELECT site_Admin.SiteID,AdminID FROM site_Admin JOIN site_AdminGroup ON site_Admin.GroupID = site_AdminGroup.GroupID WHERE 
                        site_Admin.Status = @Status AND Permission LIKE '%{0}%'", AdminPermission.客服管理.在线客服);
                DataSet ds = db.GetDataSet(CommandType.Text, sql,
                    NewParam("@Status", Admin.AdminStatus.Normal));
                Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int siteId = (int)dr["SiteID"];
                    int adminId = (int)dr["AdminID"];

                    if (!dic.ContainsKey(siteId)) dic.Add(siteId, new List<int>());
                    dic[siteId].Add(adminId);
                }
                return dic;
            }
        }

        /// <summary>
        /// 获取当前站点具有客服权限的管理员列表
        /// </summary>
        /// <returns></returns>
        public List<Admin> GetServiceAdminList()
        {
            return BDC.Admin.Where(t => t.SiteID == SiteInfo.ID && t.GroupID != 0 && t.Status == Admin.AdminStatus.Normal).Join(
                BDC.AdminGroup.Where(t => t.SiteID == SiteInfo.ID && t.Permission.Contains(AdminPermission.客服管理.在线客服)),
                t => t.GroupID, t => t.ID, (admin, group) => admin).ToList();
        }

        /// <summary>
        /// 是否是客服帐号
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public bool IsService(int adminId)
        {
            int count = BDC.AdminGroup.Join(BDC.Admin.Where(t => t.ID == adminId), t => t.ID, t => t.GroupID, (group, admin) => group.Permission).Where(t => t.Contains(AdminPermission.客服管理.在线客服)).Count();
            return count > 0;
        }
    }
}
