using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Data;

using SP.Studio.Core;

using SP.Studio.Array;

using BW.GateWay.Lottery;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 投注订单枚举
    /// </summary>
    partial class LotteryOrder
    {
        public LotteryOrder() { }

        /// <summary>
        /// 获取从客户端传递过来的投注内容
        /// </summary>
        /// <param name="ht"></param>
        public LotteryOrder(Hashtable ht, DateTime? datetime = null)
        {
            int id = ht.GetValue("id", 0);
            string number = ht.GetValue("number", string.Empty);
            LotteryMode mode = ht.GetValue("mode", string.Empty).ToEnum<LotteryMode>();
            int times = ht.GetValue("times", 1);
            int rebate = ht.GetValue("rebate", 0);
            string index = ht.GetValue("index", string.Empty);

            LotteryType type;
            int betTime;
            this.PlayerID = id;
            // 检查玩法
            IPlayer player = PlayerFactory.GetPlayer(this.PlayerID, out type);
            if (player == null) return;
            this.Type = type;

            // 检查期号
            string betIndex = Utils.GetLotteryBetIndex(type, out betTime, datetime);
            if (string.IsNullOrEmpty(index)) index = betIndex;
            if (index != betIndex)
            {
                this.Remark = string.Format("当前可投注期为{0}", betIndex);
                return;
            }
            this.Index = string.IsNullOrEmpty(index) ? index : Utils.GetLotteryBetIndex(type, out betTime, datetime);

            this.Number = number;
            this.Bet = player.Bet(number);
            this.Mode = (decimal)mode / Utils.LOTTERYMODE_UNIT;
            this.Times = times;
            this.Money = this.Bet * this.Mode * this.Times;
            this.CreateAt = datetime == null ? DateTime.Now : datetime.Value;
            this.Status = OrderStatus.Normal;
            this.Rebate = rebate;
        }

        public LotteryOrder(DataRow dr)
        {
            DataColumnCollection columns = dr.Table.Columns;
            if (columns.Contains("OrderID")) this.ID = (int)dr["OrderID"];
            if (columns.Contains("SiteID")) this.SiteID = (int)dr["SiteID"];
            if (columns.Contains("UserID")) this.UserID = (int)dr["UserID"];
            if (columns.Contains("Type")) this.Type = (LotteryType)dr["Type"];
            if (columns.Contains("Index")) this.Index = (string)dr["Index"];
            if (columns.Contains("PlayerID")) this.PlayerID = (int)dr["PlayerID"];
            if (columns.Contains("Number")) this.Number = (string)dr["Number"];
            if (columns.Contains("Bet")) this.Bet = (int)dr["Bet"];
            if (columns.Contains("Mode")) this.Mode = (decimal)dr["Mode"];
            if (columns.Contains("Times")) this.Times = (int)dr["Times"];
            if (columns.Contains("Money")) this.Money = (decimal)dr["Money"];
            if (columns.Contains("CreateAt")) this.CreateAt = (DateTime)dr["CreateAt"];
            if (columns.Contains("LotteryAt")) this.LotteryAt = (DateTime)dr["LotteryAt"];
            if (columns.Contains("IsLottery")) this.IsLottery = (bool)dr["IsLottery"];
            if (columns.Contains("Reward")) this.Reward = (decimal)dr["Reward"];
            if (columns.Contains("ResultNumber")) this.ResultNumber = (string)dr["ResultNumber"];
            if (columns.Contains("Status")) this.Status = (OrderStatus)dr["Status"];
            if (columns.Contains("ChaseID")) this.ChaseID = (int)dr["ChaseID"];
            if (columns.Contains("UnitedID")) this.UnitedID = (int)dr["UnitedID"];
            if (columns.Contains("Rebate")) this.Rebate = (int)dr["Rebate"];
            if (columns.Contains("BetReturn")) this.BetReturn = (decimal)dr["BetReturn"];
            if (columns.Contains("Remark")) this.Remark = (string)dr["Remark"];
        }

        /// <summary>
        /// 设定奖金
        /// </summary>
        /// <param name="reward"></param>
        /// <param name="betReturn">需要返点的金额</param>
        /// <returns>如果返回false表示无效投注，需进行撤单处理</returns>
        public bool SetReward(decimal reward, out decimal betReturn)
        {
            betReturn = decimal.Zero;

            // 如果奖金等于-1则退还本金
            if (reward == decimal.MinusOne) return false;

            //if (this.IsLottery || this.Status != OrderStatus.Normal) return false;
            this.Reward = reward * this.Mode * this.Times / 2M;

            if (this.Type != LotteryType.MarkSix)
            {
                this.Reward *= (this.Rebate / 2000M);
                betReturn = this.Money * this.BetReturn;
            }
            this.Status = this.Reward != decimal.Zero ? OrderStatus.Win : OrderStatus.Faild;
            this.IsLottery = true;
            this.LotteryAt = DateTime.Now;
            return true;
        }


        public enum OrderStatus : byte
        {
            /// <summary>
            /// 正常，等待开奖
            /// </summary>
            [Description("等待开奖")]
            Normal = 0,
            /// <summary>
            /// 已撤单
            /// </summary>
            [Description("撤单")]
            Revoke = 1,
            /// <summary>
            /// 未中奖
            /// </summary>
            [Description("未中奖")]
            Faild = 2,
            /// <summary>
            /// 中奖订单
            /// </summary>
            [Description("中奖")]
            Win = 3,
            /// <summary>
            /// 订单发生错误
            /// </summary>
            [Description("错误")]
            Error = 10
        }

        public LotteryMode LotteryMode
        {
            get
            {
                int value = (int)(this.Mode * Utils.LOTTERYMODE_UNIT);
                return (LotteryMode)value;
            }
        }
    }
}
