using System;
using System.Data.Linq.Mapping;
using System.Xml.Linq;

namespace BW.Common.Sites
{
    /// <summary>
    /// 站点缓存数据
    /// </summary>
    [Table(Name = "site_Cache")]
    public partial class SiteCache
    {
        [Column(Name = "CacheID", IsPrimaryKey = true)]
        public Guid ID { get; set; }

        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 缓存数据（XML）
        /// </summary>
        [Column(Name = "Data")]
        public string Data { get; set; }

        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        [Column(Name = "Type")]
        public CacheType Type { get; set; }

        private XElement _setting;
        public XElement Setting
        {
            get
            {
                if (string.IsNullOrEmpty(this.Data)) return null;
                if (_setting == null)
                {
                    _setting = XElement.Parse(this.Data);
                }
                return _setting;
            }
        }

        public enum CacheType : byte
        {
            /// <summary>
            /// 支付
            /// </summary>
            Payment
        }
    }
}
