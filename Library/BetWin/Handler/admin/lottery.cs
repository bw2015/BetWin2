using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Linq.Expressions;
using System.Transactions;

using BW.Agent;
using BW.GateWay.Lottery;
using BW.Common.Lottery;
using SP.Studio.Web;

using SP.Studio.Core;
using SP.Studio.Model;

namespace BW.Handler.admin
{
    public class lottery : IHandler
    {
        /// <summary>
        /// 彩票列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.Value)]
        private void list(HttpContext context)
        {
            List<LotterySetting> list = LotteryAgent.Instance().GetLotteryList();
            context.Response.Write(true, "彩种列表", string.Format("[{0}]", string.Join(",", list.Select(t => t.ToString()))));
        }

        /// <summary>
        /// 所有的彩种
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void gamelist(HttpContext context)
        {
            List<LotterySetting> list = LotteryAgent.Instance().GetLotteryList();
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append(string.Join(",",
                list.GroupBy(t => t.Game.GetCategory().Cate).Select(t =>
                {

                    return string.Concat("\"", t.Key.GetDescription(), "\":{",
                        string.Join(",", list.Where(p => p.Game.GetCategory().Cate == t.Key).Select(p => string.Format("\"{0}\":\"{1}{2}\"", p.Game, p.Name, p.IsOpen ? "" : "(未开放)")))
                        , "}");
                })
            ));
            sb.Append("}");

            context.Response.Write(true, this.StopwatchMessage(context), sb.ToString());
        }

