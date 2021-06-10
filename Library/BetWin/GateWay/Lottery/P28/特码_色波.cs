using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.P28
{
    /// <summary>
    /// 色波
    /// </summary>
    public class Player15 : IP28
    {
        public override string[] InputBall => new string[] { "红波", "蓝波", "绿波" };

        private int[] RED = new int[] { 3, 6, 9, 12, 15, 18, 21, 24 };
        private int[] BLUE = new int[] { 1, 4, 7, 10, 16, 19, 22, 25 };
        private int[] GREEN = new int[] { 2, 5, 8, 11, 17, 20, 23, 26 };

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            int[] num = WebAgent.GetArray<int>(number);
            int lastNum = num.LastOrDefault();
            string result = null;
            if (RED.Contains(lastNum))
            {
                result = "红波";
            }
            else if (BLUE.Contains(lastNum))
            {
                result = "蓝波";
            }
            else if (GREEN.Contains(lastNum))
            {
                result = "绿波";
            }
            decimal reward = decimal.Zero;
            if (input.Split(',').Contains(result)) reward = this.GetRewardMoney(rewardMoney);
            return reward;
        }

        public override decimal RewardMoney => 6M;
    }
}
