using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SP.Studio.Core
{
    public class XmlSettingBase : ISetting
    {
        public XmlSettingBase() { }

        public XmlSettingBase(string str)
        {
            var obj = str.ToObject(this.GetType(), Encoding.UTF8);
            if (obj == null) return;
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                try
                {
                    p.SetValue(this, p.GetValue(obj, null), null);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{0}\n{1}", ex.Message, p.Name));
                }
            }
        }

        /// <summary>
        /// 可序列化的
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToXmlString(Encoding.UTF8);
        }
    }
}
