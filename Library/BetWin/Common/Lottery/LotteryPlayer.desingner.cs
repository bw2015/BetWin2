/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 彩种玩法设定
    /// </summary>
    [Table(Name = "lot_Player")]
    public partial class LotteryPlayer
    {

        /// <summary>
        /// 玩法ID
        /// </summary>
        [Column(Name = "PlayerID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 玩法代码,彩种_Player，通过彩种代码查找玩法对象
        /// </summary>
        [Column(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        ///  彩票类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Lottery.LotteryType Type { get; set; }


        [Column(Name = "GroupName")]
        public string GroupName { get; set; }


        [Column(Name = "LabelName")]
        public string LabelName { get; set; }


        [Column(Name = "PlayName")]
        public string PlayName { get; set; }

        /// <summary>
        /// 支持移动端
        /// </summary>
        [Column(Name = "IsMobile")]
        public bool IsMobile { get; set; }

        /// <summary>
        /// 百分比，投注数量小于全部投注数量的百分之多少，等于单挑
        /// </summary>
        [Column(Name = "SingledBet")]
        public int SingledBet { get; set; }

        /// <summary>
        /// 单挑的奖金封顶
        /// </summary>
        [Column(Name = "SingledReward")]
        public Decimal SingledReward { get; set; }

        /// <summary>
        /// 最多允许的百分比，避免全包
        /// </summary>
        [Column(Name = "MaxBet")]
        public int MaxBet { get; set; }

        /// <summary>
        /// 是否开放玩法
        /// </summary>
        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// 玩法奖金
        /// </summary>
        [Column(Name = "Reward")]
        public Decimal Reward { get; set; }

        /// <summary>
        /// 排序值，从大到小
        /// </summary>
        [Column(Name = "Sort")]
        public short Sort { get; set; }

    }
}
