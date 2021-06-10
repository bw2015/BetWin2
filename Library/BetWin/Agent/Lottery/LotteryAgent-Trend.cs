using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data;

using BW.Common.Lottery;
using SP.Studio.Core;
using SP.Studio.Data;

namespace BW.Agent
{
    /// <summary>
    /// 走势图逻辑
    /// </summary>
    partial class LotteryAgent
    {
        /// <summary>
        /// 创建一个遗漏数据（XML格式）
        /// </summary>
        /// <param name="type">彩种</param>
        /// <param name="index">期号</param>
        /// <param name="number">开奖号码</param>
        /// <param name="siteId">所属站点（系统彩必填）</param>
        public bool CreateTrend(LotteryType type, string index, string number, int siteId = 0)
        {
            int numberIndex = 0;
            string[] resultNumber = number.Split(',');
            string[] ball = type.GetCategory().Cate.GetAttribute<LotteryCategoryAttribute>().Number;
            if (ball == null) return false;

            LotteryTrend trend = BDC.LotteryTrend.Where(t => t.Type == type && t.SiteID == siteId).OrderByDescending(t => t.Index).FirstOrDefault()
                ?? new LotteryTrend()
                {
                    Type = type,
                    Index = index,
                    Number = number,
                    SiteID = siteId
                };


            foreach (string num in resultNumber)
            {
                foreach (string n in ball)
                {
                    string name = string.Format("N{0}-{1}", numberIndex, n);
                    trend.SaveElement(name, n == num ? 0 : trend.GetElement<int>(name) + 1);
                }
                numberIndex++;
            }
            // 分布遗漏
            foreach (string n in ball)
            {
                string name = string.Format("D{0}", n);
                trend.SaveElement(name, resultNumber.Contains(n) ? 0 : trend.GetElement<int>(name) + 1);
            }

            trend.Index = index;
            trend.Number = number;

            using (DbExecutor db = NewExecutor())
            {
                try
                {
                    return trend.Add(db);
                }
                catch (Exception ex)
                {
                    SystemAgent.Instance().AddErrorLog(siteId, ex, "创建走势图数据出错");
                    base.Message(ex.Message);
                    return false;
                }
            }
        }
    }
}
