using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery.SmartGame
{
    public class Kawaii : IGamePlayer
    {
        public override LotteryCategory Type => LotteryCategory.SmartGame;

        public override string[] InputBall => new string[] { "石头", "剪刀", "布" };

        public override int Bet(string input)
        {
            return this.IsMatch(input) ? 1 : 0;
        }

        public override bool IsMatch(string input)
        {

            return this.InputBall.Contains(input);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.InputBall.Contains(number)) return decimal.Zero;

            int numberIndex = Array.IndexOf(this.InputBall, number);
            int inputIndex = Array.IndexOf(this.InputBall, input);

            decimal reward = decimal.Zero;
            switch (numberIndex - inputIndex)
            {
                case -1:
                case 2:
                    reward = decimal.Zero;
                    break;
                case 0:
                    reward = decimal.MinusOne;
                    break;
                case -2:
                case 1:
                    reward = this.GetRewardMoney(rewardMoney);
                    break;
            }
            return reward;
        }

        public override decimal RewardMoney => 4M;
    }
}
