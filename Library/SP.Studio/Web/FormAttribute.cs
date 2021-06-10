using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

using SP.Studio.Core;
using SP.Studio.Data;

namespace SP.Studio.Web
{
    /// <summary>
    /// 表单元素
    /// </summary>
    public class FormAttribute : Attribute
    {
        #region =========== 常量配置  ==========

        /// <summary>
        /// 当前时间
        /// </summary>
        public const string getdate = "getdate()";

        /// <summary>
        /// 省
        /// </summary>
        public const string province = "province()";

        /// <summary>
        /// 市
        /// </summary>
        public const string city = "city()";

        /// <summary>
        /// 县/区
        /// </summary>
        public const string area = "area()";

        #endregion

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool Require { get; set; }

        /// <summary>
        /// 字段标题
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public FormFieldType Type { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// 最少的字符串数量
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// 最多的字符串数量
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// 定义正则表达式来验证
        /// </summary>
        public Regex Regex { get; set; }

        /// <summary>
        /// 转化成为data属性
        /// </summary>
        public virtual string ToData()
        {
            List<string> list = new List<string>();
            if (Require) list.Add("data-require=\"1\"");
            if (!string.IsNullOrEmpty(this.Label)) list.Add("data-label=\"" + this.Label + "\"");
            if (!string.IsNullOrEmpty(this.Default)) list.Add("data-default=\"" + this.Default + "\"");
            if (this.Min != 0) list.Add("data-min=" + this.Min);
            if (this.Max != 0) list.Add("data-max=" + this.Max);
            if (this.Regex != null) list.Add("data-regex=\"" + this.Regex.ToString() + "\"");
            return string.Join(" ", list);
        }

        /// <summary>
        /// 显示表单元素
        /// </summary>
        public virtual string ToControl(string name, string id = null)
        {
            if (id == null) id = name;
            StringBuilder sb = new StringBuilder();
            switch (this.Type)
            {
                case FormFieldType.input:
                case FormFieldType.hidden:
                case FormFieldType.password:
                    sb.AppendFormat("<input type=\"{3}\" name=\"{0}\" id=\"{1}\" class=\"txt\" {2} />", name, id, this.ToData(), this.Type);
                    break;
                case FormFieldType.textarea:
                    sb.AppendFormat("<textarea name=\"{0}\" id=\"{1}\" class=\"txt\" {2}></textarea>", name, id, this.ToData());
                    break;
                case FormFieldType.select:
                    sb.AppendFormat("<select name=\"{0}\" id=\"{1}\" class=\"{1}\" {2}></select>", name, id, this.ToData());
                    break;
                default:
                    sb.Append(this.Type.ToString());
                    break;
            }
            return sb.ToString();
        }
    }

    public enum FormFieldType
    {
        /// <summary>
        /// 输入框
        /// </summary>
        input,
        /// <summary>
        /// 密码
        /// </summary>
        password,
        /// <summary>
        /// 多文本输入框
        /// </summary>
        textarea,
        /// <summary>
        /// 下拉选择
        /// </summary>
        select,
        /// <summary>
        /// 单选
        /// </summary>
        radio,
        /// <summary>
        /// 多选
        /// </summary>
        checkbox,
        /// <summary>
        /// 隐藏元素
        /// </summary>
        hidden
    }


}
