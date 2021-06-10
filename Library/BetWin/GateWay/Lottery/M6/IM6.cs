using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.M6
{
    public abstract class IM6 : IPlayer
    {
        public override LotteryCategory Type
        {
            get { return LotteryCategory.M6; }
        }

        /// <summary>
        /// 生肖
        /// </summary>
        protected virtual string[] Lunar
        {
            get
            {
                return new string[] { "鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊", "猴", "鸡", "狗", "猪" };
            }
        }

        /// <summary>
        /// 红波
        /// </summary>
        protected virtual string[] Ball_Red
        {
            get
            {
                return new string[] { "01", "02", "07", "08", "12", "13", "18", "19", "23", "24", "29", "30", "34", "35", "40", "45", "46" };
            }
        }

        /// <summary>
        /// 蓝波
        /// </summary>
        protected virtual string[] Ball_Blue
        {
            get
            {
                return new string[] { "03", "04", "09", "10", "14", "15", "20", "25", "26", "31", "36", "37", "41", "42", "47", "48" };
            }
        }

        /// <summary>
        /// 绿波
        /// </summary>
        protected virtual string[] Ball_Green
        {
            get
            {
                return new string[] { "05", "06", "11", "16", "17", "21", "22", "27", "28", "32", "33", "38", "39", "43", "44", "49" };
            }
        }

        /// <summary>
        /// 开奖号码
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        protected virtual string[] GetNumber(string number)
        {
            return number.Split(',');
        }

        /// <summary>
        /// 当年的生肖
        /// </summary>
        protected virtual int LunarIndex
        {
            get
            {
                return (DateTime.Now.Year - 2008) % 12;
            }
        }

        /// <summary>
        /// 获取号码所属的生肖
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        protected virtual string GetLunar(string num)
        {
            int index = this.LunarIndex;
            for (var i = 1; i <= 49; i++)
            {
                int indexNumber = (index - (i - 1) + 120) % 12;
                string value = i.ToString().PadLeft(2, '0');
                if (value == num) return this.Lunar[indexNumber];
            }
            return num;
        }

        public override bool IsMatch(string input)
        {
            return this.InputBall.Contains(input);
        }

        public override int Bet(string input)
        {
            return this.IsMatch(input) ? 1 : 0;
        }
    }
}
