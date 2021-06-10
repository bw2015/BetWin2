using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Model
{
    /// <summary>
    /// 格式化字符串
    /// </summary>
    public class FormatAttribute : Attribute
    {
        public FormatAttribute(string _format)
        {
            this.format = _format;
        }

        public string format { get; set; }
    }

    /// <summary>
    /// 设定动作所需要的参数
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ParameterNameAttribute : Attribute
    {
        public ParameterNameAttribute(params string[] names)
        {
            this.ParameterName = names;
        }

        /// <summary>
        /// 参数的名字
        /// </summary>
        public readonly string[] ParameterName;
    }
}
