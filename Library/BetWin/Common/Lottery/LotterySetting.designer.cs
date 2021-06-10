/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
	///  彩票设置
	/// </summary>
    [Table(Name = "lot_Setting")]
    public partial class LotterySetting
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 彩种类型
        /// </summary>
        [Column(Name = "Game", IsPrimaryKey = true)]
        public BW.Common.Lottery.LotteryType Game { get; set; }

        /// <summary>
        /// 自定义的彩种名字
        /// </summary>
        [Column(Name = "GameName")]
        public string Name { get; set; }

        /// <summary>
        /// 彩种备注
        /// </summary>
        [Column(Name = "GameDesc")]
        public string Description { get; set; }

        /// <summary>
        /// 是否开放该游戏
        /// </summary>
        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// 不允许追号
        /// </summary>
        [Column(Name = "NoChase")]
        public bool NoChase { get; set; }

        /// <summary>
        /// 允许最高的奖金组，用户在该彩种的奖金组计算公式为 UserRebate × (彩种奖金组/站点奖金组)
        /// </summary>
        [Column(Name = "MaxRebate")]
        public int MaxRebate { get; set; }

        /// <summary>
        /// 自定义排序
        /// </summary>
        [Column(Name = "Sort")]
        public short Sort { get; set; }

        /// <summary>
        /// 中奖概率
        /// </summary>
        [Column(Name = "RewardPercent")]
        public Decimal RewardPercent { get; set; }

        /// <summary>
        /// 手动开奖（选上之后系统不自动开奖）
        /// </summary>
        [Column(Name = "IsManual")]
        public bool IsManual { get; set; }

        /// <summary>
        /// 单个用户单期最多投注
        /// </summary>
        [Column(Name = "MaxBet")]
        public Decimal MaxBet { get; set; }

        /// <summary>
        /// 所属分类
        /// </summary>
        [Column(Name = "CateID")]
        public int CateID { get; set; }

        /// <summary>
        /// 单挑比例，为0不限制单挑
        /// </summary>
        [Column(Name = "SinglePercent")]
        public Decimal SinglePercent { get; set; }

        /// <summary>
        /// 单挑奖金封顶
        /// </summary>
        [Column(Name = "SingleReward")]
        public Decimal SingleReward { get; set; }

        /// <summary>
        /// 限制最多投注，为0表示不限制。 可被玩法的最多投注覆盖
        /// </summary>
        [Column(Name = "MaxPercent")]
        public Decimal MaxPercent { get; set; }

    }
}
