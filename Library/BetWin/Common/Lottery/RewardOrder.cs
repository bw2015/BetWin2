using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 待开奖的订单列表
    /// </summary>
    public struct RewardOrder
    {
        public RewardOrder(DataRow dr)
        {
            this.Type = (LotteryType)dr["Type"];
            this.OrderID = (int)dr["OrderID"];
            this.Number = (string)dr["Number"];
            this.UserID = (int)dr["UserID"];
        }

        /// <summary>
        /// 彩种
        /// </summary>
        public LotteryType Type;

        /// <summary>
        /// 订单号
        /// </summary>
        public int OrderID;

        /// <summary>
        /// 开奖号码
        /// </summary>
        public string Number;

        /// <summary>
        /// 订单所属的用户
        /// </summary>
        public int UserID;

    }
}
