using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace SP.Studio.Controls.Charts
{
    /// <summary>
    /// 生成xml数据
    /// </summary>
    [Serializable, XmlRoot(ElementName = "chart")]
    public abstract class XmlDataBase
    {
        private string _bgColor = "#FFFFFF";
        /// <summary>
        /// 背景颜色
        /// </summary>
        public string bgColor
        {
            get
            {
                return _bgColor;
            }
            set
            {
                _bgColor = value;
            }
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string caption { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string subcaption { get; set; }

        /// <summary>
        /// 底部文字
        /// </summary>
        public string xAxisName { get; set; }

        /// <summary>
        /// 左侧文字
        /// </summary>
        public string yAxisName { get; set; }

        private string _numberPrefix = "$";
        /// <summary>
        /// 数字前缀
        /// </summary>
        public string numberPrefix
        {
            get
            {
                return _numberPrefix;
            }
            set
            {
                _numberPrefix = value;
            }
        }

        private string _outCnvBaseFont = "宋体";
        /// <summary>
        /// 基本字体
        /// </summary>
        public string outCnvBaseFont
        {
            get
            {
                return _outCnvBaseFont;
            }
            set
            {
                _outCnvBaseFont = value;
            }
        }

        private int _outCnvBaseFontSize = 12;
        /// <summary>
        /// 字体大小
        /// </summary>
        public int outCnvBaseFontSize
        {
            get
            {
                return _outCnvBaseFontSize;
            }
            set
            {
                _outCnvBaseFontSize = value;
            }
        }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public string outCnvBaseFontColor { get; set; }

        /// <summary>
        /// 显示边框
        /// </summary>
        public bool showBorder { get; set; }

        /// <summary>
        /// 保存图片功能
        /// </summary>
        public bool imageSave { get; set; }

        /// <summary>
        /// 图片保存路径
        /// </summary>
        public string imageSaveURL { get; set; }

        private bool _showValues = true;
        /// <summary>
        /// 是否显示值
        /// </summary>
        public bool showValues
        {
            get
            {
                return _showValues;
            }
            set
            {
                _showValues = value;
            }
        }

        /// <summary>
        /// 对柱状图是否使用圆角
        /// </summary>
        public bool useRoundEdges { get; set; }

        /// <summary>
        /// 根属性
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("<chart");
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
            sb.Append(">");
            return sb.ToString();
        }


        public static implicit operator string(XmlDataBase data)
        {
            return data.ToString();
        }

    }
}
