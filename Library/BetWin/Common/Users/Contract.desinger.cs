/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户契约
    /// </summary>
    [Table(Name = "usr_Contract")]
    public partial class Contract
    {

        /// <summary>
        /// 契约编号
        /// </summary>
        [Column(Name = "ContractID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 契约甲方
        /// </summary>
        [Column(Name = "User1")]
        public int User1 { get; set; }

        /// <summary>
        /// 契约乙方
        /// </summary>
        [Column(Name = "User2")]
        public int User2 { get; set; }

        /// <summary>
        /// 契约的签订日期
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///  契约类型（分红、日工资）
        /// </summary>
        [Column(Name = "Type")]
        public ContractType Type { get; set; }

        /// <summary>
        /// 契约内容
        /// </summary>
        [Column(Name = "Content")]
        public String Content { get; set; }

        /// <summary>
        /// 当前契约状态		0、待确认		1、已正确生效		3、请求取消		4、已取消（可重新编辑，状态变更为待确认）
        /// </summary>
        [Column(Name = "Status")]
        public ContractStatus Status { get; set; }

    }
}
