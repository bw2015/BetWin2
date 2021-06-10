using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Model
{
    /// <summary>
    /// 权限设定
    /// </summary>
    public class PermissionAttribute : System.Attribute
    {
        public PermissionAttribute(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }
    }
}
