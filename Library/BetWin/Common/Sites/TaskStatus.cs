/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 任务状态
    /// </summary>
    [Table(Name = "site_TaskStatus")]
    public partial class TaskStatus
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "SourceID", IsPrimaryKey = true)]
        public int SourceID { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public TaskType Type { get; set; }

        /// <summary>
        /// 任务开始时间
        /// </summary>
        [Column(Name = "StartAt")]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// 任务结束时间
        /// </summary>
        [Column(Name = "EndAt")]
        public DateTime EndAt { get; set; }

        /// <summary>
        /// 任务运行击数
        /// </summary>
        [Column(Name = "Count")]
        public int Count { get; set; }

        /// <summary>
        /// 任务总数
        /// </summary>
        [Column(Name = "Total")]
        public int Total { get; set; }

    }
}
