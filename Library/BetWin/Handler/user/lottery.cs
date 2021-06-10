using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

using BW.Agent;
using SP.Studio.Core;
using BW.Common.Lottery;
using BW.Common.Users;
using BW.Framework;
using SP.Studio.Model;
using SP.Studio.Web;
using BW.GateWay.Lottery;

namespace BW.Handler.user
{
    /// <summary>
    /// 彩票
    /// </summary>
    public class lottery : IHandler
    {
        /// <summary>
        /// 获取用户的彩种列表
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void list(HttpContext context)
        {
            if (SiteInfo.LotteryList == null)
            {
                context.Response.Write(false, "站点的彩种列表为空");
            }
            short index = 0;

            List<UserLottery> list = new List<UserLottery>();
            if (UserInfo != null)
            {
                foreach (UserLotterySetting userSetting in UserAgent.Instance().GetUserLottery(UserInfo.ID))
                {
                    LotterySetting lottery = SiteInfo.LotteryList.Where(t => t.Game == userSetting.Game).FirstOrDefault();
                    if (lottery == null) continue;
                    list.Add(new UserLottery(lottery, index++));
                }
            }
            list.AddRange(SiteInfo.LotteryList.Where(t => !list.Select(p => p.Game).Contains(t.Game)).Select(t => new UserLottery(t, index++)));
            if (QF("wechat", 0) == 1)
            {
                list = list.FindAll(t => t.Game.GetCategory().Wechat);
            }
            StringBuilder sb = new StringBuilder();
            if (QF("category", 0) == 1)
            {
                sb.Append("{")
                  .Append(string.Concat("\"list\":[", string.Join(",", list.Select(t => t.ToString())), "],"))
                  .AppendFormat("\"category\":{0}", LotteryAgent.Instance().GetLotteryCateList().ToJson(t => t.ID, t => t.Name, t => t.Code))
                  .Append("}");
            }
            else
            {
                sb.Append(string.Concat("[", string.Join(",", list.Select(t => t.ToString())), "]"));
            }

            context.Response.Write(true, this.StopwatchMessage(context), sb.ToString());
        }

        private void category(HttpContext context)
        {

        }

        /// <summary>
        /// 所有的彩种
        /// </summary>
        /// <param name="context"></param>
        private void gamelist(HttpContext context)
        {
            LotterySetting[] list = SiteInfo.LotteryList;
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
        /// 投注订单状态
        /// </summary>
        /// <param name="context"></param>
        private void orderstatus(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(typeof(LotteryOrder.OrderStatus).ToList(), t => new
            {
                value = t.Name,
                text = t.Description
            }));
        }

        /// <summary>
        /// 保存用户的彩种排序
        /// </summary>
        /// <param name="context"></param>
        private void sort(HttpContext context)
        {
            this.CheckUserLogin(context);
            LotteryType[] lottery = QF("Sort").Split(',').Select(t => t.ToEnum<LotteryType>()).ToArray();

            context.Response.Write(UserAgent.Instance().UpdateLotterySort(UserInfo.ID, lottery), this.StopwatchMessage(context));
        }

        /// <summary>
        /// 彩种的玩法
        /// </summary>
        /// <param name="context"></param>
        private void player(HttpContext context)
        {
            this.CheckUserLogin(context);
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            if (!Enum.IsDefined(typeof(LotteryType), (byte)game))
            {
                context.Response.Write(false, "没有指定彩种");
            }

            if (!SiteInfo.LotteryPlayer.ContainsKey(game))
            {
                context.Response.Write(false, "当前站点没有开放该彩种");
            }

            bool isMobile = WebAgent.QF("IsMobile", 0) == 1;

            IEnumerable<LotteryPlayer> player = SiteInfo.LotteryPlayer[game];
            if (isMobile) player = player.Where(t => t.IsMobile);


            LotterySetting setting = SiteInfo.LotteryList.Where(t => t.Game == game).FirstOrDefault();

            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Type\":\"{0}\",", setting.Game)
                .AppendFormat("\"Category\":\"{0}\",", setting.Game.GetCategory().Cate)
                .AppendFormat("\"Game\":\"{0}\",", setting.Name)
                .AppendFormat("\"Description\":\"{0}\",", setting.Description)
                .AppendFormat("\"Rebate\":{0},", setting.GetRebate(UserInfo.Rebate))
                .AppendFormat("\"Player\" : [{0}]", string.Join(",", player.Select(t =>
                    t.ToString(Utils.GetRebate(SiteInfo.Setting.MaxRebate, UserInfo.Rebate, setting.MaxRebate), setting.SinglePercent, setting.SingleReward))))
            .Append("}");

