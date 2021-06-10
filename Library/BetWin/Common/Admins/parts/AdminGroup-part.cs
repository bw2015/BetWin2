using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Common.Admins
{
    partial class AdminGroup
    {
        private string[] _permissionList;
        public string[] PermissionList
        {
            get
            {
                if (_permissionList == null)
                {
                    _permissionList = this.Permission.Split(',');
                }
                return _permissionList;
            }
        }

        /// <summary>
        /// 角色是否拥有对应权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool HasPermission(string permission)
        {
            if (this.ID == 0) return true;
            return this.Permission.Contains(permission);
        }
    }
}
