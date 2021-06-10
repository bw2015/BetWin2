using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

using SP.Studio.Data;

namespace Web.GateWay.App_Code
{
    /// <summary>
    /// 代理层基类
    /// </summary>
    public abstract class AgentBase : DbAgent
    {
        public AgentBase() : base(DbConnection, DatabaseType.SqlServer, DataConnectionMode.Instance) { }

        private static string DbConnection
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            }
        }
    }
}