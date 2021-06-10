using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Common.Lottery
{
    partial class SiteNumber
    {
        /// <summary>
        /// 转化成为官方彩的开奖对象
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static implicit operator ResultNumber(SiteNumber siteNumber)
        {
            if (siteNumber == null) return null;
            return new ResultNumber()
            {
                Index = siteNumber.Index,
                IsLottery = true,
                Number = siteNumber.Number,
                ResultAt = siteNumber.ResultAt,
                Type = siteNumber.Type
            };
        }
    }
}
