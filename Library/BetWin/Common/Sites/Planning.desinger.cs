/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 系统活动设置
    /// </summary>
    [Table(Name = "site_Planning")]
    public partial class Planning
    {

        /// <summary>
        /// 活动编号
        /// </summary>
        [Column(Name = "PlanID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        ///  活动类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.GateWay.Planning.PlanType Type { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 是否开启
        /// </summary>
        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// 系统的设定值
        /// </summary>
        [Column(Name = "Setting")]
        public String Setting { get; set; }

    }
}
