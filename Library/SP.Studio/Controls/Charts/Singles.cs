using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SP.Studio.Controls.Charts
{
    [Serializable, XmlRoot(ElementName = "chart")]
    public class Singles : XmlDataBase
    {
        public Singles()
        {
            this.ItemList = new List<Item>();
        }

        public List<Item> ItemList { get; set; }

        /// <summary>
        /// 转化成为XML
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.ToString());
            ItemList.ForEach(t =>
            {
                sb.Append(t.ToString());
            });
            sb.Append("</chart>");
            return sb.ToString();
        }

    }
}
