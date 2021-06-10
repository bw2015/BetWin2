using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SP.Studio.Core;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 时时彩玩法基类
    /// </summary>
    public abstract class IX5 : IPlayer
    {
        /// <summary>
        /// 当前彩票类型
        /// </summary>
        public override LotteryCategory Type
        {
            get { return LotteryCategory.X5; }
        }

        protected virtual NumberRange NumberType
        {
            get { return NumberRange.Star5; }
        }

        protected virtual string[] GetNumber(string number)
        {
            return this.GetNumber(number, this.NumberType);
        }

        /// <summary>
        /// 获取开奖号码
        /// </summary>
        /// <param name="number"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected string[] GetNumber(string number, NumberRange type)
        {
            string[] resultNumber = number.Split(',');
            switch (type)
            {
                case NumberRange.Star4:
                    resultNumber = resultNumber.Skip(1).ToArray();
                    break;
                case NumberRange.Star41:
                    resultNumber = resultNumber.Take(4).ToArray();
                    break;
                case NumberRange.Star31:
                    resultNumber = resultNumber.Take(3).ToArray();
                    break;
                case NumberRange.Star32:
                    resultNumber = resultNumber.Skip(1).Take(3).ToArray();
                    break;
                case NumberRange.Star33:
                    resultNumber = resultNumber.Skip(2).Take(3).ToArray();
                    break;
                case NumberRange.Star21:
                    resultNumber = resultNumber.Take(2).ToArray();
                    break;
                case NumberRange.Star22:
                    resultNumber = resultNumber.Skip(resultNumber.Length - 2).Take(2).ToArray();
                    break;
            }
            return resultNumber;
        }

        /// <summary>
        /// 单式是否中奖
        /// </summary>
        /// <param name="input">投注号码</param>
        /// <param name="number">开奖号码</param>
        /// <param name="ignoreOrder">忽略排序</param>
        /// <returns></returns>
        protected virtual bool IsSingleReward(string input, string number, bool ignoreOrder = false)
        {
            if (ignoreOrder)
            {
                input = string.Join("|", input.Split('|').Select(t => string.Join(",", t.Split(',').OrderBy(p => p))));
                number = string.Join(",", this.GetNumber(number, this.NumberType).OrderBy(t => t));
            }
            else
            {
                number = string.Join(",", this.GetNumber(number, this.NumberType));
            }
            return input.Split('|').Contains(number);
        }

        protected enum NumberRange
        {
            /// <summary>
            /// 五星
            /// </summary>
            Star5,
            /// <summary>
            /// 四星（后四）
            /// </summary>
            Star4,
            /// <summary>
            /// 前四
            /// </summary>
            Star41,
            /// <summary>
            /// 前三
            /// </summary>
            Star31,
            /// <summary>
            /// 中三
            /// </summary>
            Star32,
            /// <summary>
            /// 后三
            /// </summary>
            Star33,
            /// <summary>
            /// 前二
            /// </summary>
            Star21,
            /// <summary>
            /// 后二
            /// </summary>
            Star22
        }

    }
}
