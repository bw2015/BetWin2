using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using System.Web;

using BW.Agent;
using BW.Common;
using BW.Common.Admins;
using SP.Studio.Web;
using SP.Studio.Model;
using SP.Studio.Core;
using SP.Studio.Security;

namespace BW.Handler.admin
{
    /// <summary>
    /// 管理员账户相关
    /// </summary>
    public class account : IHandler
    {
        private void login(HttpContext context)
        {
            string userName = QF("UserName");
            string password = QF("Password");
            string code = QF("Code");

            this.ShowResult(context, AdminAgent.Instance().Login(userName, password, code), "登录成功");
        }

        /// <summary>
        /// 当前登录的管理员帐号信息
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void info(HttpContext context)
        {
            context.Response.Write(true, "管理员", new
            {
                AdminName = AdminInfo.AdminName,
                NickName = AdminInfo.NickName,
                Name = AdminInfo.Name,
                Face = AdminInfo.FaceShow,
                Password = string.Empty,
                Session = AdminAgent.Instance().GetAdminSession(AdminInfo.ID)
            });
        }

        /// <summary>
        /// 获取谷歌验证码的安全码
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void getsecretkey(HttpContext context)
        {
            Admin admin = AdminAgent.Instance().GetAdminInfo(AdminInfo.ID);
            if (admin.SecretKey != Guid.Empty)
            {
                context.Response.Write(false, "您已设定谷歌验证");
            }

            string key = Guid.NewGuid().ToString("N");
            SetupCode info = new GoogleAuthenticator().GenerateSetupCode(SiteInfo.ID.ToString(), admin.AdminName, key);
            context.Response.Write(true, "", new
            {
                SecretKey = key,
                Code = info.QrCodeSetupImageUrl
            });
        }

        /// <summary>
        /// 保存谷歌验证码
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void savesecretkey(HttpContext context)
        {
            if (AdminInfo.SecretKey != Guid.Empty)
            {
                context.Response.Write(false, "您已设定谷歌验证");
            }
            Guid key = QF("Key", Guid.Empty);
            this.ShowResult(context, AdminAgent.Instance().UpdateAdminInfo(AdminInfo.ID, key), "保存成功");
        }

        /// <summary>
        /// 修改管理员信息
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void saveinfo(HttpContext context)
        {
            string password = QF("Password");
            string password2 = QF("Password2");
            if (password != password2)
            {
                context.Response.Write(false, "两次输入的密码不相同");
            }

            this.ShowResult(context, AdminAgent.Instance().UpdateAdminInfo(AdminInfo.ID, QF("NickName"), password));
        }

        /// <summary>
        /// 保存头像
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void saveface(HttpContext context)
        {
            context.Response.ContentEncoding = Encoding.GetEncoding("GBK");
            Guid session = QF("_tb_token_", Guid.Empty);
            if (session == Guid.Empty)
            {
                context.Response.Write("{\"isSuccess\":false,\"msg\":\"Token" + QF("_tb_token_") + "验证失败\",\"erCode\":\"\"}");
                return;
            }
            int adminId = AdminAgent.Instance().GetAdminID(QF("_tb_token_", Guid.Empty));

            if (AdminAgent.Instance().UpdateFace(adminId, context.Request.Files["Filedata"]))
            {
                context.Response.Write("{\"isSuccess\":true,\"msg\":\"头像保存成功\",\"erCode\":\"\"}");
            }
            else
            {
                context.Response.Write("{\"isSuccess\":false,\"msg\":\"" + AdminAgent.Instance().Message() + "\",\"erCode\":\"\"}");
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="context"></param>
        private void logout(HttpContext context)
        {
            AdminAgent.Instance().Logout();
            context.Response.Write(true, "退出成功");
        }

        /// <summary>
        /// 管理员列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员列表.Value)]
        private void adminlist(HttpContext context)
        {

            List<Admin> list = AdminAgent.Instance().GetAdminList();
            List<AdminGroup> groupList = AdminAgent.Instance().GetAdminGroupList();
            Dictionary<int, string> group = groupList.Count == 0 ? new Dictionary<int, string>() : groupList.ToDictionary(t => t.ID, t => t.Name);
            group.Add(0, "超级管理员");
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.AdminName,
                t.Name,
                Group = group[t.GroupID],
                t.CreateAt,
                t.LoginAt,
                t.LoginIP,
                IPAddress = UserAgent.Instance().GetIPAddress(t.LoginIP),
                Status = t.Status.GetDescription(),
                IsCode = t.SecretKey != Guid.Empty
            }));
        }

