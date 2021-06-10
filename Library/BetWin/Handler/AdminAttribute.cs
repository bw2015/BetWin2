using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Handler
{
    /// <summary>
    /// 管理员权限的属性标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AdminAttribute : Attribute
    {
        /// <summary>
        /// 已经选中的
        /// </summary>
        public readonly string[] Permission;

        /// <summary>
        /// 设置权限标记（多个权限为或关系，只要满足其中一个就有权限）
        /// </summary>
        /// <param name="permission"></param>
        public AdminAttribute(params string[] permission)
        {
            this.Permission = permission;
        }


    }
}
