/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Games
{
    /// <summary>
    /// 游戏账户预存分数变化
    /// </summary>
    [Table(Name = "game_MoneyLog")]
    public partial class GameMoneyLog
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "Type")]
        public BW.Common.Games.GameType Type { get; set; }


        [Column(Name = "Money")]
        public Decimal Money { get; set; }


        [Column(Name = "Balance")]
        public Decimal Balance { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 对应的转账ID
        /// </summary>
        [Column(Name = "SourceID")]
        public int SourceID { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "LogDesc")]
        public string Description { get; set; }

    }
}
