using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace SP.Studio.Core
{
    /// <summary>
    /// 可以和JSON字符串转换的实体类基类
    /// </summary>
    [DataContract, Serializable]
    public abstract class JsonBase : ISetting
    {
        public JsonBase() { }

        public JsonBase(string str)
        {
            if (str != null) str = str.Trim();
            if (string.IsNullOrEmpty(str)) return;
            
            object obj = str.ToObject(this.GetType());
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

        public override string ToString()
        {
            return this.ToJsonString();
        }
    }
}
