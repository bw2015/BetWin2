/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Games
{
    /// <summary>
    /// 游戏间的转账记录日志
    /// </summary>
    [Table(Name = "game_TransferLog")]
    public partial class TransferLog
    {

        /// <summary>
        /// 转账编号
        /// </summary>
        [Column(Name = "TransferID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Games.GameType Type { get; set; }

        /// <summary>
        /// 转账金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 转账提交的时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 到账时间（或者检查到确认失败的时间）
        /// </summary>
        [Column(Name = "CheckAt")]
        public DateTime CheckAt { get; set; }

        /// <summary>
        /// 转账状态
        /// </summary>
        [Column(Name = "Status")]
        public TransferStatus Status { get; set; }

        /// <summary>
        /// 转账动作（转入或者转出）
        /// </summary>
        [Column(Name = "Action")]
        public ActionType Action { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "TransferDesc")]
        public string Description { get; set; }

    }
}