            context.Response.Write(true, this.StopwatchMessage(context), sb.ToString());
        }

        /// <summary>
        /// 获取用户的赔率
        /// </summary>
        /// <param name="context"></param>
        private void playerreward(HttpContext context)
        {
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            if (!Enum.IsDefined(typeof(LotteryType), (byte)game))
            {
                context.Response.Write(false, "没有指定彩种");
            }
            if (!SiteInfo.LotteryPlayer.ContainsKey(game))
            {
                context.Response.Write(false, "当前站点没有开放该彩种");
            }
            string[] code = QF("Code").Split(',').Select(t => string.Format("{0}_{1}", game, t)).ToArray();

            LotteryPlayer[] player = SiteInfo.LotteryPlayer[game];
            LotterySetting setting = SiteInfo.LotteryList.Where(t => t.Game == game).FirstOrDefault();

            context.Response.Write(true, LotteryAgent.Instance().GetLotteryName(game), string.Concat("{",
                string.Join(",",
                player.Where(t => code.Contains(t.Code)).Select(t => string.Format("\"{0}\":{1}", t.Code, t.ToString(Utils.GetRebate(SiteInfo.Setting.MaxRebate, UserInfo.Rebate, setting.MaxRebate))))),
                "}"));
        }

        /// <summary>
        /// 保存投注结果
        /// </summary>
        /// <param name="context"></param>
        private void save(HttpContext context)
        {
            StreamReader stream = new StreamReader(context.Request.InputStream);
            string data = stream.ReadToEnd();
            DateTime? now = (DateTime?)context.Items[BetModule.DATETIME];

            List<LotteryOrder> orders = LotteryAgent.Instance().GetLotteryOrderList(data, true, now);
            if (orders == null)
            {
                context.Response.Write(false, LotteryAgent.Instance().Message());
            }

            LotteryOrder order = orders.FirstOrDefault();

            this.ShowResult(context, LotteryAgent.Instance().SaveOrder(UserInfo.ID, orders),
                string.Format("{0}第{1}期投注成功", LotteryAgent.Instance().GetLotteryName(order.Type), order.Index), new
                {
                    Index = order.Index,
                    BuildIndex = new JsonString(order.Type.GetCategory().BuildIndex ? 1 : 0)
                });
        }

        /// <summary>
        /// 在此购买一次订单
        /// </summary>
        /// <param name="context"></param>
        private void orderrepeater(HttpContext context)
        {
            this.ShowResult(context, LotteryAgent.Instance().SaveOrder(QF("ID", 0), UserInfo.ID), "再次购买成功");
        }

        /// <summary>
        /// 保存追号
        /// </summary>
        /// <param name="context"></param>
        private void savechase(HttpContext context)
        {
            StreamReader stream = new StreamReader(context.Request.InputStream);
            string data = stream.ReadToEnd();
            LotteryChaseItem[] items;
            LotteryChase chase = new LotteryChase(data, out items);
            if (chase == null || items == null)
            {
                context.Response.Write(false, "追号内容错误");
            }

            this.ShowResult(context, LotteryAgent.Instance().SaveOrder(UserInfo.ID, chase, items), "追号成功");
        }

