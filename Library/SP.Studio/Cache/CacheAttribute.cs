using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Cache
{
    /// <summary>
    /// 标记一个方法使用了缓存
    /// </summary>
    public class ICacheAttribute : Attribute
    {
        private string _id = "ID";
        /// <summary>
        /// 主键的名字
        /// </summary>
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                this._id = value;
            }
        }

        private string _name = "Name";
        /// <summary>
        /// 名字的标记
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        /// <summary>
        /// 设定的存储值
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// 缓存时间（秒）
        /// </summary>
        public int Time { get; set; }
    }

    /// <summary>
    /// 缓存类型
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// 页面缓存
        /// </summary>
        Page
    }
}