        /// <summary>
        /// 获取管理员的信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员列表.Value)]
        private void admininfo(HttpContext context)
        {
            int adminId = QF("id", 0);
            Admin admin = adminId == 0 ? new Admin() : AdminAgent.Instance().GetAdminInfo(adminId);
            if (admin == null)
            {
                context.Response.Write(false, "编号错误");
            }
            if (admin.ID != 0 && admin.GroupID == 0)
            {
                context.Response.Write(false, "不能修改超级管理员资料");
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                admin.ID,
                admin.AdminName,
                admin.NickName,
                admin.GroupID,
                admin.Status
            });
        }

        /// <summary>
        /// 添加或者修改管理员资料
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员列表.Value)]
        private void saveadmininfo(HttpContext context)
        {
            int adminId = QF("id", 0);
            Admin admin = adminId == 0 ? new Admin() : AdminAgent.Instance().GetAdminInfo(adminId);
            if (admin == null)
            {
                context.Response.Write(false, "编号错误");
            }
            admin = context.Request.Form.Fill(admin);
            this.ShowResult(context, AdminAgent.Instance().SaveAdminInfo(admin));
        }

        /// <summary>
        /// 清除谷歌验证码
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员列表.Value)]
        private void cleanadminsecret(HttpContext context)
        {
            int adminId = QF("id", 0);
            this.ShowResult(context, AdminAgent.Instance().CleanAdminSecretKey(adminId), "清除成功");
        }

        /// <summary>
        /// 删除管理员
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员列表.Value)]
        private void deleteadmin(HttpContext context)
        {
            this.ShowResult(context, AdminAgent.Instance().DeleteAdmin(QF("id", 0)), "删除成功");
        }

        /// <summary>
        /// 管理员操作日志
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员日志.Value)]
        private void log(HttpContext context)
        {
            var list = BDC.AdminLog.Where(t => t.SiteID == SiteInfo.ID);

            Dictionary<int, string> admin = AdminAgent.Instance().GetAdminList().ToDictionary(t => t.ID, t => t.AdminName);

            if (!string.IsNullOrEmpty(QF("title"))) list = list.Where(t => t.Content.Contains(QF("title")));
            if (!string.IsNullOrEmpty(QF("admin")))
            {
                Dictionary<string, int> adminName = admin.ToDictionary(t => t.Value, t => t.Key);
                list = list.Where(t => t.AdminID == (adminName.ContainsKey(QF("admin")) ? adminName[QF("admin")] : 0));
            }
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Admin = admin[t.AdminID],
                t.CreateAt,
                Type = t.Type.GetDescription(),
                t.IP,
                IPAddress = UserAgent.Instance().GetIPAddress(t.IP),
                t.Content
            }));
        }

        /// <summary>
        /// 系统中的权限列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员角色.Value)]
        private void permisssion(HttpContext context)
        {
            context.Response.Write(true, "系统权限列表", string.Concat("[", string.Join(",", AdminAgent.Instance().GetAdminPermissionList().Select(t => t.ToString())), "]"));
        }

        /// <summary>
        /// 保存角色组信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员角色.Value)]
        private void savegroupinfo(HttpContext context)
        {

            AdminGroup group = context.Request.Form.Fill<AdminGroup>();
            this.ShowResult(context, AdminAgent.Instance().SaveAdminGroupInfo(group));
        }

        /// <summary>
        /// 管理员角色列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员角色.Value)]
        private void grouplist(HttpContext context)
        {

            List<AdminGroup> list = AdminAgent.Instance().GetAdminGroupList();
            List<Admin> admin = AdminAgent.Instance().GetAdminList();
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.Name,
                t.Description,
                Member = admin.Count(p => p.GroupID == t.ID)
            }));
        }

        /// <summary>
        /// 查看管理角色的信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.后台权限.管理员角色.Value)]
        private void getgroupinfo(HttpContext context)
        {

            AdminGroup group = AdminAgent.Instance().GetAdminGroupInfo(QF("id", 0)) ?? new AdminGroup();

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                group.ID,
                group.Name,
                group.Description,
                group.Permission
            });
        }


    }
}
