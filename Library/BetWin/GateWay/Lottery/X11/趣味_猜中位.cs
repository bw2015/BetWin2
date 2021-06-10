using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 猜中位
    /// </summary>
    public class Player72 : IX11
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "03", "04", "05", "06", "07", "08", "09" };
            }
        }
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber[0].Length;
        }

        Dictionary<string, decimal> reardNumber = new Dictionary<string, decimal>();

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[] resultNumber = number.Split(',').OrderBy(t => t).ToArray();

            string result = resultNumber.Skip(2).FirstOrDefault();

            string[][] inputNumber = input.GetInputNumber();

            if (inputNumber[0].Contains(result))
            {
                decimal zoom = 1M;
                switch (result)
                {
                    case "03":
                    case "09":
                        zoom = 0.28M;
                        break;
                    case "04":
                    case "08":
                        zoom = 0.63M;
                        break;
                    case "05":
                    case "07":
                        zoom = 0.9M;
                        break;
                }

                return this.GetRewardMoney(rewardMoney) / zoom;
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 9.24M;
            }
        }
    }
}
