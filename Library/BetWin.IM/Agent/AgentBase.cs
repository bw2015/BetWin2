using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using SP.Studio.Data;
using BW.IM.Framework;
using BW.IM.Common;

namespace BW.IM.Agent
{
    public abstract class AgentBase<T> : DbAgent where T : class,new()
    {
        public AgentBase() : base(SysSetting.GetSetting().DbConnection, DatabaseType.SqlServer, DataConnectionMode.Instance) { }

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

        /// <summary>
        /// 当前登录的用户
        /// </summary>
        protected virtual User UserInfo
        {
            get
            {
                return (User)context.Items[Utils.USERINFO];
            }
        }
    }
}
