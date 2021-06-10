using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SP.Studio.Array;

namespace BW.GateWay.Planning
{
    /// <summary>
    /// 单级分红
    /// </summary>
    public class SingleBonus : IPlan
    {
        public SingleBonus() : base() { }

        public SingleBonus(XElement root) : base(root) { }

        /// <summary>
        /// 返回可分红的比例
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override decimal GetValue(params decimal[] values)
        {
            decimal percent = decimal.Zero;
            //<Money1 Name = "亏损金额1" Description="要求的亏损业绩">0</Money1>
            //<Sale1 Name = "销售业绩1" Description="要求的销售业绩">0</Sale1>
            //<User1 Name = "有效人数1" Description="要求达到的有效人数">0</User1>
            //<Agent1 Name = "分红比例1" Description="1级别的分红比例">0</Agent1>

            decimal moneyValue = values[0];
            decimal saleValue = values[1];
            decimal userValue = values[2];

            int index = 0;
            while (true)
            {
                index++;
                if (!this.Value.ContainsKey("Agent" + index)) break;
                decimal money = this.Value.Get("Money" + index, decimal.Zero);
                decimal sale = this.Value.Get("Sale" + index, decimal.Zero);
                decimal user = this.Value.Get("User" + index, decimal.Zero);
                decimal agent = this.Value["Agent" + index];

                if (moneyValue >= money && saleValue >= sale && userValue >= user)
                {
                    if (percent < agent) percent = agent;
                }
            }

            return percent;
        }
    }
}
