using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using SP.Studio.Xml;

namespace BW.Common.Admins
{
    /// <summary>
    /// 管理员的权限设定
    /// </summary>
    public struct Permission
    {
        public Permission(XElement element)
        {
            this.Name = element.GetAttributeValue("name");
            this.ID = element.GetAttributeValue("ID", Guid.Empty);
            IEnumerable<XElement> list = element.Elements();
            if (list.Count() == 0)
            {
                this.List = null;
            }
            else
            {
                this.List = new List<Permission>();
                foreach (XElement child in list)
                {
                    this.List.Add(new Permission(child));
                }
            }
        }

        /// <summary>
        /// 权限名字
        /// </summary>
        public string Name;

        /// <summary>
        /// 权限编号
        /// </summary>
        public Guid ID;

        /// <summary>
        /// 下级编号
        /// </summary>
        public List<Permission> List;


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"name\":\"{0}\"", this.Name)
                .AppendFormat(",\"id\":\"{0}\"", this.ID.ToString("N"))
                .Append(",\"list\":");

            if (this.List == null)
            {
                sb.Append("null");
            }
            else
            {
                sb.AppendFormat("[{0}]", string.Join(",", this.List.Select(t => t.ToString())));
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}
