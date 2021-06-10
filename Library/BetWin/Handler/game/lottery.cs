using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Web;
using System.Xml.Linq;

using BW;
using BW.Agent;
using BW.Framework;
using BW.Common.Lottery;
using BW.Common.Users;

namespace BW.Handler.game
{
    /// <summary>
    /// 彩种
    /// </summary>
    public class lottery : IHandler
    {
        /// <summary>
        /// 获取当前可投注的彩期以及当前开奖的奖期
        /// </summary>
        [Guest]
        private void index(HttpContext context)
        {
            LotteryType game = WebAgent.GetParam("Game", "ChungKing").ToEnum<LotteryType>();
            if (game.GetCategory().BuildIndex)
            {
                context.Response.Write(false, "没有彩期", new
                {
                    BuildIndex = true
                });
            }
            if (!SysSetting.GetSetting().LotteryTimeTemplate.ContainsKey(game))
            {
                context.Response.Write(false, "当前彩种没有固定彩期");
            }

            StringBuilder sb = new StringBuilder();
            int betTime, openTime;
            string openIndex = Utils.GetLotteryIndex(game, out openTime);

            string openNumber = SysSetting.GetSetting().GetResultNumber(game, openIndex);
            LotterySetting setting = SiteInfo.LotteryList.FirstOrDefault(t => t.Game == game);
            if (setting == null || (setting.IsManual && !game.GetCategory().SiteLottery))
            {
                openNumber = null;
            }

            sb.Append("{")
                .AppendFormat("\"Game\":\"{0}\",", game)
                .AppendFormat("\"ServerTime\":{0},", WebAgent.GetTimeStamps())
                .AppendFormat("\"OpenIndex\":\"{0}\",", openIndex)
                .AppendFormat("\"OpenNumber\":\"{0}\",", openNumber)
                .AppendFormat("\"OpenTime\":{0},", openTime)
                .AppendFormat("\"BetIndex\":\"{0}\",", Utils.GetLotteryBetIndex(game, out betTime)) // 当前可投注期
                .AppendFormat("\"BetTime\":{0}", betTime)
                .Append("}");

            context.Response.Write(true, this.StopwatchMessage(context), sb.ToString());
        }

