/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 提现处理日志
    /// </summary>
    [Table(Name = "usr_WithdrawOrderLog")]
    public partial class WithdrawOrderLog
    {

        /// <summary>
        ///  日志编号
        /// </summary>
        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 提现订单
        /// </summary>
        [Column(Name = "WithdrawID")]
        public int WithdrawID { get; set; }

        /// <summary>
        /// 处理人
        /// </summary>
        [Column(Name = "AdminID")]
        public int AdminID { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        [Column(Name = "Status")]
        public BW.Common.Users.WithdrawOrder.WithdrawStatus Status { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 处理备注
        /// </summary>
        [Column(Name = "LogDesc")]
        public string Description { get; set; }

    }
}
