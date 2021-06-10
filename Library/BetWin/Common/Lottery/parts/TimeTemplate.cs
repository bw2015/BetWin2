using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BW.Common.Lottery
{
    partial class TimeTemplate
    {

        /// <summary>
        /// 当前彩期
        /// </summary>
        public string LotteryIndex
        {
            get
            {
                return this.Index.ToString().PadLeft(this.Type.GetCategory().IndexLength, '0');
            }
        }

        /// <summary>
        /// 获取用秒表示的当前时间
        /// </summary>
        public int Seconds
        {
            get
            {
                return this.Time;
            }
        }

    }
}
