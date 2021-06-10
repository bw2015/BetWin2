using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.Agent;
using BW.Common.Users;
using BW.Common.Lottery;

namespace BW.Common.Sites
{
    /// <summary>
    /// 站点会员相关
    /// </summary>
    partial class Site
    {
        private Dictionary<int, UserGroup> _userGroup;

        /// <summary>
        /// 当前站点配置的用户分组
        /// </summary>
        public Dictionary<int, UserGroup> UserGroup
        {
            get
            {
                if (_userGroup == null)
                {
                    List<UserGroup> list = UserAgent.Instance().GetGroupList();
                    _userGroup = list.ToDictionary(t => t.ID, t => t);
                    _userGroup.Add(0,
                        list.Find(t => t.IsDefault) ?? new UserGroup() { Name = "默认分组" });
                }
                return _userGroup;
            }
            internal set
            {
                this._userGroup = null;
            }
        }
    }
}
