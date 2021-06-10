/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 开奖结果
    /// </summary>
    [Table(Name = "lot_ResultNumber")]
    public partial class ResultNumber
    {

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public LotteryType Type { get; set; }


        [Column(Name = "Index", IsPrimaryKey = true)]
        public string Index { get; set; }

        /// <summary>
        /// 开奖号码
        /// </summary>
        [Column(Name = "Number")]
        public string Number { get; set; }

        /// <summary>
        /// 平台的开奖时间
        /// </summary>
        [Column(Name = "ResultAt")]
        public DateTime ResultAt { get; set; }

        /// <summary>
        /// 是否已经开奖
        /// </summary>
        [Column(Name = "IsLottery")]
        public bool IsLottery { get; set; }

    }
}
