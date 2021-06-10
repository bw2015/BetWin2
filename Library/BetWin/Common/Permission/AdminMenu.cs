using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using SP.Studio.Xml;
using BW.Common.Admins;

namespace BW.Common.Permission
{
    /// <summary>
    /// 管理员菜单
    /// </summary>
    public class AdminMenu
    {
        public AdminMenu(XElement menu, Admin admin, List<AdminMenu> list)
        {
            this.Name = menu.GetAttributeValue("name");
            this.Href = menu.GetAttributeValue("href");
            this.ID = menu.GetAttributeValue("ID");
            this.Icon = menu.GetAttributeValue("icon");
            this.Menu = list;
        }

        /// <summary>
        /// 菜单名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 链接地址
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// 权限编号
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 下级菜单
        /// </summary>
        public List<AdminMenu> Menu { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"name\":\"{0}\" ,", this.Name)
                .AppendFormat("\"href\":\"{0}\" ,", this.Href)
                .AppendFormat("\"id\":\"{0}\",", this.ID)
                .AppendFormat("\"icon\":\"{0}\"", this.Icon);

            if (this.Menu != null && this.Menu.Count > 0)
            {
                sb.Append(",\"menu\":[");
                sb.Append(string.Join(",", this.Menu.Select(t => t.ToString())));
                sb.Append("]");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}
