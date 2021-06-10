/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    ///  活动任务运行状态
    /// </summary>
    [Table(Name = "site_PlanStatus")]
    public partial class PlanStatus
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        ///  活动类型
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public BW.GateWay.Planning.PlanType Type { get; set; }


        [Column(Name = "SourceID", IsPrimaryKey = true)]
        public int SourceID { get; set; }

        /// <summary>
        /// 任务创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 任务结束时间
        /// </summary>
        [Column(Name = "EndAt")]
        public DateTime EndAt { get; set; }

        /// <summary>
        /// 总任务数量
        /// </summary>
        [Column(Name = "Total")]
        public int Total { get; set; }

        /// <summary>
        /// 成功的数量
        /// </summary>
        [Column(Name = "Count")]
        public int Count { get; set; }

    }
}
