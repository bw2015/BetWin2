/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Reports
{
    /// <summary>
    ///  团队的日资金报表（包括自己）
    /// </summary>
    [Table(Name = "rpt_TeamDate")]
    public partial class TeamDateMoney
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        [Column(Name = "Date", IsPrimaryKey = true)]
        public DateTime Date { get; set; }

        /// <summary>
        /// 资金类型
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public BW.Common.Users.MoneyLog.MoneyType Type { get; set; }


        [Column(Name = "Money")]
        public Decimal Money { get; set; }

    }
}
