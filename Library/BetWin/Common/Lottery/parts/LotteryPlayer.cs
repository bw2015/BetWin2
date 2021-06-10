using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SP.Studio.Core;
using BW.GateWay.Lottery;

using BW.Agent;
using BW.Common.Users;

namespace BW.Common.Lottery
{

    partial class LotteryPlayer : CommBase
    {

        public LotteryPlayer() { }

        public LotteryPlayer(IPlayer player, LotteryType type)
        {
            this.GroupName = player.Group;
            this.LabelName = player.Label;
            this.PlayName = player.Name;
            this.Code = string.Concat(type, "_", player.Code);
            this.Type = type;
        }

        private IPlayer _player;

        /// <summary>
        /// 玩法逻辑对象
        /// </summary>
        public IPlayer Player
        {
            get
            {
                if (_player == null)
                {
                    LotteryType type;
                    _player = PlayerFactory.GetPlayer(this.Code, out type);
                }
                return _player;
            }
            private set
            {
                _player = value;
            }
        }

        /// <summary>
        /// 转换成为json
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(new
            {
                this.ID,
                this.Code,
                this.GroupName,
                this.LabelName,
                this.PlayName,
                this.IsMobile,
                this.IsOpen,
                this.SingledBet,
                this.SingledReward,
                this.MaxBet,
                this.Type,
                this.Name,
                RewardMoney = this.Reward == decimal.Zero ? this.Player.RewardMoney : this.Reward,
                Game = LotteryAgent.Instance().GetLotteryName(this.Type),
                Wechat = this.Player.HasAttribute<BetChatAttribute>() ? 1 : 0,
                this.Sort
            }.ToJson());

            return sb.ToString();
        }

        /// <summary>
        /// 返回奖金组
        /// </summary>
        /// <param name="userRebate">用户的奖金组</param>
        /// <param name="singleReward">单挑的奖金</param>
        /// <param name="singlerPercent">单挑的比例</param>
        /// <returns></returns>
        public string ToString(int userRebate, decimal singlerPercent = decimal.Zero, decimal singleReward = decimal.Zero)
        {
            StringBuilder sb = new StringBuilder();
            int full = this.Player.Full;
            sb.Append(new
            {
                this.ID,
                this.Code,
                this.GroupName,
                this.LabelName,
                this.PlayName,
                this.IsMobile,
                this.IsOpen,
                SingledBet = this.GetSingleBet(singlerPercent),
                SingledReward = this.GetSingleReward(singleReward),
                this.MaxBet,
                this.Type,
                this.Name,
                RewardMoney = Utils.GetReward(this.Reward == decimal.Zero ? this.Player.RewardMoney : this.Reward, userRebate),
                Full = full,
                this.Player.Tip,
                Game = LotteryAgent.Instance().GetLotteryName(this.Type)
            }.ToJson());

            return sb.ToString();
        }

        /// <summary>
        /// 玩法名字
        /// </summary>
        public string Name
        {
            get
            {
                return string.Concat(this.GroupName, " ", this.LabelName, " ", this.PlayName);
            }
        }

        /// <summary>
        /// 全包的注数（为0表示不做单挑、全包限制）
        /// </summary>
        public int Full
        {
            get
            {
                if (this.Player == null) return 0;
                return this.Player.Full;
            }
        }

        /// <summary>
        ///  获取单挑注数
        /// </summary>
        /// <param name="singlerPercent">彩种设定的单挑比例</param>
        /// <returns></returns>
        public int GetSingleBet(decimal singlerPercent)
        {
            if (this.SingledBet != 0) return this.SingledBet;
            if (this.Full == 0) return 0;
            if (singlerPercent == decimal.Zero) return 0;
            return (int)(singlerPercent * (decimal)this.Full);
        }

        /// <summary>
        /// 获取单挑奖金
        /// </summary>
        /// <param name="singleReward"></param>
        /// <returns></returns>
        public decimal GetSingleReward(decimal singleReward)
        {
            return this.SingledReward != decimal.Zero ? this.SingledReward : singleReward;
        }
    }
}
