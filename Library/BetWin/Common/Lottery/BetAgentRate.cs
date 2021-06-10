using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 上级返点比例
    /// </summary>
    public struct BetAgentRate
    {
        public BetAgentRate(int userId, decimal rate)
        {
            this.UserID = userId;
            this.Rate = rate;
        }

        /// <summary>
        /// 上级用户ID
        /// </summary>
        public int UserID;

        /// <summary>
        /// 上级返点比例
        /// </summary>
        public decimal Rate;
    }
}