        /// <summary>
        /// 保存微信投注
        /// </summary>
        /// <param name="context"></param>
        private void savewechat(HttpContext context)
        {
            LotteryType type = WebAgent.GetParam("Type").ToEnum<LotteryType>();
            string content = WebAgent.GetParam("Content");

            this.ShowResult(context, LotteryAgent.Instance().SaveOrder(UserInfo.ID, type, content), "投注成功", new
            {
                Money = UserAgent.Instance().GetUserMoney(UserInfo.ID)
            });
        }

        /// <summary>
        /// 投注记录
        /// </summary>
        /// <param name="context"></param>
        private void playerorders(HttpContext context)
        {
            this.CheckUserLogin(context);
            LotteryType game = QF("Game").ToEnum<LotteryType>();
            if (!Enum.IsDefined(typeof(LotteryType), (byte)game))
            {
                context.Response.Write(false, "没有指定彩种");
            }

            List<LotteryOrder> list = LotteryAgent.Instance().GetLotteryOrderList(UserInfo.ID, game, 10);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                Game = LotteryAgent.Instance().GetLotteryName(game),
                t.Index,
                t.PlayerID,
                Player = LotteryAgent.Instance().GetPlayerInfo(t.PlayerID).Name,
                t.Rebate,
                t.BetReturn,
                Number = WebAgent.Left(t.Number, 6),
                t.Money,
                t.ResultNumber,
                t.Reward,
                Status = t.Status.GetDescription()
            }));
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="context"></param>
        private void orderdetail(HttpContext context)
        {
            int orderId = QF("id", 0);
            bool history = QF("history", 0) == 1;
            LotteryOrder order = history ? LotteryAgent.Instance().GetLotteryOrderHisotryInfo(orderId) : LotteryAgent.Instance().GetLotteryOrderInfo(orderId);
            if (order == null || (order.UserID != UserInfo.ID && !UserAgent.Instance().IsUserChild(UserInfo.ID, order.UserID)))
            {
                context.Response.Write(false, "订单号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                order.ID,
                order.CreateAt,
                order.Index,
                Game = LotteryAgent.Instance().GetLotteryName(order.Type),
                order.PlayerID,
                Player = SiteInfo.LotteryPlayerInfo[order.PlayerID].Name,
                order.Rebate,
                order.BetReturn,
                order.Number,
                order.Bet,
                order.Mode,
                order.LotteryMode,
                order.Times,
                order.Money,
                Status = order.Status.GetDescription(),
                order.ResultNumber,
                order.LotteryAt,
                order.Reward,
                IsRevoke = UserInfo.ID == order.UserID && LotteryAgent.Instance().IsRevoke(order) && !history ? 1 : 0
            });
        }

        /// <summary>
        /// 提交撤单
        /// </summary>
        /// <param name="context"></param>
        private void orderrevoke(HttpContext context)
        {
            int orderId = QF("id", 0);
            LotteryOrder order = LotteryAgent.Instance().GetLotteryOrderInfo(orderId);
            if (order == null || order.UserID != UserInfo.ID)
            {
                context.Response.Write(false, "订单号错误");
            }

            this.ShowResult(context, LotteryAgent.Instance().LotteryOrderRevoke(order), "撤单成功");
        }

        /// <summary>
        /// 投注订单
        /// </summary>
        /// <param name="context"></param>
        private void orderlist(HttpContext context)
        {
            var list = BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID);

            if (!string.IsNullOrEmpty(QF("Type")))
            {
                LotteryType type = QF("Type").ToEnum<LotteryType>();
                list = list.Where(t => t.Type == type);
            }
            if (!string.IsNullOrEmpty(QF("Status")))
            {
                LotteryOrder.OrderStatus status = QF("Status").ToEnum<LotteryOrder.OrderStatus>();
                list = list.Where(t => t.Status == status);
            }
            switch (QF("View", 0))
            {
                case 1: // 直属下级
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.Depth == 1).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 2: // 全部下级
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 3: // 指定用户
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.ChildID == UserAgent.Instance().GetUserID(QF("User"))).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                default:
                    list = list.Where(t => t.UserID == UserInfo.ID && t.TableID == UserInfo.ID.GetTableID());
                    break;
            }

            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));

            bool revoke = QF("revoke", 0) == 1;
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Lottery = t.Type,
                Type = LotteryAgent.Instance().GetLotteryName(t.Type),
                Player = SiteInfo.LotteryPlayerInfo.ContainsKey(t.PlayerID) ? SiteInfo.LotteryPlayerInfo[t.PlayerID].Name : t.PlayerID.ToString(),
                Number = WebAgent.Left(t.Number, 10),
                t.Mode,
                t.Times,
                t.Bet,
                t.Money,
                Status = t.Status.GetDescription(),
                t.Reward,
                t.Index,
                t.Remark,
                IsRevoke = !revoke ? false : LotteryAgent.Instance().IsRevoke(t)
            }));
        }

        /// <summary>
        /// 历史投注订单
        /// </summary>
        /// <param name="context"></param>
        private void historyorderlist(HttpContext context)
        {
            var list = BDC.LotteryOrderHistory.Where(t => t.SiteID == SiteInfo.ID);

            if (!string.IsNullOrEmpty(QF("Type")))
            {
                LotteryType type = QF("Type").ToEnum<LotteryType>();
                list = list.Where(t => t.Type == type);
            }
            if (!string.IsNullOrEmpty(QF("Status")))
            {
                LotteryOrder.OrderStatus status = QF("Status").ToEnum<LotteryOrder.OrderStatus>();
                list = list.Where(t => t.Status == status);
            }
            switch (QF("View", 0))
            {
                case 1: // 直属下级
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.Depth == 1).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 2: // 全部下级
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 3: // 指定用户
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.ChildID == UserAgent.Instance().GetUserID(QF("User"))).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                default:
                    list = list.Where(t => t.UserID == UserInfo.ID);
                    break;
            }

            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));

            bool revoke = QF("revoke", 0) == 1;
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Type = LotteryAgent.Instance().GetLotteryName(t.Type),
                Player = SiteInfo.LotteryPlayerInfo.ContainsKey(t.PlayerID) ? SiteInfo.LotteryPlayerInfo[t.PlayerID].Name : t.PlayerID.ToString(),
                Number = WebAgent.Left(t.Number, 10),
                t.Mode,
                t.Times,
                t.Bet,
                t.Money,
                Status = t.Status.GetDescription(),
                t.Reward,
                t.Index,
                t.Remark,
                IsRevoke = !revoke ? false : LotteryAgent.Instance().IsRevoke(t)
            }));
        }

        /// <summary>
        /// 追号列表
        /// </summary>
        /// <param name="context"></param>
        private void chaselist(HttpContext context)
        {
            IQueryable<LotteryChase> list = BDC.LotteryChase.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Game = t.Type,
                Type = LotteryAgent.Instance().GetLotteryName(t.Type),
                t.Money,
                t.Count,
                t.Total,
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
        private void chaseinfo(HttpContext context)
        {
            LotteryChase chase = LotteryAgent.Instance().GetLotteryChaseInfo(QF("ID", 0));
            if (chase == null || chase.UserID != UserInfo.ID)
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
                StatusName = chase.Status.GetDescription()
            });
        }

        /// <summary>
        /// 追号的内容
        /// </summary>
        /// <param name="context"></param>
        private void chaseitemlist(HttpContext context)
        {
            List<LotteryChaseItem> list = LotteryAgent.Instance().GetLotteryChaseItemList(QF("ID", 0));
            if (list.Count == 0 || list.Exists(t => t.UserID != UserInfo.ID))
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
        private void chaserevoke(HttpContext context)
        {
            this.ShowResult(context, LotteryAgent.Instance().RevokeLotteryChase(UserInfo.ID, QF("ID", 0)), "撤单成功");
        }
    }
}
