using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using System.Web;

using SP.Studio.Xml;
using SP.Studio.Core;

namespace SP.Studio.Model
{
    /// <summary>
    /// 实体类基类
    /// </summary>
    public class ModelBase<T> where T : class,new()
    {
        public string ToControl(Expression<Func<T, object>> expression)
        {
            throw new NotImplementedException();
        }

        #region ========== XML 的扩展类  ============

        /// <summary>
        /// 扩展的XML内容
        /// </summary>
        protected virtual string _extendXML { get; set; }

        private XElement _xmlObj;
        [NoFillAttribute]
        public XElement XMLObj
        {
            get
            {
                if (string.IsNullOrEmpty(this._extendXML) && _xmlObj == null) return null;
                if (_xmlObj == null)
                {
                    try
                    {
                        _xmlObj = XDocument.Parse(this._extendXML).Root;
                    }
                    catch (Exception ex)
                    {
                        //throw new Exception(ex.Message + "\n" + this._extendXML);
                        _xmlObj = new XElement("root");
                        _xmlObj.SetAttributeValue("Exception", ex.Message);
                    }
                }
                return _xmlObj;
            }
            set { _xmlObj = value; }
        }

        /// <summary>
        /// 获取扩展类的内容
        /// </summary>
        public virtual string GetElement(string key)
        {
            return this.GetElement<string>(key);
        }

        /// <summary>
        /// 获取扩展类的内容
        /// </summary>
        public virtual TObj GetElement<TObj>(string key)
        {
            if (this.XMLObj == null) return default(TObj);
            XElement item = this.XMLObj.Elements("item").FirstOrDefault(t => t.Attribute("name").Value == key);
            if (item == null) return default(TObj);
            string value = item.Attribute("value").Value;
            return (TObj)value.GetValue(typeof(TObj));
        }

        /// <summary>
        /// 获取扩展类的列表
        /// </summary>
        public virtual Dictionary<string, string> GetElements()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (this.XMLObj == null) return dic;
            foreach (XElement item in this.XMLObj.Elements("item"))
            {
                string name = item.GetAttributeValue("name");
                string value = item.GetAttributeValue("value");
                if (dic.ContainsKey(name)) continue;
                dic.Add(name, value);
            }
            return dic;
        }

        /// <summary>
        /// 新增或者修改XML中的内容
        /// </summary>
        public virtual void SaveElement(string key, object value)
        {
            if (this.XMLObj == null) this._xmlObj = new XElement("root");
            XElement item = this._xmlObj.Elements("item").FirstOrDefault(t => t.Attribute("name").Value == key);
            if (item == null)
            {
                item = new XElement("item");
                item.SetAttributeValue("name", key);
                this._xmlObj.Add(item);
            }
            item.SetAttributeValue("value", value);
            this._extendXML = this._xmlObj.ToString();
        }

        /// <summary>
        /// 自动从Request.Form里面获取要保存到xml的扩展内容
        /// </summary>
        /// <param name="prefix">前缀字段，包括点号</param>
        public virtual void SaveElement(string prefix = "Extend.")
        {
            NameValueCollection qf = HttpContext.Current.Request.Form;
            foreach (string key in qf.AllKeys.Where(t => t.StartsWith(prefix)))
            {
                string name = key.Substring(prefix.Length);
                string value = qf[key];
                this.SaveElement(name, value);
            }
        }

        /// <summary>
        /// 删除一个节点
        /// </summary>
        /// <param name="key"></param>
        public virtual void RemoveElement(string key)
        {
            if (this._xmlObj == null) return;
            XElement item = this._xmlObj.Elements("item").FirstOrDefault(t => t.Attribute("name").Value == key);
            if (item != null)
            {
                item.Remove();
                this._extendXML = this._xmlObj.ToString();
            }
        }

        /// <summary>
        /// 清除xml的所有配置
        /// </summary>
        public virtual void ClearElement()
        {
            this._xmlObj = new XElement("root");
        }



        #endregion

    }
}
