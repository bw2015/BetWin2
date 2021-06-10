using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.PageBase
{
    public struct MethodResult
    {
        /// <summary>
        /// 类
        /// </summary>
        public Type type;

        /// <summary>
        /// 动作
        /// </summary>
        public MethodInfo method;

        /// <summary>
        /// 代理层类别
        /// </summary>
        public string agent;
    }
}
