/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 彩票订单
    /// </summary>
    [Table(Name = "LotteryOrder")]
    public partial class LotteryOrder
    {      

        /// <summary>
        /// 投注编号
        /// </summary>
        [Column(Name = "OrderID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Lottery.LotteryType Type { get; set; }

        /// <summary>
        /// 彩期
        /// </summary>
        [Column(Name = "Index")]
        public string Index { get; set; }

        /// <summary>
        /// 玩法编号
        /// </summary>
        [Column(Name = "PlayerID")]
        public int PlayerID { get; set; }

        /// <summary>
        /// 投注内容
        /// </summary>
        [Column(Name = "Number")]
        public string Number { get; set; }

        /// <summary>
        /// 投注内容注数
        /// </summary>
        [Column(Name = "Bet")]
        public int Bet { get; set; }

        /// <summary>
        /// 模式，对应元角分厘
        /// </summary>
        [Column(Name = "Mode")]
        public decimal Mode { get; set; }

        /// <summary>
        /// 倍数
        /// </summary>
        [Column(Name = "Times")]
        public int Times { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 下单时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 开奖时间
        /// </summary>
        [Column(Name = "LotteryAt")]
        public DateTime LotteryAt { get; set; }

        /// <summary>
        /// 是否已经开奖
        /// </summary>
        [Column(Name = "IsLottery")]
        public bool IsLottery { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        [Column(Name = "Reward")]
        public Decimal Reward { get; set; }

        /// <summary>
        /// 开奖号码
        /// </summary>
        [Column(Name = "ResultNumber")]
        public string ResultNumber { get; set; }

        /// <summary>
        ///  该条投注的状态（0：正常 1：已撤单）
        /// </summary>
        [Column(Name = "Status")]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// 所属的追号计划
        /// </summary>
        [Column(Name = "ChaseID")]
        public int ChaseID { get; set; }

        /// <summary>
        /// 合买订单
        /// </summary>
        [Column(Name = "UnitedID")]
        public int UnitedID { get; set; }

        /// <summary>
        /// 投注所使用的奖金组
        /// </summary>
        [Column(Name = "Rebate")]
        public int Rebate { get; set; }

        /// <summary>
        /// 投注返点比例
        /// </summary>
        [Column(Name = "BetReturn")]
        public Decimal BetReturn { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "Remark")]
        public string Remark { get; set; }

        /// <summary>
        /// 所属分区
        /// </summary>
        [Column(Name = "TableID")]
        public int TableID { get; set; }

    }
}
