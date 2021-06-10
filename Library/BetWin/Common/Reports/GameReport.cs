using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace BW.Common.Reports
{
    /// <summary>
    /// 第三方游戏报表
    /// </summary>
    public class GameReport
    {
        public GameReport(int userId, DataSet ds)
        {
            this.UserID = userId;
            if (ds.Tables.Count == 0) return;
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                switch ((string)dr["Type"])
                {
                    case "Video":
                        this.VideoBet = (decimal)dr["BetAmount"];
                        this.VideoMoney = (decimal)dr["Money"];
                        break;
                    case "Slot":
                        this.SlotBet = (decimal)dr["BetAmount"];
                        this.SlotMoney = (decimal)dr["Money"];
                        break;
                    case "Sport":
                        this.SportBet = (decimal)dr["BetAmount"];
                        this.SportMoney = (decimal)dr["Money"];
                        break;
                }
            }
        }

        /// <summary>
        /// 所属的团队/个人
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 真人视讯投注总额
        /// </summary>
        public decimal VideoBet { get; set; }

        /// <summary>
        /// 真人视讯盈亏
        /// </summary>
        public decimal VideoMoney { get; set; }


        /// <summary>
        /// 电子游艺投注总额
        /// </summary>
        public decimal SlotBet { get; set; }

        /// <summary>
        /// 电子游艺盈亏
        /// </summary>
        public decimal SlotMoney { get; set; }


        /// <summary>
        /// 体育游戏投注总额
        /// </summary>
        public decimal SportBet { get; set; }

        /// <summary>
        /// 体育游戏盈亏
        /// </summary>
        public decimal SportMoney { get; set; }
    }
}
