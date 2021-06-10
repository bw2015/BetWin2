using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Data;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Permission.Model;

namespace SP.Studio.Permission
{
    /// <summary>
    /// 通用的权限管理模块
    /// </summary>
    public abstract class PermissionAgent<T> : DbAgentBase<T> where T : class, new()
    {
        public PermissionAgent(string dbConnection) : base(dbConnection) { }

        /// <summary>
        /// 管理员登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="key">谷歌验证码</param>
        /// <returns></returns>
        public virtual bool Login(string userName, string password, string key)
        {
            using (DbExecutor db = NewExecutor())
            {
                AdminUser admin = new AdminUser() { UserName = userName }.Info(db, t => t.UserName);
                if (admin == null) return this.FaildMessage("用户名不存在");
                if (admin.SecretKey != Guid.Empty && string.IsNullOrEmpty(key)) return this.FaildMessage("请输入验证码");
                if (admin.SecretKey != Guid.Empty && new GoogleAuthenticator().ValidateTwoFactorPIN(admin.SecretKey.ToString("N"), key)) return this.FaildMessage("验证码错误");
                if (admin.Password != MD5.toMD5(password)) return this.FaildMessage("密码错误");

                admin.LoginAt = DateTime.Now;
                admin.LoginIP = IPAgent.IP;
                admin.Update(db, t => t.LoginAt, t => t.LoginIP);

                this.SaveToken(admin);
                return true;
            }
        }


        /// <summary>
        /// 管理员登录成功，保存token的方法
        /// </summary>
        public abstract void SaveToken(AdminUser admin);

        /// <summary>
        /// 自定义实现的获取当前登录管理员帐号的方法
        /// </summary>
        /// <returns>为0表示没有登录</returns>
        public abstract int GetAdminID();

        /// <summary>
        /// 获取当前管理员权限
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public virtual string[] GetAdminPermission(int adminId)
        {
            return null;
        }

        /// <summary>
        /// 检查当前管理员是否拥有该项权限
        /// </summary>
        public virtual bool IsPermission(int adminId, string permission)
        {
            return false;
        }
    }
}
