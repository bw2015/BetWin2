using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;
using BW.Common.Lottery.Limited;

namespace BW.Agent
{
    /// <summary>
    /// 彩票的限号管理
    /// </summary>
    partial class LotteryAgent
    {
        public static List<SiteLimited> limitedList = new List<SiteLimited>();

        /// <summary>
        /// 获取一个彩期的限号对象（如果不存在则新建一个）
        /// </summary>
        /// <returns></returns>
        public SiteLimited GetSiteLimited(LotteryType game, string index, LimitedType type)
        {
            SiteLimited siteLimited = limitedList.Find(t => t.SiteID == SiteInfo.ID && t.Game == game && t.Type == type && t.Index == index);
            if (siteLimited.SiteID == 0)
            {
                siteLimited = new SiteLimited(SiteInfo.ID, game, index, type);
                limitedList.Add(siteLimited);
            }
            return siteLimited;
        }

        /// <summary>
        /// 检查号码是否符合限号规则
        /// </summary>
        /// <param name="siteLimited">限号对象</param>
        /// <param name="numberList">投注号码列表</param>
        /// <param name="money">奖金</param>
        /// <param name="maxReward">设定的封锁值</param>
        /// <returns></returns>
        public bool CheckLimitedNumber(SiteLimited siteLimited, IEnumerable<string> numberList, decimal money, decimal maxReward)
        {
            if (maxReward == decimal.Zero) return true;
            foreach (string number in numberList)
            {
                if (!siteLimited.Number.ContainsKey(number))
                {
                    siteLimited.Number.Add(number, decimal.Zero);
                }
                if (siteLimited.Number[number] + money > maxReward)
                {
                    base.Message("投注号码[{0}]已限号", number);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 撤单减去该订单所占用的额度
        /// </summary>
        /// <param name="siteLimited"></param>
        /// <param name="number"></param>
        /// <param name="money">奖金</param>
        public void RevokeLimitedNumber(SiteLimited siteLimited, string number, decimal money)
        {
            if (!siteLimited.Number.ContainsKey(number)) return;
            siteLimited.Number[number] -= money;
        }

        /// <summary>
        /// 把检查之后的号码加入列队当中
        /// </summary>
        /// <param name="siteLimited"></param>
        /// <param name="numberList"></param>
        /// <param name="money"></param>
        public void AddLimitedNumber(SiteLimited siteLimited, IEnumerable<string> numberList, decimal money)
        {
            foreach (string number in numberList)
            {
                siteLimited.Number[number] += money;
            }
        }


        /// <summary>
        /// 获取当前站点已经设置的限号策略
        /// </summary>
        /// <returns></returns>
        public List<LimitedSetting> GetLimitedSettingList()
        {
            return BDC.LimitedSetting.Where(t => t.SiteID == SiteInfo.ID && t.Money > decimal.Zero).ToList();
        }
    }
}
