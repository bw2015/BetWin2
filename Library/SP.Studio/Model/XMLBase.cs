using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using System.Reflection;
using SP.Studio.Xml;
using SP.Studio.Core;

namespace SP.Studio.Model
{
    /// <summary>
    /// 使用XML作为数据存储的实体基类
    /// </summary>
    public abstract class XMLBase : XMLElementBase
    {
        public XMLBase() : base() { }

        public XMLBase(XElement root)
            : base(root)
        {

        }
        /// <summary>
        /// 初始化元素
        /// </summary>
        /// <param name="xml"></param>
        public XMLBase(string xml)
            : this(string.IsNullOrEmpty(xml) ? null : XDocument.Parse(xml).Root)
        {

        }

        /// <summary>
        /// 把当前对象转化成为xml
        /// </summary>
        /// <returns></returns>
        public override XElement ToXml(string elementName = "root")
        {
            XElement root = new XElement(elementName);

            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (!property.CanWrite) continue;
                object value = property.GetValue(this, null);
                if (value != null)
                {
                    XElement item = new XElement(property.Name);
                    item.SetValue(value);
                    root.Add(item);
                }
            }

            return root;
        }

        public override string ToString()
        {
            return this.ToString("root");
        }
    }

    /// <summary>
    /// 一条记录的xml元素
    /// </summary>
    public abstract class XMLElementBase
    {
        public XMLElementBase() { }

        public XMLElementBase(XElement root)
        {
            if (root == null) return;
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (!property.CanWrite) continue;
                string value = root.GetValue(property.Name);
                if (string.IsNullOrEmpty(value)) value = root.GetAttributeValue(property.Name);
                property.SetValue(this, value.GetValue(property.PropertyType), null);
            }
        }

        public virtual XElement ToXml(string elementName = null)
        {
            if (string.IsNullOrEmpty(elementName)) elementName = this.GetType().Name;
            XElement root = new XElement(elementName);

            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (!property.CanWrite) continue;
                object value = property.GetValue(this, null).GetString(property.PropertyType);
                if (value != null)
                {
                    root.SetAttributeValue(property.Name, value);
                }
            }

            return root;
        }

        public virtual string ToString(string elementName)
        {
            return this.ToXml(elementName).ToString();
        }

        public override string ToString()
        {
            return this.ToString(null);
        }
    }
}
