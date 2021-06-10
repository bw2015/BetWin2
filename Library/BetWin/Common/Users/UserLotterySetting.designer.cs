/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    ///  用户对于彩票的设置（排序、分组）
    /// </summary>
    [Table(Name = "usr_LotterySetting")]
    public partial class UserLotterySetting
    {


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Game", IsPrimaryKey = true)]
        public BW.Common.Lottery.LotteryType Game { get; set; }

        /// <summary>
        /// 排序值（从小到大）
        /// </summary>
        [Column(Name = "Sort")]
        public short Sort { get; set; }

    }
}
