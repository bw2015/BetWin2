using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using System.Reflection;

namespace SP.Studio.Controls.Charts
{
    public sealed class Item 
    {
        public string label { get; set; }

        public int value { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public string color { get; set; }

        /// <summary>
        /// 自定义的鼠标提示
        /// </summary>
        public string toolText { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string link { get; set; }

        private bool _showLabel = true;
        /// <summary>
        /// 显示标签
        /// </summary>
        public bool showLabel
        {
            get
            {
                return _showLabel;
            }
            set
            {
                _showLabel = value;
            }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("<set");
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                object obj = property.GetValue(this, null);
                string objValue = null;
                switch (property.PropertyType.Name)
                {
                    case "String":
                        objValue = (string)obj;
                        break;
                    case "Int32":
                        objValue = obj.ToString();
                        break;
                    case "Boolean":
                        objValue = (bool)obj ? "1" : "0";
                        break;
                }

                if (!string.IsNullOrEmpty(objValue))
                {
                    sb.AppendFormat(" {0}='{1}'", property.Name, objValue);
                }
            }
            sb.Append(" />");
            return sb.ToString();
        }
    }
}
