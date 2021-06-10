using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Core
{
    /// <summary>
    /// 标记一个属性支持html格式传入
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HtmlEncodeAttribute : Attribute
    {
    }
}
