using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using System.Web;

using SP.Studio.Data;
using SP.Studio.Web;
using SP.Studio.Core;

namespace SP.Studio.Model
{
    /// <summary>
    /// 具有表单元素的继承类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FormModel<T> where T : class ,new()
    {
        /// <summary>
        /// 插入一条记录
        /// </summary>
        public virtual void Insert(HttpContext context)
        {
            context.Request.Form.Fill(this);
            this.InsertAfter();
            Result result = this.InsertCheck();
            if (result.Success <= 0)
            {
                context.Response.Write(result.ToJson());
                return;
            }

            try
            {
                this.Add(true);
                result.Message = "添加成功";
            }
            catch (Exception ex)
            {
                result = new Result(false, ex.Message);
            }
            finally
            {
                context.Response.Write(result.ToJson());
            }
        }

        /// <summary>
        /// 插入之前进行的处理
        /// </summary>
        protected virtual void InsertAfter()
        {
            // do something...
        }

        /// <summary>
        /// 插入之前的数据检查
        /// </summary>
        protected virtual Result InsertCheck()
        {
            foreach (PropertyInfo property in this.GetType().GetProperties().Where(p => p.HasAttribute(typeof(FormAttribute))))
            {
                FormAttribute att = property.GetAttribute<FormAttribute>();
                object value = property.GetValue(this, null);
                if (att.Require && string.IsNullOrEmpty(value.ToString())) return new Result(false, "“{0}”未填写", att.Label);
            }
            return new Result();
        }

        /// <summary>
        /// 输出成为表单（JSON）
        /// </summary>
        public virtual string ToForm()
        {
            Type type = typeof(T);
            List<Object> list = new List<Object>();
            foreach (PropertyInfo property in type.GetProperties().Where(t => t.HasAttribute(typeof(FormAttribute))))
            {
                FormAttribute att = property.GetAttribute<FormAttribute>();
                if (string.IsNullOrEmpty(att.Label)) continue;
                Object obj = new
                {
                    Label = att.Label,
                    Require = att.Require,
                    Control = att.ToControl(property.Name)
                };
                list.Add(obj);
            }
            return list.ToJson();
        }
    }
}
