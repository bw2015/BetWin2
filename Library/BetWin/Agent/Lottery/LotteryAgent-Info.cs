using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;
using BW.Framework;

namespace BW.Agent
{
    /// <summary>
    /// 彩票页面的信息 （请求来自 controls/lottery-info.html)
    /// </summary>
    partial class LotteryAgent
    {
        /// <summary>
        /// 获取最近开奖的结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ResultNumber> GetResultNumber(LotteryType type, int count)
        {
            List<ResultNumber> list;
            if (type.GetCategory().SiteLottery)
            {
                list = BDC.SiteNumber.Where(t => t.SiteID == SiteInfo.ID && t.Type == type && t.ResultAt < DateTime.Now).OrderByDescending(t => t.ResultAt).Take(count).ToList().ConvertAll(t => (ResultNumber)t);
            }
            else
            {
                list = BDC.ResultNumber.Where(t => t.Type == type && t.ResultAt < DateTime.Now).OrderByDescending(t => t.ResultAt).Take(count).ToList();
                this.GetSiteResultNumber(type, ref list);
            }
            return list;
        }

        /// <summary>
        /// 获取指定期号的开奖结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetResultNumber(LotteryType type, string index)
        {
            if (type.GetCategory().SiteLottery)
            {
                return BDC.SiteNumber.Where(t => t.SiteID == SiteInfo.ID && t.Index == index && t.Type == type).Select(t => t.Number).FirstOrDefault();
            }
            else
            {
                return BDC.ResultNumber.Where(t => t.Type == type && t.Index == index).Select(t => t.Number).FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取开奖结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public ResultNumber GetResultNumberInfo(LotteryType type, string index)
        {
            if (type.GetCategory().SiteLottery)
            {
                return BDC.SiteNumber.Where(t => t.SiteID == SiteInfo.ID && t.Index == index && t.Type == type).FirstOrDefault();
            }
            else
            {
                return BDC.ResultNumber.Where(t => t.Type == type && t.Index == index).FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取当前期号之后的指定期号
        /// </summary>
        /// <param name="type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ResultNumber> GetResultIndex(LotteryType type, int count)
        {
            return Utils.GetLotteryIndex(type, count).ToList();
        }

        /// <summary>
        /// 获取所有开放了当前彩种的站点列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<int> GetOpenSiteList(LotteryType type)
        {
            return BDC.LotterySetting.Where(t => t.Game == type && t.IsOpen).Select(t => t.SiteID).ToList();
        }
    }
}
