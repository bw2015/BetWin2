/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 资金锁定记录
    /// </summary>
    [Table(Name = "usr_MoneyLock")]
    public partial class MoneyLock
    {

        /// <summary>
        /// 锁定编号
        /// </summary>
        [Column(Name = "LockID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 锁定金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 锁定类型
        /// </summary>
        [Column(Name = "Type")]
        public LockType Type { get; set; }

        /// <summary>
        /// 锁定的目标表的关联字段
        /// </summary>
        [Column(Name = "SourceID")]
        public int SourceID { get; set; }

        /// <summary>
        /// 锁定时间
        /// </summary>
        [Column(Name = "LockAt")]
        public DateTime LockAt { get; set; }

        /// <summary>
        /// 解锁时间
        /// </summary>
        [Column(Name = "UnLockAt")]
        public DateTime UnLockAt { get; set; }

        /// <summary>
        ///  资金锁定的备注信息
        /// </summary>
        [Column(Name = "LockDesc")]
        public string Description { get; set; }

        /// <summary>
        /// 资金解锁时候的备注信息
        /// </summary>
        [Column(Name = "UnLockDesc")]
        public string UnLockDesc { get; set; }

    }
}