        /// <summary>
        /// 投注记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.投注记录.Value)]
        private void betlog(HttpContext context)
        {
            IQueryable<SiteOrder> list = BDC.SiteOrder.Where(t => t.SiteID == SiteInfo.ID);
            if (QF("ID", 0) != 0) list = list.Where(t => t.ID == QF("ID", 0));
            if (!string.IsNullOrEmpty(QF("User"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            if (!string.IsNullOrEmpty(QF("Agent")))
            {
                int agentId = UserAgent.Instance().GetUserID(QF("Agent"));
                if (agentId != 0)
                {
                    list = list.Where(t => t.UserID == agentId || BDC.UserDepth.Where(p => p.SiteID == SiteInfo.ID && p.UserID == agentId).Select(p => p.ChildID).Contains(t.UserID));
                }
            }
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<LotteryType>());
            if (!string.IsNullOrEmpty(QF("Index"))) list = list.Where(t => t.Index == QF("Index"));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")));
            if (!string.IsNullOrEmpty(QF("Status"))) list = list.Where(t => t.Status == QF("Status").ToEnum<LotteryOrder.OrderStatus>());
            if (QF("MinMoney", 0M) != 0) list = list.Where(t => t.Money >= QF("MinMoney", 0.00M));
            if (QF("MaxMoney", 0M) != 0) list = list.Where(t => t.Money <= QF("MaxMoney", 0.00M));
            if (QF("MinReward", 0M) != 0) list = list.Where(t => t.Reward >= QF("MinReward", 0.00M));
            if (QF("MaxReward", 0M) != 0) list = list.Where(t => t.Reward <= QF("MaxReward", 0.00M));
            if (QF("NoTest", 0) == 1) list = list.Where(t => !BDC.User.Where(p => p.SiteID == SiteInfo.ID && p.IsTest).Select(p => p.ID).Contains(t.UserID));
            if (!string.IsNullOrEmpty(QF("Content"))) list = list.Where(t => t.Number == QF("Content"));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowLinqResult(list.OrderByDescending(t => t.ID), BDC.SiteOrder, SP.Studio.Data.LockType.READPAST, t => new
            {
                t.ID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Game = LotteryAgent.Instance().GetLotteryName(t.Type),
                t.Index,
                Player = SiteInfo.LotteryPlayerInfo.ContainsKey(t.PlayerID) ? SiteInfo.LotteryPlayerInfo[t.PlayerID].PlayName : t.PlayerID.ToString(),
                t.CreateAt,
                Number = WebAgent.Left(t.Number, 256),
                t.Times,
                t.Mode,
                t.Money,
                t.Bet,
                Status = t.Status.GetDescription(),
                t.Reward
            }, new
            {
                Money = this.Show(list.Where(t => t.Status != LotteryOrder.OrderStatus.Revoke).Sum(t => (decimal?)t.Money)),
                Reward = this.Show(list.Sum(t => (decimal?)t.Reward))
            }));
        }

        /// <summary>
        /// 彩票历史订单
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.历史订单.Value)]
        private void bethistory(HttpContext context)
        {
            var list = BDC.LotteryOrderHistory.Where(t => t.SiteID == SiteInfo.ID);
            if (QF("ID", 0) != 0) list = list.Where(t => t.ID == QF("ID", 0));
            if (!string.IsNullOrEmpty(QF("User"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<LotteryType>());
            if (!string.IsNullOrEmpty(QF("Index"))) list = list.Where(t => t.Index == QF("Index"));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")));
            if (!string.IsNullOrEmpty(QF("Status"))) list = list.Where(t => t.Status == QF("Status").ToEnum<LotteryOrder.OrderStatus>());
            if (QF("MinMoney", 0M) != 0) list = list.Where(t => t.Money >= QF("MinMoney", 0.00M));
            if (QF("MaxMoney", 0M) != 0) list = list.Where(t => t.Money <= QF("MaxMoney", 0.00M));
            if (QF("MinReward", 0M) != 0) list = list.Where(t => t.Reward >= QF("MinReward", 0.00M));
            if (QF("MaxReward", 0M) != 0) list = list.Where(t => t.Reward <= QF("MaxReward", 0.00M));
            if (QF("NoTest", 0) == 1) list = list.Where(t => !BDC.User.Where(p => p.SiteID == SiteInfo.ID && p.IsTest).Select(p => p.ID).Contains(t.UserID));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Game = LotteryAgent.Instance().GetLotteryName(t.Type),
                t.Index,
                Player = SiteInfo.LotteryPlayerInfo.ContainsKey(t.PlayerID) ? SiteInfo.LotteryPlayerInfo[t.PlayerID].PlayName : t.PlayerID.ToString(),
                t.CreateAt,
                Number = WebAgent.Left(t.Number, 256),
                t.Times,
                t.Mode,
                t.Money,
                t.Bet,
                Status = t.Status.GetDescription(),
                t.Reward
            }, new
            {
                Money = this.Show(list.Where(t => t.Status != LotteryOrder.OrderStatus.Revoke).Sum(t => (decimal?)t.Money)),
                Reward = this.Show(list.Sum(t => (decimal?)t.Reward))
            }));
        }

        /// <summary>
        /// 查看投注明细
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.投注记录.Value)]
        private void betinfo(HttpContext context)
        {
            LotteryOrder order = QF("history", 0) == 1 ? LotteryAgent.Instance().GetLotteryOrderHisotryInfo(QF("ID", 0)) : LotteryAgent.Instance().GetLotteryOrderInfo(QF("ID", 0));
            if (order == null)
            {
                context.Response.Write(false, "编号错误");
            }

            LotteryPlayer player = SiteInfo.LotteryPlayerInfo.ContainsKey(order.PlayerID) ? SiteInfo.LotteryPlayerInfo[order.PlayerID] : new LotteryPlayer() { PlayName = order.PlayerID.ToString() };
            ResultNumber result = LotteryAgent.Instance().GetResultNumberInfo(order.Type, order.Index);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                order.ID,
                order.UserID,
                UserName = UserAgent.Instance().GetUserName(order.UserID),
                order.Rebate,
                BetReturn = order.BetReturn.ToString("P"),
                order.Index,
                Game = LotteryAgent.Instance().GetLotteryName(player.Type),
                Player = string.Concat(player.GroupName, " ", player.LabelName, " ", player.PlayName),
                Number = WebAgent.Left(order.Number, 4000),
                order.Times,
                order.Mode,
                order.Bet,
                order.Money,
                order.CreateAt,
                order.LotteryAt,
                order.Reward,
                order.ResultNumber,
                Status = order.Status.GetDescription(),
                order.Type,
                order.PlayerID,
                order.Remark,
                ResultAt = result == null ? "N/A" : result.ResultAt.ToString()
            });
        }

        /// <summary>
        /// 投注的具体号码（超过100K的不会在前台显示）
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.投注记录.Value)]
        private void betnumber(HttpContext context)
        {
            LotteryOrder order = LotteryAgent.Instance().GetLotteryOrderInfo(QF("ID", 0));
            if (order == null)
            {
                context.Response.Write(false, "编号错误");
            }
            //StringBuilder sb = new StringBuilder();
            //sb.Append("<html><head><title>投注号码详情</title></head><body style=\"word-break:break-all\">")
            //    .AppendFormat("<h1>投注编号：{0}</h1>", order.ID)
            //    .AppendFormat("{0}", order.Number)
            //    .Append("</body></html>");
            context.Response.ContentType = "text/plain";
            context.Response.Write(order.Number);
        }

        /// <summary>
        /// 追号管理
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.追号管理.Value)]
        private void chaselist(HttpContext context)
        {
            var list = BDC.LotteryChase.Where(t => t.SiteID == SiteInfo.ID);
            if (!string.IsNullOrEmpty(QF("User"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<LotteryType>());
            if (!string.IsNullOrEmpty(QF("Status"))) list = list.Where(t => t.Status == QF("Status").ToEnum<LotteryChase.ChaseStatus>());
            if (!string.IsNullOrEmpty(QF("StartAt"))) list = list.Where(t => t.CreateAt > QF("StartAt", new DateTime(2000, 1, 1)));
            if (!string.IsNullOrEmpty(QF("EndAt"))) list = list.Where(t => t.CreateAt < QF("EndAt", DateTime.Now).AddDays(1));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Game = LotteryAgent.Instance().GetLotteryName(t.Type),
                t.Money,
                t.Count,
                t.Total,
                t.IsRewardStop,
                t.CreateAt,
                Status = t.Status.GetDescription(),
                t.BetMoney,
                t.Reward
            }));
        }

        /// <summary>
        /// 追号详情
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.追号管理.Value)]
        private void chaseinfo(HttpContext context)
        {
            LotteryChase chase = LotteryAgent.Instance().GetLotteryChaseInfo(QF("ID", 0));
            if (chase == null)
            {
                context.Response.Write(false, "编号错误");
            }

            var content = LotteryAgent.Instance().GetLotteryOrderList(chase.Content).ConvertAll(t => new
            {
                Player = SiteInfo.LotteryPlayerInfo[t.PlayerID].Name,
                t.Mode,
                t.Bet,
                t.Times,
                t.Money,
                Number = WebAgent.Left(t.Number, 200)
            });

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Game = LotteryAgent.Instance().GetLotteryName(chase.Type),
                Content = new JsonString(content.ToJson()),
                chase.ID,
                chase.Status,
                StatusName = chase.Status.GetDescription(),
                chase.UserID,
                UserName = UserAgent.Instance().GetUserName(chase.UserID)
            });
        }

        /// <summary>
        /// 追号的内容
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.追号管理.Value)]
        private void chaseitemlist(HttpContext context)
        {
            List<LotteryChaseItem> list = LotteryAgent.Instance().GetLotteryChaseItemList(QF("ID", 0));
            if (list.Count == 0)
            {
                context.Response.Write(false, "编号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Index,
                t.Status,
                StatusName = t.Status.GetDescription(),
                t.Times,
                t.Money,
                t.Reward,
                t.StartAt,
                t.CreateAt
            }));
        }

        /// <summary>
        /// 取消追号
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.追号管理.Value)]
        private void chaserevoke(HttpContext context)
        {
            this.ShowResult(context, LotteryAgent.Instance().RevokeLotteryChase(0, QF("ID", 0)), "撤单成功");
        }

        /// <summary>
        /// 更新彩种的设置
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatesetting(HttpContext context)
        {
            if (string.IsNullOrEmpty(QF("Game")))
            {
                context.Response.Write(false, "未指定彩种");
            }
            LotteryType type = QF("Game").ToEnum<LotteryType>();
            if ((int)type == 0)
            {
                context.Response.Write(false, string.Format("彩种代码{0}不存在", QF("Game")));
            }
            string name = QF("name");
            object value = null;

            Expression<Func<LotterySetting, object>> field = null;
            switch (name)
            {
                case "Name":
                    value = QF("value");
                    if (string.IsNullOrEmpty((string)value)) value = null;
                    field = t => t.Name;
                    break;
                case "RewardPercent":
                    value = QF("value", decimal.MinusOne);
                    if ((decimal)value < 0.00M && (decimal)value >= 100M)
                    {
                        context.Response.Write(false, "中奖概率设定应在0%～100%之间");
                    }
                    field = t => t.RewardPercent;
                    break;
                case "IsOpen":
                    value = QF("value", 0) == 1;
                    field = t => t.IsOpen;
                    break;
                case "NoChase":
                    value = QF("value", 0) == 1;
                    field = t => t.NoChase;
                    break;
                case "IsManual":
                    value = QF("value", 0) == 1;
                    field = t => t.IsManual;
                    break;
            }
            if (field == null)
            {
                context.Response.Write(false, "未指定要保存的属性");
            }
            this.ShowResult(context, LotteryAgent.Instance().SaveLotterySetting(type, field, value), "保存成功");
        }

        /// <summary>
        /// 保存一个彩票类型的最大返点
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatemaxreward(HttpContext context)
        {
            LotteryCategory category = QF("Category").ToEnum<LotteryCategory>();
            if ((int)category == 0)
            {
                context.Response.Write(false, string.Format("类型{0}不存在", QF("Category")));
            }

            int value = QF("value", 0);
            if (value == 0) context.Response.Write(false, "输入的数字不正确");

            this.ShowResult(context, LotteryAgent.Instance().UpdateLotteryMaxReward(category, value), "彩种最大返奖设置成功");

        }

        /// <summary>
        /// 保存一个彩票类型的最大返点
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatemaxbet(HttpContext context)
        {
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            decimal value = QF("value", decimal.Zero);
            this.ShowResult(context, LotteryAgent.Instance().UpdateLotteryMaxBet(game, value), "单期限额设置成功");
        }

        /// <summary>
        /// 保存彩种的排序值
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatesort(HttpContext context)
        {
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            short value = QF("value", (short)0);
            this.ShowResult(context, LotteryAgent.Instance().UpdateLotterySort(game, value), "排序值设置成功");
        }

        /// <summary>
        /// 修改彩票分类
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatecategory(HttpContext context)
        {
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            int value = QF("value", 0);
            this.ShowResult(context, LotteryAgent.Instance().UpdateLotteryCategory(game, value), "分类修改成功");
        }


        /// <summary>
        /// 修改彩种的单挑限制比例
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatesinglepercent(HttpContext context)
        {
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            decimal value = QF("value", decimal.Zero);

            this.ShowResult(context, LotteryAgent.Instance().UpdateLotterySinglePercent(game, value), "单挑比例修改成功");
        }

        /// <summary>
        /// 修改彩种的单挑最大奖金
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatesinglereward(HttpContext context)
        {
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            decimal value = QF("value", decimal.Zero);
            this.ShowResult(context, LotteryAgent.Instance().UpdateLotterySingleReward(game, value), "单挑奖金修改成功");
        }

        /// <summary>
        /// 修改彩种的全包限制比例
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatemaxpercent(HttpContext context)
        {
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            decimal value = QF("value", decimal.Zero);
            this.ShowResult(context, LotteryAgent.Instance().UpdateLotteryMaxPercent(game, value), "全包比例修改成功");
        }

        /// <summary>
        /// 查找彩种列表,返回 text:"",value:""
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.Value)]
        private void type(HttpContext context)
        {
            List<LotterySetting> list = LotteryAgent.Instance().GetLotteryList();
            LotteryCategory category = QF("Value").ToEnum<LotteryCategory>();
            context.Response.Write(true, category.GetDescription(),
                list.Where(t => t.Game.GetCategory().Cate == category).Select(t => new
                {
                    text = t.Name + (t.IsOpen ? "" : "(停止)"),
                    value = t.Game
                }).ToList().ToJson());
        }

        /// <summary>
        /// 玩法列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.玩法管理.Value)]
        private void player(HttpContext context)
        {
            LotteryType type = QF("Lottery").ToEnum<LotteryType>();
            if (!Enum.IsDefined(typeof(LotteryType), (byte)type))
            {
                context.Response.Write(false, "未指定要查询的彩种");
            }

            context.Response.Write(true, type.GetDescription(), this.ShowResult(LotteryAgent.Instance().GetPlayerList(type), t => t.ToString()));
        }

        /// <summary>
        /// 获取玩法的信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.玩法管理.Value)]
        private void playerinfo(HttpContext context)
        {
            int id = QF("id", 0);
            LotteryPlayer player = LotteryAgent.Instance().GetPlayerInfo(id);
            if (player == null)
            {
                context.Response.Write(false, "玩法编号错误");
            }
            else
            {
                ResultNumber resultNumber = LotteryAgent.Instance().GetResultNumber(player.Type, 1).FirstOrDefault();
                context.Response.Write(true, "获取玩法信息", new
                {
                    player.ID,
                    player.Code,
                    player.GroupName,
                    player.LabelName,
                    player.PlayName,
                    player.Name,
                    player.Type,
                    Game = LotteryAgent.Instance().GetLotteryName(player.Type),
                    ResultNumber = resultNumber == null ? string.Empty : resultNumber.Number
                });
            }
        }

        /// <summary>
        /// 测试玩法的投注数量以及奖金
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.玩法管理.Value)]
        private void playertest(HttpContext context)
        {
            int playerId = QF("id", 0);
            string number = QF("Number");
            string resultNumber = QF("ResultNumber");

            LotteryType type;
            IPlayer player = PlayerFactory.GetPlayer(playerId, out type);
            if (player == null)
            {
                context.Response.Write(false, "玩法编号错误");
            }

            int times;
            string wechatBet = player.HasAttribute<BetChatAttribute>() ? player.GetBetChat(number, out times) : string.Empty;
            if (!string.IsNullOrEmpty(wechatBet))
            {
                number = wechatBet;
            }

            int bet = player.Bet(number);
            decimal reward = player.Reward(number, resultNumber);

            IEnumerable<string> limited = bet == 0 ? Enumerable.Empty<string>() : player.ToLimited(number);

            string time = this.StopwatchMessage(context);

            context.Response.Write(true, string.Format("耗时：{0} 注数：{1}注 奖金：{2}元<textarea style=\"width:100%; height:150px;\">投注内容：{3} \n限号信息：{4} \n共{5}条</textarea>",
                time, bet, reward.ToString("n"),
                number,
                limited == null ? "null" : string.Join(" | ", limited),
                limited == null ? 0 : limited.Count()
                ));
        }

        /// <summary>
        /// 更新玩法属性
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.玩法管理.Value)]
        private void updateplayer(HttpContext context)
        {
            string code = QF("Code");
            LotteryPlayer player = LotteryAgent.Instance().GetPlayerInfo(code);
            if (player == null)
            {
                context.Response.Write(false, LotteryAgent.Instance().Message());
            }

            string name = QF("name");
            object value = null;
            Expression<Func<LotteryPlayer, object>> field = null;

            switch (name)
            {
                case "IsMobile":
                    value = QF("value", 0) == 1;
                    field = t => t.IsMobile;
                    break;
                case "IsOpen":
                    value = QF("value", 0) == 1;
                    field = t => t.IsOpen;
                    break;
                case "SingledBet":
                    value = QF("value", -1);
                    if ((int)value < 0)
                    {
                        context.Response.Write(false, "请输入一个正确的数字");
                    }
                    field = t => t.SingledBet;
                    break;
                case "SingledReward":
                    value = QF("value", decimal.MinusOne);
                    if ((decimal)value < 0)
                    {
                        context.Response.Write(false, "请输入一个正确的单挑奖金");
                    }
                    field = t => t.SingledReward;
                    break;
                case "MaxBet":
                    value = QF("value", -1);
                    if ((int)value < 0)
                    {
                        context.Response.Write(false, "请输入一个正确的最大投注值");
                    }
                    field = t => t.MaxBet;
                    break;
                case "PlayName":
                    value = QF("value");
                    if (string.IsNullOrEmpty((string)value))
                    {
                        context.Response.Write(false, "请输入玩法的名字");
                    }
                    field = t => t.PlayName;
                    break;
                case "RewardMoney":
                    value = QF("value", decimal.Zero);
                    if ((decimal)value == decimal.Zero)
                    {
                        context.Response.Write(false, "请输入玩法奖金");
                    }
                    field = t => t.Reward;
                    break;
                case "Sort":
                    value = QF("value", (short)0);
                    field = t => t.Sort;
                    break;
            }
            if (field == null)
            {
                context.Response.Write(false, "未指定要保存的属性");
            }
            this.ShowResult(context, LotteryAgent.Instance().SaveLotteryPlayer(player, field, value), "保存成功");
        }

        /// <summary>
        /// 开奖号码管理
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.开奖管理.Value)]
        private void resultnumber(HttpContext context)
        {
            LotteryType type = QF("Type").ToEnum<LotteryType>();

            if (!type.GetCategory().SiteLottery)
            {
                IQueryable<ResultNumber> list = BDC.ResultNumber.Where(t => t.Type == type);
                if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.ResultAt > DateTime.Parse(QF("StartAt")));
                if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.ResultAt < DateTime.Parse(QF("EndAt")).AddDays(1));
                context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.Index), t => new
                {
                    Type = LotteryAgent.Instance().GetLotteryName(t.Type),
                    t.Index,
                    CreateAt = DateTime.Now,
                    t.ResultAt,
                    t.Number
                }));
            }
            else
            {
                IQueryable<SiteNumber> list = BDC.SiteNumber.Where(t => t.SiteID == SiteInfo.ID && t.Type == type);
                if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.ResultAt > DateTime.Parse(QF("StartAt")));
                if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.ResultAt < DateTime.Parse(QF("EndAt")).AddDays(1));
                context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.Index), t => new
                {
                    Type = LotteryAgent.Instance().GetLotteryName(t.Type),
                    t.Index,
                    CreateAt = DateTime.Now,
                    t.ResultAt,
                    t.Number
                }));
            }
        }


        /// <summary>
        /// 限号封锁列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.限号管理.Value)]
        private void limited(HttpContext context)
        {
            LotteryType game = QF("Lottery").ToEnum<LotteryType>();
            if (!Enum.IsDefined(typeof(LotteryType), game))
            {
                context.Response.Write(false, "没有指定彩种");
            }
            IEnumerable<LotteryPlayer> player = LotteryAgent.Instance().GetPlayerList(game).Where(t => t.Player != null);
            Dictionary<LimitedType, decimal> dic = LotteryAgent.Instance().GetLimitedSettingList(game).ToDictionary(t => t.Type, t => t.Money);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(player.GroupBy(t => t.Player.Limited).Select(t => t.Key).OrderBy(t => (int)t).Where(t => t != LimitedType.None), t => new
            {
                GameCode = game,
                Game = LotteryAgent.Instance().GetLotteryName(game),
                Type = t,
                TypeName = t.GetDescription(),
                Player = string.Join(",", player.Where(p => p.Player.Limited == t).Select(p => p.Name)),
                Money = dic.ContainsKey(t) ? dic[t] : decimal.Zero
            }));
        }

        /// <summary>
        /// 修改限号封锁值
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.限号管理.Value)]
        private void updatelimited(HttpContext context)
        {
            LotteryType game = QF("game").ToEnum<LotteryType>();
            LimitedType type = QF("type").ToEnum<LimitedType>();
            decimal money = QF("value", decimal.MinusOne);

            this.ShowResult(context, LotteryAgent.Instance().UpdateLimitedSetting(game, type, money), "限号封锁值设置成功");
        }


        /// <summary>
        /// 站点自有彩种开奖
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.开奖管理.Value)]
        private void opennumber(HttpContext context)
        {
            LotteryType type = WebAgent.GetParam("Type").ToEnum<LotteryType>();
            string index;
            bool isNeed = LotteryAgent.Instance().IsNeedOpen(SiteInfo.ID, type, out index);
            if (!isNeed)
            {
                context.Response.Write(false, string.Format("当前彩期{0}不需要开奖", index));
            }
            string number;
            this.ShowResult(context, LotteryAgent.Instance().OpenResultNumber(SiteInfo.ID, type, index, out number), "开奖成功");
        }

        /// <summary>
        /// 开奖管理
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.开奖管理.Value)]
        private void getindex(HttpContext context)
        {
            LotteryType type = QF("Type").ToEnum<LotteryType>();
            List<ResultNumber> list = LotteryAgent.Instance().GetResultIndex(type, 12);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Type = type,
                Game = LotteryAgent.Instance().GetLotteryName(type),
                List = new JsonString(list.ToJson(t => t.Index, t => t.ResultAt))
            });
        }

        /// <summary>
        /// 手动开奖
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.开奖管理.Value)]
        private void savenumber(HttpContext context)
        {
            ResultNumber number = context.Request.Form.Fill<ResultNumber>();
            if (number.Type.GetCategory().SiteLottery)
            {
                this.ShowResult(context, LotteryAgent.Instance().SaveResultNumber(number.Type, number.Index, number.Number), "保存成功");
            }
            else
            {
                this.ShowResult(context, LotteryAgent.Instance().SaveSiteResultNumber(number.Type, number.Index, number.Number), "保存成功");
            }
        }

        /// <summary>
        /// 获取试算的5个随机号码
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.开奖管理.Value)]
        private void testnumber(HttpContext context)
        {
            ResultNumber number = context.Request.Form.Fill<ResultNumber>();
            Dictionary<string, decimal> data = LotteryAgent.Instance().GetLotteryTestNumber(SiteInfo.ID, number.Type, number.Index);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(data.OrderBy(t => t.Value), t => new
            {
                Number = t.Key,
                Money = t.Value
            }));
        }

        /// <summary>
        /// 改单
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.改单)]
        private void betmodify(HttpContext context)
        {
            this.ShowResult(context, LotteryAgent.Instance().UpdateOrderNumber(QF("ID", 0), QF("PlayerID", 0), QF("Number")));
        }

        /// <summary>
        /// 管理员操作强制撤单
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.投注管理.撤单)]
        private void revoke(HttpContext context)
        {
            this.ShowResult(context, LotteryAgent.Instance().LotteryOrderRevoke(QF("ID", 0)), "撤单成功");
        }

        /// <summary>
        /// 自动采集开奖的接口
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.开奖管理.Value)]
        private void autoresultnumber(HttpContext context)
        {
            LotteryType type = QF("Type").ToEnum<LotteryType>();
            if ((byte)type == 0 || !type.GetCategory().SiteLottery)
            {
                context.Response.Write(false, "彩种错误");
            }
            this.ShowResult(context, LotteryAgent.Instance().SaveResultNumber(SiteInfo.ID, type, QF("Index"), QF("Number")), "保存成功");
        }

        /// <summary>
        /// 彩票分类管理
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void catelist(HttpContext context)
        {
            List<LotteryCate> list = BDC.LotteryCate.Where(t => t.SiteID == SiteInfo.ID).OrderByDescending(t => t.Sort).ToList();
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.Name,
                t.Sort,
                t.Code
            }));
        }

        /// <summary>
        /// 添加一个分类
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void savecate(HttpContext context)
        {
            this.ShowResult(context, LotteryAgent.Instance().AddCategory(QF("Name"), QF("Code")), "保存成功");
        }


        /// <summary>
        /// 修改彩种分类信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void updatecate(HttpContext context)
        {
            int id = QF("ID", 0);
            Expression<Func<LotteryCate, object>> field = null;
            object value = null;
            switch (QF("Name"))
            {
                case "Name":
                    field = t => t.Name;
                    value = QF("Value");
                    break;
                case "Sort":
                    field = t => t.Sort;
                    value = QF("Value", (short)0);
                    break;
                case "Code":
                    field = t => t.Code;
                    value = QF("Value");
                    break;
            }
            if (field == null)
            {
                context.Response.Write(false, "未指定要更新的字段");
            }

            this.ShowResult(context, LotteryAgent.Instance().UpdateCategory(id, value, field));
        }

        /// <summary>
        /// 删除彩种分类
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.彩票管理.彩票配置.彩种管理.Value)]
        private void deletecate(HttpContext context)
        {
            this.ShowResult(context, LotteryAgent.Instance().DeleteCategory(QF("ID", 0)));
        }
    }
}
