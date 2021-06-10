using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.Common;
using SP.Studio.Data;
using BW.Common.Sites;
using BW.Common.Users;
using BW.Common.Admins;

using BW.Framework;

namespace BW.Agent
{
    /// <summary>
    /// 所有逻辑类的基类
    /// </summary>
    /// <typeparam name="T">代理类</typeparam>
    public abstract class AgentBase<T> : DbAgent where T : class, new()
    {
        public AgentBase(string connection) : base(connection, DatabaseType.SqlServer, DataConnectionMode.Instance) { }

        /// <summary>
        /// 全局的连接数据库对象
        /// </summary>
        public AgentBase() : base(SysSetting.GetSetting().DbConnection, DatabaseType.SqlServer, DataConnectionMode.Instance) { }

        protected BetDataContext BDC
        {
            get
            {
                return DbSetting.GetSetting().CreateDataContext<BetDataContext>();
            }
        }


        /// <summary>
        /// 当前站点对象
        /// </summary>
        protected virtual Site SiteInfo
        {
            get
            {
                if (HttpContext.Current == null) return null;
                return (Site)HttpContext.Current.Items[BetModule.SITEINFO];
            }
        }

        /// <summary>
        /// 当前登录的用户
        /// </summary>
        protected virtual User UserInfo
        {
            get
            {
                if (HttpContext.Current == null) return null;
                return (User)HttpContext.Current.Items[BetModule.USERINFO];
            }
        }

        /// <summary>
        /// 当前登录的管理员
        /// </summary>
        protected virtual Admin AdminInfo
        {
            get
            {
                if (HttpContext.Current == null) return null;
                return AdminAgent.Instance().GetAdminInfo();
            }
        }

        /// <summary>
        /// 检查当前登录用户是否是可操作用户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected virtual bool CheckLogin(int userId)
        {
            if (UserInfo == null)
            {
                this.Message("请先登录");
                return false;
            }
            if (UserInfo.ID != userId)
            {
                this.Message("您不能操作他人账户");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断
        /// </summary>
        /// <param name="db"></param>
        /// <param name="isNewExecutor"></param>
        /// <returns></returns>
        protected DbExecutor GetNewExecutor(DbExecutor db, out bool isNewExecutor)
        {
            isNewExecutor = false;
            if (db == null)
            {
                db = NewExecutor();
                isNewExecutor = true;
            }
            return db;
        }



        private static T _instance;
        /// <summary>
        /// 返回单例对象
        /// </summary>
        /// <returns></returns>
        public static T Instance()
        {
            if (_instance == null)
            {
                _instance = new T();
            }

            return _instance;
        }
    }
}
