using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Sites;

namespace BW.Agent
{
    /// <summary>
    /// 全站系统管理
    /// </summary>
    partial class SystemAgent
    {
        /// <summary>
        /// 获取所有站点列表
        /// </summary>
        /// <returns></returns>
        public List<Site> GetSiteList()
        {
            return BDC.Site.ToList();
        }
    }
}
