/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 站点所使用的域名
    /// </summary>
    [Table(Name = "site_Domain")]
    public partial class SiteDomain
    {

        /// <summary>
        /// 域名
        /// </summary>
        [Column(Name = "Domain", IsPrimaryKey = true)]
        public string Domain { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// CDN高防域名
        /// </summary>
        [Column(Name = "IsCDN")]
        public bool IsCDN { get; set; }

        /// <summary>
        /// 测速域名
        /// </summary>
        [Column(Name = "IsSpeed")]
        public bool IsSpeed { get; set; }

        /// <summary>
        /// 权重，从大到小。
        /// </summary>
        [Column(Name = "Sort")]
        public short Sort { get; set; }

        /// <summary>
        /// 对外的链接
        /// </summary>
        public string Link
        {
            get
            {
                return string.Format("{0}://{1}", this.Domain.EndsWith("443") ? "https" : "http", this.Domain);
            }
        }

    }
}
