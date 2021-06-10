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
    /// 单层分红的统计数据结构
    /// </summary>
    public struct PlanSingleBouns
    {
        public PlanSingleBouns(DataRow dr)
        {
            this.UserID = (int)dr["UserID"];
            this.Sales = (decimal)dr["Sales"];
            this.MemberCount = (int)dr["MemberCount"];
            this.Money = (decimal)dr["Money"];
        }

        /// <summary>
        /// 团队
        /// </summary>
        public int UserID;

        /// <summary>
        /// 销量
        /// </summary>
        public decimal Sales;

        /// <summary>
        /// 有效用户
        /// </summary>
        public int MemberCount;

        /// <summary>
        /// 亏损金额
        /// </summary>
        public decimal Money;

        /// <summary>
        /// 获取当前可获得分红金额
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public decimal GetBouns(Dictionary<string, decimal> setting)
        {
            int type = (int)setting.Get("Type", decimal.Zero);
            decimal bouns = decimal.Zero;
            decimal userMoney = this.Money * -1;
            if (userMoney <= decimal.Zero) return decimal.Zero;
            for (int i = 1; i < 16; i++)
            {
                decimal agent = setting.Get("Agent" + i, decimal.Zero);
                if (agent == decimal.Zero) continue;

                decimal money = setting.Get("Money" + i, decimal.Zero);
                int user = (int)setting.Get("User" + i, decimal.Zero);
                decimal sales = setting.Get("Sales" + i, decimal.Zero);

                switch (type)
                {
                    case 0: // 满足所有条件
                        if (userMoney < money || this.MemberCount < user || this.Sales < sales) continue;
                        break;
                    case 1: // 满足销售额或者亏损额两者之一的条件
                        if (this.MemberCount < user || (userMoney < money && this.Sales < sales)) continue;
                        break;
                }

                if (userMoney * agent > bouns) bouns = userMoney * agent;
            }
            return bouns;
        }
    }
}