        /// <summary>
        /// 获取指定期号的开奖结果
        /// </summary>
        /// <param name="context"></param>
        private void indexresult(HttpContext context)
        {
            LotteryType game = WebAgent.GetParam("Game").ToEnum<LotteryType>();
            string index = WebAgent.QF("index");
            string number = LotteryAgent.Instance().GetResultNumber(game, index);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Game = game,
                Index = index,
                Number = number
            });
        }

        /// <summary>
        /// 内存奖期测试
        /// </summary>
        /// <param name="context"></param>
        private void indextest(HttpContext context)
        {
            IEnumerable<string> list = SysSetting.GetSetting().ResultNumber.Select(t =>
            {
                return string.Concat("\"", t.Key, "\":{", string.Join(",", t.Value.Select(p => string.Format("\"{0}\":\"{1}\"", p.Key, p.Value))), "}");
            });
            context.Response.Write(true, this.StopwatchMessage(context), string.Concat("{", string.Join(",", list), "}"));
        }

        /// <summary>
        /// 从当前期算起120期
        /// </summary>
        /// <param name="context"></param>
        private void numberindex(HttpContext context)
        {
            LotteryType type = QF("Type").ToEnum<LotteryType>();
            int count = Math.Max(120, QF("Count", 120));
            List<ResultNumber> list = LotteryAgent.Instance().GetResultIndex(type, Math.Max(1, count));
            int index = 1;
            context.Response.Write(true, type.GetDescription(), this.ShowResult(list, t => new
            {
                ID = index++,
                t.Index,
                t.ResultAt
            }));
        }

        /// <summary>
        /// 查看当前限号详情
        /// </summary>
        /// <param name="context"></param>
        private void limited(HttpContext context)
        {
            context.Response.Write(true, "查看限号详情",
                this.ShowResult(LotteryAgent.limitedList.Where(t => t.Game == WebAgent.GetParam("Game").ToEnum<LotteryType>()), t => new
                {
                    t.Game,
                    t.Index,
                    Number = new JsonString(t.Number.ToJson()),
                    t.Type
                }));
        }

        /// <summary>
        /// 服务端的开奖服务（也能用于被前端调用，如秒秒彩）
        /// </summary>
        /// <param name="context"></param>
        private void open(HttpContext context)
        {
            int id = QF("ID", 0);
            if (id == 0)
            {
                context.Response.Write(false, "编号错误");
            }
            string lockId = string.Concat("OPENREWARD-", QF("ID"));

            lock (lockId)
            {
                this.ShowResult(context, LotteryAgent.Instance().OpenReward(id), "开奖成功", new
                {
                    Time = this.StopwatchMessage(context)
                });
            }
        }

        /// <summary>
        /// 待开奖列表
        /// </summary>
        /// <param name="context"></param>
        private void openlist(HttpContext context)
        {
            List<RewardOrder> list = LotteryAgent.Instance().GetRewardOrderList();
            if (list.Count == 0)
            {
                context.Response.Write(false, "当前没有待开奖订单");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new JsonString(string.Concat("{", string.Join(",", list.Select(t => string.Format("\"{0}\":\"{1}\"", t.OrderID, t.Number))), "}")));
        }

        /// <summary>
        /// 中奖名单
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void rewardtip(HttpContext context)
        {
            // 最低的名单
            decimal money = QF("Money", 100M);
            int count = QF("Count", 20);

            List<RewardTip> list = LotteryAgent.Instance().GetRewardTip(money, count);

            context.Response.Write(true, this.StopwatchMessage(context), "[" + string.Join(",", list.Select(t => t.ToString())) + "]");
        }

        #region ==========  彩票页面的信息列表 来自 controls/lottery-info.html ==============

        /// <summary>
        /// 开奖号码
        /// </summary>
        /// <param name="context"></param>
        private void resultnumber(HttpContext context)
        {
            if (string.IsNullOrEmpty(QF("Game")))
            {
                context.Response.Write(false, "未指定彩种");
            }
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            List<ResultNumber> list = LotteryAgent.Instance().GetResultNumber(game, Math.Max(10, QF("count", 10)));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Index,
                t.Number,
                t.ResultAt
            }));
        }

        /// <summary>
        /// 账户全览（投注+奖金）
        /// </summary>
        /// <param name="context"></param>
        private void accountcount(HttpContext context)
        {
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            Dictionary<MoneyLog.MoneyType, decimal> list = BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.CreateAt > DateTime.Now.Date && (t.Type == MoneyLog.MoneyType.Bet || t.Type == MoneyLog.MoneyType.Reward)).GroupBy(t => t.Type).ToDictionary(t => t.Key, t => t.Sum(p => p.Money));

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Bet = list.Get(MoneyLog.MoneyType.Bet, decimal.Zero),
                Reward = list.Get(MoneyLog.MoneyType.Reward, decimal.Zero)
            });

        }

        /// <summary>
        /// 中奖纪录
        /// </summary>
        /// <param name="context"></param>
        private void rewardlist(HttpContext context)
        {
            var list = BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID && t.Status == LotteryOrder.OrderStatus.Win && t.Reward > decimal.Zero).OrderByDescending(t => t.ID).Take(Math.Max(10, QF("count", 10))).Select(t => new
            {
                t.UserID,
                t.Type,
                t.Reward
            }).ToList();

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                UserName = WebAgent.HiddenName(UserAgent.Instance().GetUserName(t.UserID)),
                Game = LotteryAgent.Instance().GetLotteryName(t.Type),
                t.Reward
            }));
        }

        #endregion

        #region ================  合买功能 ===============

        /// <summary>
        /// 合买功能的初始化选项
        /// </summary>
        /// <param name="context"></param>
        private void unitedinit(HttpContext context)
        {
            LotteryType type = QF("Type").ToEnum<LotteryType>();

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Game = LotteryAgent.Instance().GetLotteryName(type),
                Type = type,
                Index = new JsonString(Utils.GetLotteryIndex(type, 12).ToJson(t => t.Index, t => t.ResultAt)),
                Public = new JsonString(typeof(United.PublicType).ToList().Select(t => new { t.Name, t.Description }).ToJson())
            });
        }

        /// <summary>
        /// 发布合买
        /// </summary>
        /// <param name="context"></param>
        private void saveunited(HttpContext context)
        {
            context.Response.Write(false, "合买功能暂未开放");
            United united = context.Request.Form.Fill<United>();

            this.ShowResult(context, LotteryAgent.Instance().AddUnited(UserInfo.ID, united), "合买发布成功");
        }

        /// <summary>
        /// 合买订单列表
        /// </summary>
        /// <param name="context"></param>
        private void unitedlist(HttpContext context)
        {
            IQueryable<United> list = BDC.United.Where(t => t.SiteID == SiteInfo.ID);
            if (!string.IsNullOrEmpty(QF("Game"))) list = list.Where(t => t.Type == QF("Game").ToEnum<LotteryType>());

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Game = LotteryAgent.Instance().GetLotteryName(t.Type),
                t.Index,
                t.Title,
                UserName = WebAgent.HiddenName(UserAgent.Instance().GetUserName(t.UserID)),
                t.Money,
                t.Progress,
                t.StatusName
            }));
        }

        /// <summary>
        /// 合买信息
        /// </summary>
        /// <param name="context"></param>
        private void unitedinfo(HttpContext context)
        {
            United united = LotteryAgent.Instance().GetlotteryUnitedInfo(QF("ID", 0));
            if (united == null)
            {
                context.Response.Write(false, "编号错误");
            }
            List<UnitedItem> itemList = LotteryAgent.Instance().GetLotteryUnitedList(united.ID);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                united.ID,
                united.Title,
                Game = LotteryAgent.Instance().GetLotteryName(united.Type),
                united.Index,
                united.Total,
                united.Remaining,
                united.Package,
                united.UnitMoney,
                UserName = WebAgent.HiddenName(UserAgent.Instance().GetUserName(united.UserID)),
                Profit = 0.1M,
                united.Status,
                united.StatusName,
                united.Commission,
                united.CreateAt,
                united.Money,
                list = new JsonString(itemList.Select(t => new
                {
                    UserName = WebAgent.HiddenName(UserAgent.Instance().GetUserName(t.UserID)),
                    t.Unit,
                    t.CreateAt
                }).ToJson())
            });
        }

        /// <summary>
        /// 参与合买
        /// </summary>
        /// <param name="context"></param>
        private void joinunited(HttpContext context)
        {
            int unit = QF("Unit", 0);
            int id = QF("ID", 0);

            this.ShowResult(context, LotteryAgent.Instance().AddUnitedItem(UserInfo.ID, id, unit), "合买成功");
        }

        #endregion

        #region =========== 走势图  ============

        /// <summary>
        /// 走势图数据
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void trendlist(HttpContext context)
        {
            if (string.IsNullOrEmpty(QF("Type")))
            {
                context.Response.Write(false, "未指定彩种");
            }
            int count = QF("Count", 30);
            LotteryType type = QF("Type").ToEnum<LotteryType>();

            IQueryable<LotteryTrend> list = BDC.LotteryTrend.Where(t => t.Type == type);
            if (type.GetCategory().SiteLottery)
            {
                list = list.Where(t => t.SiteID == SiteInfo.ID).Join(BDC.SiteNumber.Where(t => t.SiteID == SiteInfo.ID && t.Type == type && t.ResultAt < DateTime.Now), t => t.Index, t => t.Index, (a, b) => a);
            }

            list = list.OrderByDescending(t => t.Index).Take(count).OrderBy(t => t.Index);
            if (!type.GetCategory().SiteLottery)
            {
                if (list.Select(t => t.Index).ToArray().Join(LotteryAgent.Instance().GetSiteResultNumber(type), t => t, t => t.Key, (a, b) => a).Count() != 0)
                {
                    context.Response.Write(false, "发生错误");
                }
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.ToList(), t => new
            {
                t.Index,
                t.Number,
                Data = new JsonString(t.ToString())
            }));
        }

        #endregion

        /// <summary>
        /// 获取彩种的ID以及约束
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void type(HttpContext context)
        {
            XElement root = new XElement("root");
            foreach (LotteryType t in Enum.GetValues(typeof(LotteryType)))
            {
                XElement item = new XElement(t.ToString());
                item.SetAttributeValue("Value", (byte)t);
                item.SetAttributeValue("Number", t.GetCategory().CategoryInfo.Ball);
                item.SetAttributeValue("Length", t.GetCategory().CategoryInfo.Length);
                item.SetAttributeValue("Name", t.GetDescription());
                root.Add(item);
            }
            root.SetAttributeValue("time", this.StopwatchMessage(context));
            context.Response.ContentType = "text/xml";
            context.Response.Write(root);
        }

        /// <summary>
        /// 获取一个彩种的信息
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void gameinfo(HttpContext context)
        {
            LotteryType game = WebAgent.QF("Game").ToEnum<LotteryType>();

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Name = LotteryAgent.Instance().GetLotteryName(game)
            });
        }

    }
}
