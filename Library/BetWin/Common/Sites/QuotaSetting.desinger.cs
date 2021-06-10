/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 配额设定
    /// </summary>
    [Table(Name = "site_QuotaSetting")]
    public partial class QuotaSetting
    {

        [Column(Name = "QuotaID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 最小返点值
        /// </summary>
        [Column(Name = "MinRebate")]
        public int MinRebate { get; set; }

        /// <summary>
        /// 最大返点值
        /// </summary>
        [Column(Name = "MaxRebate")]
        public int MaxRebate { get; set; }

        /// <summary>
        /// 配额数量
        /// </summary>
        [Column(Name = "Number")]
        public int Number { get; set; }

    }
}
