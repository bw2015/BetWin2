using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using System.Web;
using System.Web.Caching;

using SP.Studio.Data;
using BW.Common.Lottery;
using BW.GateWay.Lottery;
using SP.Studio.Core;
using BW.Framework;

namespace BW.Agent
{
    /// <summary>
    /// 彩票开奖
    /// </summary>
    partial class LotteryAgent
    {
        /// <summary>
        /// 上一次开奖的期号
        /// </summary>
        private Dictionary<string, string> lastOpenIndex = new Dictionary<string, string>();

        /// <summary>
        /// 保存开奖结果
        /// </summary>
        /// <param name="type">彩种</param>
        /// <param name="index">彩期</param>
        /// <param name="number">号码</param>
        /// <returns></returns>
        public bool SaveResultNumber(LotteryType type, string index, string number)
        {
            if (this.SaveResultNumber(SiteInfo == null ? 0 : SiteInfo.ID, type, index, number))
            {
                if (AdminInfo != null) AdminInfo.Log(Common.Admins.AdminLog.LogType.Lottery, "【{0}】第{1}期手工开奖，号码：{2}", type.GetDescription(), index, number);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 系统彩种手工开奖
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public bool SaveSiteResultNumber(LotteryType type, string index, string number)
        {
            LotteryAttribute category = type.GetCategory();
            if (category.SiteLottery)
            {
                base.Message("当前彩种为系统彩种");
                return false;
            }
            LotteryCategoryAttribute lotteryCategory = category.Cate.GetAttribute<LotteryCategoryAttribute>();
            if (!lotteryCategory.IsMatch(number))
            {
                base.Message("号码不符合规则");
                return false;
            }
            DateTime resultAt = Utils.GetLotteryTime(type, index);
            using (DbExecutor db = NewExecutor())
            {
                int result = db.ExecuteNonQuery(CommandType.StoredProcedure, "SaveSiteResultNumber",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@Type", type),
                    NewParam("@Index", index),
                    NewParam("@Number", number),
                    NewParam("@ResultAt", Utils.GetLotteryTime(type, index)));

                if (result == 0)
                {
                    base.Message("当前期已经开奖");
                    return false;
                }
            }
            Dictionary<string, string> dic = this.GetSiteResultNumber(type);
            if (dic.ContainsKey(index))
            {
                dic[index] = number;
            }
            else
            {
                dic.Add(index, number);
            }
            AdminInfo.Log(Common.Admins.AdminLog.LogType.Lottery, "【{0}】第{1}期手工开奖，号码：{2}", type.GetDescription(), index, number);
            return true;
        }

        /// <summary>
        /// 保存开奖结果（兼容系统彩）
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public bool SaveResultNumber(int siteId, LotteryType type, string index, string number)
        {
            if (string.IsNullOrEmpty(index))
            {
                base.Message("奖期不能为空");
                return false;
            }
            if (string.IsNullOrEmpty(number))
            {
                base.Message("开奖号码不能为空");
                return false;
            }
            LotteryAttribute category = type.GetCategory();
            if (category.SiteLottery && siteId == 0)
            {
                base.Message("系统彩站点不能为0");
                return false;
            }
            LotteryCategoryAttribute lotteryCategory = category.Cate.GetAttribute<LotteryCategoryAttribute>();
            if (!lotteryCategory.IsMatch(number))
            {
                base.Message("号码不符合规则");
                return false;
            }

            // 上一次开奖的Key，避免重复采集开奖（执行本方法之前一定要先执行 IsNeedOpen 方法）
            string key = string.Concat(siteId, "-", type);

            DateTime resultAt = siteId == 0 || type.GetCategory().BuildIndex ? DateTime.Now : Utils.GetLotteryTime(type, index);
            if (type.GetCategory().Delay != 0) resultAt = resultAt.AddSeconds(type.GetCategory().Delay);
            if (resultAt < DateTime.Now) resultAt = DateTime.Now;

            bool success = false;
            using (DbExecutor db = NewExecutor())
            {
                if (db.ExecuteNonQuery(CommandType.StoredProcedure, "SaveResultNumber",
                    NewParam("@SiteID", category.SiteLottery ? siteId : 0),
                    NewParam("@Type", type),
                    NewParam("@Index", index),
                    NewParam("@Number", number),
                    NewParam("@ResultAt", resultAt)) == 1)
                {
                    lastOpenIndex[key] = index;
                    success = true;
                }
            }
            if (success)
            {
                this.CreateTrend(type, index, number, category.SiteLottery ? siteId : 0);
            }
            return success;
        }

        /// <summary>
        /// 查看当前是否需要开奖
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="index">当前期</param>
        /// <returns></returns>
        public bool IsNeedOpen(int siteId, LotteryType type, out string index)
        {
            string key = string.Concat(siteId, "-", type);
            if (!lastOpenIndex.ContainsKey(key)) lastOpenIndex.Add(key, this.GetLastIndex(siteId, type));
            string lastIndex = lastOpenIndex[key];

            if (type.GetCategory().BuildIndex)
            {
                using (DbExecutor db = NewExecutor())
                {
                    index = (string)db.ExecuteScalar(CommandType.Text, "SELECT [Index] FROM lot_Order WHERE SiteID = @SiteID AND Type = @Type AND IsLottery = 0 AND Status = @Status AND ResultNumber = ''",
                        NewParam("@SiteID", siteId),
                        NewParam("@Type", type),
                        NewParam("@Status", LotteryOrder.OrderStatus.Normal));
                    return !string.IsNullOrEmpty(index);
                }
            }

            int time;
            index = Utils.GetLotteryIndex(type, out time);
            if (string.IsNullOrEmpty(index)) return false;
            if (index != lastIndex)
            {
                if (BDC.LotterySetting.Where(t => t.SiteID == siteId && t.Game == type && !t.IsManual && t.IsOpen).Count() == 0) return false;

                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取彩种最后一次开奖的期号
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetLastIndex(int siteId, LotteryType type)
        {
            return BDC.SiteNumber.Where(t => t.SiteID == siteId && t.Type == type).OrderByDescending(t => t.ResultAt).Select(t => t.Index).FirstOrDefault();
        }

        /// <summary>
        /// 站点自有彩种开奖
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="number">开奖号码</param>
        /// <param name="gamePlayer">玩法（只有自有小游戏才需要）</param>
        /// <returns></returns>
        public bool OpenResultNumber(int siteId, LotteryType type, string index, out string number, IPlayer gamePlayer = null)
        {
            number = null;
            if (!type.GetCategory().SiteLottery) { base.Message("非系统彩种"); return false; }

            decimal money = decimal.Zero;
            decimal reward = decimal.Zero;

            // 开奖号码
            number = type.GetCategory().CreateNumber(gamePlayer);
            if (string.IsNullOrEmpty(number)) return false;

            LotterySetting setting;

            // 待开奖的订单列表
            List<LotteryOrder> orderList;

            //#1 获取当前彩种24小时内的输赢概率
            using (DbExecutor db = NewExecutor())
            {
                setting = this.GetLotterySettingInfo(db, siteId, type);
                // 如果是手工开奖则直接返回
                if (setting.IsManual) return false;
                // 如果不开放彩种则随机开奖
                if (!setting.IsOpen || setting.RewardPercent == decimal.Zero)
                {
                    SystemAgent.Instance().AddSystemLog(siteId, string.Format("{0}第{1}期随机开奖：{1}", type, index, number));
                    return this.SaveResultNumber(siteId, type, index, number);
                }
                //獲取當前中獎概率
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetSiteLottery",
                    NewParam("@SiteID", siteId),
                    NewParam("@Type", type),
                    NewParam("@Index", index));

                money = (decimal)ds.Tables[0].Rows[0]["Money"];
                reward = (decimal)ds.Tables[0].Rows[0]["Reward"];

                orderList = new List<LotteryOrder>();
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    orderList.Add(new LotteryOrder(dr));
                }
            }

            if (orderList.Count == 0)
            {
                SystemAgent.Instance().AddSystemLog(siteId, string.Format("{0}第{1}期无人投注,随机开奖：{1}", type, index, number));
                return this.SaveResultNumber(siteId, type, index, number);
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}第{1}期，预设返奖率：{2}", type, index, (setting.RewardPercent / 100M).ToString("P")).AppendLine();
            Dictionary<string, decimal> percent = new Dictionary<string, decimal>();
            percent.Add(number, decimal.Zero);

            // 当前返奖率
            decimal currentPercent = money == decimal.Zero ? decimal.Zero : reward / money;
            // 如果当前返奖率小于预设返奖率则随机开奖
            if (currentPercent < setting.RewardPercent / 100M)
            {
                sb.AppendFormat("当前返奖率：{0}小于预设返奖率", currentPercent.ToString("P")).AppendLine();
                sb.AppendFormat("随机开奖：{0}", number);
                SystemAgent.Instance().AddSystemLog(siteId, sb.ToString());
                return this.SaveResultNumber(siteId, type, index, number);
            }

            for (int i = 0; i < 9; i++)
            {
                number = type.GetCategory().CreateNumber(gamePlayer);
                if (!percent.ContainsKey(number)) percent.Add(number, decimal.Zero);
            }

            string[] _percentNumber = percent.Select(t => t.Key).ToArray();
            foreach (string _number in _percentNumber)
            {
                decimal _money = money;
                decimal _reward = reward;
                orderList.ForEach(t =>
                {
                    LotteryPlayer player = this.GetPlayerInfo(t.PlayerID);
                    decimal orderReward = PlayerFactory.GetPlayer(player.Code, out type).Reward(t.Number, _number) * t.Mode * t.Times * t.Rebate / 2000M;
                    _money += t.Money;
                    _reward += orderReward < decimal.Zero ? t.Money : orderReward;
                });

                percent[_number] = _reward / _money;
                sb.AppendFormat("开奖号码：{0} 返奖率：{1}", _number, percent[_number].ToString("P"))
                    .AppendLine();
            }

            var resultList = percent.Where(t => t.Value < setting.RewardPercent / 100M);

            if (resultList.Count() != 0)
            {
                number = resultList.OrderByDescending(t => t.Value).First().Key;
            }
            else
            {
                number = percent.OrderBy(t => Math.Abs(t.Value - setting.RewardPercent / 100M)).First().Key;
            }

            sb.AppendFormat("最终开奖号码：{0}", number);

            SystemAgent.Instance().AddSystemLog(siteId, sb.ToString());

            return this.SaveResultNumber(siteId, type, index, number);
        }

        /// <summary>
        /// 自定义开奖的官方号码
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetSiteResultNumber(LotteryType type)
        {
            if (type.GetCategory().SiteLottery) return null;
            string key = string.Format("{0}-{1}", SiteInfo.ID, type);
            Dictionary<string, string> list = (Dictionary<string, string>)HttpRuntime.Cache[key];
            if (list == null)
            {
                list = BDC.SiteResultNumber.Where(t => t.SiteID == SiteInfo.ID && t.Type == type).OrderByDescending(t => t.ResultAt).Take(120).ToList().ToDictionary(t => t.Index, t => t.Number);
                HttpRuntime.Cache.Insert(key, list, BetModule.SiteCacheDependency);
            }
            return list;
        }

        /// <summary>
        /// 把自定义开奖号码覆盖官方号码
        /// </summary>
        /// <param name="list"></param>
        public void GetSiteResultNumber(LotteryType type, ref List<ResultNumber> list)
        {
            Dictionary<string, string> dic = this.GetSiteResultNumber(type);
            if (dic == null) return;
            list.ForEach(t =>
            {
                if (dic.ContainsKey(t.Index)) t.Number = dic[t.Index];
            });
        }

        /// <summary>
        /// 获取试算的盈亏号码
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <returns>号码/盈亏</returns>
        public Dictionary<string, decimal> GetLotteryTestNumber(int siteId, LotteryType type, string index)
        {
            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            // 待开奖的订单列表
            IEnumerable<LotteryOrder> orderList = BDC.LotteryOrder.Where(t => t.SiteID == siteId && t.Type == type && t.Index == index).AsEnumerable();
            for (int i = 0; i < 9; i++)
            {
                string number = type.GetCategory().CreateNumber();
                if (!data.ContainsKey(number)) data.Add(number, decimal.Zero);
            }
            foreach (string _number in data.Select(t => t.Key).ToArray())
            {
                decimal _money = decimal.Zero;
                decimal _reward = decimal.Zero;
                foreach (LotteryOrder order in orderList)
                {
                    LotteryPlayer player = this.GetPlayerInfo(order.PlayerID);
                    decimal orderReward = PlayerFactory.GetPlayer(player.Code, out type).Reward(order.Number, _number) / 2M * order.Mode * order.Times * order.Rebate / 2000M;
                    _money += order.Money;
                    _reward += orderReward;
                }

                data[_number] = _reward - _money;
            }
            return data;
        }
    }
}
