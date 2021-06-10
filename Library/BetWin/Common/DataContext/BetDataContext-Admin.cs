using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Common.Admins;


namespace BW.Common
{
    /// <summary>
    /// 管理员对象
    /// </summary>
    partial class BetDataContext : DataContext, IDisposable
    {
        public Table<Admin> Admin
        {
            get
            {
                return this.GetTable<Admin>();
            }
        }

        /// <summary>
        /// 在线状态
        /// </summary>
        public Table<AdminSession> AdminSession
        {
            get
            {
                return this.GetTable<AdminSession>();
            }
        }

        /// <summary>
        /// 管理员角色组
        /// </summary>
        public Table<AdminGroup> AdminGroup
        {
            get
            {
                return this.GetTable<AdminGroup>();
            }
        }

        /// <summary>
        /// 管理员日志
        /// </summary>
        public Table<AdminLog> AdminLog
        {
            get
            {
                return this.GetTable<AdminLog>();
            }
        }

    }
}
