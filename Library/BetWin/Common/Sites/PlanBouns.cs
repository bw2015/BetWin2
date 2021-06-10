using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SP.Studio.Array;

namespace BW.Common.Sites
{
    /// <summary>
    /// 总代分红活动（兼容二级分红）
    /// </summary>
    public struct PlanBouns
    {
        public PlanBouns(DataRow dr)
        {
            this.UserID = (int)dr["UserID"];
            this.Member = (int)dr["Member"];
            this.Sales = (decimal)dr["Sales"];
            this.Money = (decimal)dr["Money"];
        }

        /// <summary>
        /// 代理用户
        /// </summary>
        public int UserID;

        /// <summary>
        /// 有效会员
        /// </summary>
        public int Member;

        /// <summary>
        /// 销售额
        /// </summary>
        public decimal Sales;

        /// <summary>
        /// 亏损金额（负数为盈利）
        /// </summary>
        public decimal Money;

        /// <summary>
        /// 根据配置项目获取可以得到奖金
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public decimal GetBouns(Dictionary<string, decimal> setting, Dictionary<string, decimal> contractSetting = null)
        {
            if (this.Money >= decimal.Zero) return decimal.Zero;

            if (contractSetting != null)
            {
                foreach (KeyValuePair<string, decimal> item in contractSetting)
                {
                    if (setting.ContainsKey(item.Key))
                    {
                        setting[item.Key] = item.Value;
                    }
                    else
                    {
                        setting.Add(item.Key, item.Value);
                    }
                }
            }

            int rule = (int)setting.Get("Rule", decimal.Zero);

            decimal bouns = decimal.Zero;
            //0：需满足所有条件 1：一到三等级日量亏损二者满足其一，等级三以上满足所有条件
            for (int i = 0; i < 20; i++)
            {
                decimal money = setting.Get("Money" + i, decimal.Zero);
                decimal sale = setting.Get("Sale" + i, decimal.Zero);
                int user = (int)setting.Get("User" + i, decimal.Zero);
                decimal agent = setting.Get("Agent" + i, decimal.Zero);
                if (agent == decimal.Zero) continue;
                decimal _bouns = decimal.Zero;
                switch (rule)
                {
                    case 0:
                        if (Math.Abs(this.Money) >= money && this.Sales >= sale && this.Member >= user) _bouns = Math.Abs(this.Money) * agent;
                        break;
                    case 1:
                        if (i < 4)
                        {
                            if ((Math.Abs(this.Money) >= money || this.Sales >= sale) && this.Member >= user) _bouns = Math.Abs(this.Money) * agent;
                        }
                        else
                        {
                            if (Math.Abs(this.Money) >= money && this.Sales >= sale && this.Member >= user) _bouns = Math.Abs(this.Money) * agent;
                        }
                        break;
                }
                if (_bouns > bouns) bouns = _bouns;
            }
            return bouns;
        }

        public override string ToString()
        {
            List<string> sb = new List<string>();
            if (Sales != decimal.Zero) sb.Add(string.Format("销售额:{0}元", this.Sales.ToString("n")));
            if (Money != decimal.Zero) sb.Add(string.Format("亏损:{0}元", this.Money.ToString("n")));
            if (Member != 0) sb.Add(string.Format("有效会员:{0}人", this.Member));
            return string.Join(" ", sb);
        }
    }
}
