using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.P28
{
    /// <summary>
    /// 特码和值
    /// 从0到27中选择一个数字进行投注，所选数值等于三个开奖号码的相加之和(特码数字，即为中奖
    /// </summary>
    public class Player11 : IP28
    {
        public override string[] InputBall => new[]
        {
            "0","1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19","20","21","22","23","24","25","26","27"
        };

        public override int Bet(string input)
        {
            return input.Split(',').Distinct().Count();
        }


        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            decimal money = decimal.Zero;
            int result = WebAgent.GetArray<int>(number).Last();
            int[] inputNumber = WebAgent.GetArray<int>(input);
            if (!inputNumber.Contains(result)) return money;
            if (rewardMoney == decimal.Zero) rewardMoney = this.RewardMoney;
            return this.rewardMoney[result] * rewardMoney / this.RewardMoney;
        }

        public override decimal RewardMoney => 2000M;

        public Dictionary<int, decimal> rewardMoney => new Dictionary<int, decimal>()
        {
            {0,2000M },
            {1,666M },
            {2,333M },
            {3,200M },
            {4,132M },
            {5,94M },
            {6,70M },
            {7,54M },
            {8,44M },
            {9,36M },
            {10,30M },
            {11,28M },
            {12,26M },
            {13,26M },
            {14,26M },
            {15,26M },
            {16,28M },
            {17,30M },
            {18,36M },
            {19,44M },
            {20,54M },
            {21,70M },
            {22,94M },
            {23,132M },
            {24,200M },
            {25,333M },
            {26,666M },
            {27,2000M }
        };
    }
}
