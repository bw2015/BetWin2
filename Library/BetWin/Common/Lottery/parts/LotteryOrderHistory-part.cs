using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.Common.Lottery
{
    partial class LotteryOrderHistory
    {
        /// <summary>
        /// 转化成为订单
        /// </summary>
        /// <returns></returns>
        public static implicit operator LotteryOrder(LotteryOrderHistory order)
        {
            return new LotteryOrder()
            {
                ID = order.ID,
                Bet = order.Bet,
                BetReturn = order.BetReturn,
                ChaseID = order.ChaseID,
                CreateAt = order.CreateAt,
                Index = order.Index,
                IsLottery = order.IsLottery,
                LotteryAt = order.LotteryAt,
                Mode = order.Mode,
                Money = order.Money,
                Number = order.Number,
                PlayerID = order.PlayerID,
                Rebate = order.Rebate,
                Remark = order.Remark,
                ResultNumber = order.ResultNumber,
                Reward = order.Reward,
                SiteID = order.SiteID,
                Status = order.Status,
                Times = order.Times,
                Type = order.Type,
                UnitedID = order.UnitedID,
                UserID = order.UserID
            };
        }
    }
}
