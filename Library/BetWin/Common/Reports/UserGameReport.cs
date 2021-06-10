using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace BW.Common.Reports
{
    /// <summary>
    /// 团队的账户报表
    /// </summary>
    public struct UserGameReport
    {
        public UserGameReport(DataRow dr)
        {
            this.UserID = (int)dr["UserID"];
            this.Type = (Games.GameType)dr["Type"];
            this.Money = (decimal)dr["Money"];
            this.Amount = (decimal)dr["Amount"];
        }

        /// <summary>
        /// 团队领导
        /// </summary>
        public int UserID;

        /// <summary>
        /// 游戏类型
        /// </summary>
        public Games.GameType Type { get; set; }

        /// <summary>
        /// 盈亏金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 投注总额
        /// </summary>
        public decimal Amount { get; set; }

    }
}
