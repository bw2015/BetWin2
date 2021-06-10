using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using SP.Studio.Core;

using BW.Agent;
using BW.Common.Games;
using BW.GateWay.Games;
using SP.Studio.Model;
using SP.Studio.Web;
using SP.Studio.Data;
using SP.Studio.PageBase;
using BW.Common.Users;

namespace BW.Handler.admin
{
    /// <summary>
    /// 第三方游戏
    /// </summary>
    public class game : IHandler
    {
        /// <summary>
        /// 第三方游戏列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.参数设定.Value)]
        private void gamelist(HttpContext context)
        {
            List<GameSetting> list = GameAgent.Instance().GetGameSetting();

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Type,
                TypeName = t.Type.GetDescription(),
                t.IsOpen,
                t.Turnover,
                t.TotalMoney,
                t.Money,
                t.Rate,
                MoneyValue = t.Rate == decimal.Zero ? decimal.Zero : t.Money / t.Rate
            }));
        }

        /// <summary>
        /// 分数额度变化日志
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.参数设定.Value)]
        private void moneylog(HttpContext context)
        {
            GameType type = QF("Type").ToEnum<GameType>();
            IQueryable<GameMoneyLog> list = BDC.GameMoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.Type == type);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.Money,
                t.Balance,
                t.CreateAt,
                t.Description
            }));
        }

        /// <summary>
        /// 获取第三方游戏接口设置
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.参数设定.Value)]
        private void setting(HttpContext context)
        {
            if (!Enum.IsDefined(typeof(GameType), QF("Type")))
            {
                context.Response.Write(false, "类型输入错误");
            }
            GameType type = QF("Type").ToEnum<GameType>();
            GameSetting setting = GameAgent.Instance().GetGameSettingInfo(type);

            context.Response.Write(true, this.StopwatchMessage(context), setting.ToString());
        }

        /// <summary>
        /// 保存游戏接口设置信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.参数设定.Value)]
        private void savesetting(HttpContext context)
        {
            if (!Enum.IsDefined(typeof(GameType), QF("Type")))
            {
                context.Response.Write(false, "类型输入错误");
            }
            GameType type = QF("Type").ToEnum<GameType>();
            GameSetting setting = GameAgent.Instance().GetGameSettingInfo(type);

            setting = context.Request.Form.Fill(setting);

            this.ShowResult(context, GameAgent.Instance().SaveGameSettingInfo(setting), "保存成功");
        }

        /// <summary>
        /// 电子游戏记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏记录.电子记录.Value)]
        private void slotlog(HttpContext context)
        {
            var list = BDC.SlotLog.Where(t => t.SiteID == SiteInfo.ID);

            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<GameType>());
            if (!string.IsNullOrEmpty(QF("User")))
            {
                list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            }
            if (!string.IsNullOrEmpty(QF("Game"))) list = list.Where(t => t.GameName.Contains(QF("Game")));
            if (!string.IsNullOrEmpty(QF("BillNo"))) list = list.Where(t => t.BillNo.Contains(QF("BillNo")));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.PlayAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.PlayAt < DateTime.Parse(QF("EndAt")).AddDays(1));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.PlayAt), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.GameName,
                t.BillNo,
                t.BetAmount,
                t.Money,
                t.Balance,
                t.PlayAt,
                t.CreateAt
            }, new
            {
                BetAmount = this.Show(list.Sum(t => (decimal?)t.BetAmount)),
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }


        [Admin(AdminPermission.第三方游戏.游戏记录.真人记录.Value)]
        private void videolog(HttpContext context)
        {
            var list = BDC.VideoLog.Where(t => t.SiteID == SiteInfo.ID);
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<GameType>());
            if (!string.IsNullOrEmpty(QF("User")))
            {
                list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            }
            if (!string.IsNullOrEmpty(QF("Game"))) list = list.Where(t => t.GameName.Contains(QF("Game")));
            if (!string.IsNullOrEmpty(QF("BillNo"))) list = list.Where(t => t.BillNo.Contains(QF("BillNo")));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.StartAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.StartAt < DateTime.Parse(QF("EndAt")).AddDays(1));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.GameName,
                t.BillNo,
                t.GameCode,
                t.PlayType,
                Status = t.Status.GetDescription(),
                t.BetAmount,
                t.Money,
                t.Balance,
                t.StartAt,
                t.CreateAt
            }, new
            {
                BetAmount = this.Show(list.Sum(t => (decimal?)t.BetAmount)),
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 体育记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏记录.体育记录.Value)]
        private void sportlog(HttpContext context)
        {
            var list = BDC.SportLog.Where(t => t.SiteID == SiteInfo.ID);
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<GameType>());
            if (!string.IsNullOrEmpty(QF("User"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            if (!string.IsNullOrEmpty(QF("WagersID"))) list = list.Where(t => t.WagersID.Contains(QF("WagersID")));
            if (!string.IsNullOrEmpty(QF("GameType"))) list = list.Where(t => t.GameType == QF("GameType"));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.PlayAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.PlayAt < DateTime.Parse(QF("EndAt")).AddDays(1));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.GameType,
                t.WagersID,
                t.PlayAt,
                t.BetAmount,
                Status = t.Status.GetDescription(),
                t.BetMoney,
                t.Money,
                t.ResultAt,
                t.Result
            }, new
            {
                BetMoney = this.Show(list.Sum(t => (decimal?)t.BetMoney)),
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 第三方游戏的原始日志详情
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏记录.Value)]
        private void logview(HttpContext context)
        {
            string result = string.Empty;
            int id = QF("ID", 0);
            switch (QF("Type"))
            {
                case "Video":
                    result = BDC.VideoLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).Select(t => t.ExtendXML).FirstOrDefault();
                    break;
                case "Slot":
                    result = BDC.SlotLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).Select(t => t.ExtendXML).FirstOrDefault();
                    break;
                case "Sport":
                    result = BDC.SportLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).Select(t => t.ExtendXML).FirstOrDefault();
                    break;
            }
            if (string.IsNullOrEmpty(result))
            {
                context.Response.Write(false, "没有记录");
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Content = result
            });
        }
        /// <summary>
        /// 游戏列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.参数设定.Value)]
        private void playerlist(HttpContext context)
        {
            GameType type = QF("Type").ToEnum<GameType>();
            IEnumerable<GamePlayer> list = GameAgent.Instance().GetGamePlayerList(type);
            if (list.Count() == 0)
            {
                context.Response.Write(false, "当前游戏没有单独游戏入口");
            }
            switch (QF("Platform"))
            {
                case "PC":
                    list = list.Where(t => t.GameInfo.IsPC);
                    break;
                case "Mobile":
                    list = list.Where(t => t.GameInfo.IsMobile);
                    break;
            }
            if (!string.IsNullOrEmpty(QF("Category"))) list = list.Where(t => t.GameInfo.Category == QF("Category"));
            if (!string.IsNullOrEmpty(QF("Name"))) list = list.Where(t => t.GameInfo.Name.Contains(QF("Name")));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.IsTop), t => new
            {
                t.ID,
                t.Name,
                t.Category,
                t.IsOpen,
                t.IsTop,
                Platform = t.Platform
            }));
        }

        /// <summary>
        /// 更改玩法的参数值
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.参数设定.Value)]
        private void updateplayercategory(HttpContext context)
        {
            GamePlayer player = GameAgent.Instance().GetGamePlayerInfo(QF("id", 0));
            if (player == null)
            {
                context.Response.Write(false, "编号错误");
            }

            switch (QF("name"))
            {
                case "IsTop":
                    player.IsTop = QF("value", 0) == 1;
                    player.Update(null, t => t.IsTop);
                    break;
                case "IsOpen":
                    player.IsOpen = QF("value", 0) == 1;
                    player.Update(null, t => t.IsOpen);
                    break;
                //case "Sort":
                //    if (QF("value", (short)-1) == -1)
                //    {
                //        context.Response.Write(false, "排序值输入错误");
                //    }
                //    player.Sort = QF("value", (short)0);
                //    player.Update(null, t => t.Sort);
                //    break;
            }

            context.Response.Write(true, "修改成功");
        }

        /// <summary>
        /// 游戏的分类列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.参数设定.Value)]
        private void playercategory(HttpContext context)
        {
            GameType type = QF("Type").ToEnum<GameType>();
            var list = GameAgent.Instance().GetGamePlayerList(type).GroupBy(t => t.Category).Select(t => t.Key);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                text = t,
                value = t
            }));
        }


        /// <summary>
        /// 刷新用户的第三方游戏账户余额
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.第三方游戏)]
        private void updatebalance(HttpContext context)
        {
            int userId = QF("UserID", 0);
            GameType type = QF("Type").ToEnum<GameType>();

            this.ShowResult(context, UserAgent.Instance().UpdateGameAccountMoney(userId, type), "刷新成功");
        }

        /// <summary>
        /// 转账记录列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.转账记录.Value)]
        private void transferlog(HttpContext context)
        {
            var list = BDC.TransferLog.Where(t => t.SiteID == SiteInfo.ID);
            if (!string.IsNullOrEmpty(QF("Type")))
            {
                list = list.Where(t => t.Type == QF("Type").ToEnum<GameType>());
            }
            if (!string.IsNullOrEmpty(QF("Action")))
            {
                list = list.Where(t => t.Action == QF("Action").ToEnum<TransferLog.ActionType>());
            }
            if (!string.IsNullOrEmpty(QF("Status")))
            {
                list = list.Where(t => t.Status == QF("Status").ToEnum<TransferLog.TransferStatus>());
            }
            if (WebAgent.IsType<DateTime>(QF("StartAt")))
            {
                list = list.Where(t => t.CreateAt >= DateTime.Parse(QF("StartAt")));
            }
            if (WebAgent.IsType<DateTime>(QF("EndAt")))
            {
                list = list.Where(t => t.CreateAt <= DateTime.Parse(QF("EndAt")));
            }
            if (!string.IsNullOrEmpty(QF("User")))
            {
                list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            }
            if (QF("SourceID", 0) != 0)
            {
                list = list.Where(t => t.ID == QF("SourceID", 0));
            }
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Action = t.Action.GetDescription(),
                t.Money,
                t.CreateAt,
                t.CheckAt,
                Status = t.Status.GetDescription()
            }));
        }

        /// <summary>
        /// 转账信息（状态）
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.转账记录.Value)]
        private void transferinfo(HttpContext context)
        {
            TransferLog log = GameAgent.Instance().GetTransferInfo(QF("ID", 0));
            if (log == null)
            {
                context.Response.Write(false, "编号错误");
            }
            MoneyLock moneyLock = null;
            if (log.Action == TransferLog.ActionType.IN)
            {
                moneyLock = UserAgent.Instance().GetMoneyLockInfo(log.UserID, log.ID, MoneyLock.LockType.Transfer);
            }

            MoneyLog moneyLog = null;
            moneyLog = UserAgent.Instance().GetMoneyLogInfo(log.UserID, log.Action == TransferLog.ActionType.IN ? MoneyLog.MoneyType.TransferToGame : MoneyLog.MoneyType.TransferToSite, log.ID);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                log.ID,
                log.UserID,
                UserName = UserAgent.Instance().GetUserName(log.UserID),
                Type = log.Type.GetDescription(),
                log.CreateAt,
                log.CheckAt,
                log.Description,
                Action = log.Action.GetDescription(),
                Status = log.Status.GetDescription(),
                Result = log.Check().GetDescription(),
                MoneyLock = moneyLock == null ? "无锁定信息" : string.Format("[{0}] {1}元 备注：{2}",
                    moneyLock.UnLockAt.Year > 2000 ? "已解锁" : "锁定中",
                    moneyLock.Money.ToString("c"),
                    moneyLock.UnLockAt.Year > 2000 ? moneyLock.UnLockDesc : moneyLock.Description
                ),
                MoneyLog = new JsonString(moneyLog == null ? "null" :
                    moneyLog.ToJson(t => t.Money, t => t.Balance, t => t.CreateAt, t => t.Description))
            });
        }

        /// <summary>
        /// 检查并且处理转账的状态
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.转账记录.Value)]
        private void transfercheck(HttpContext context)
        {
            this.ShowResult(context, GameAgent.Instance().CheckTransfer(QF("ID", 0), SiteInfo.ID), "处理成功");
        }

        /// <summary>
        /// 批量处理转账记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.转账记录.Value)]
        private void transferchecklist(HttpContext context)
        {
            IQueryable<int> list = BDC.TransferOrder.Where(t => t.SiteID == SiteInfo.ID && t.Status == TransferOrder.TransferStatus.None && t.CreateAt < DateTime.Now.AddMinutes(-2)).Select(t => t.ID);
            int success, faild;
            success = faild = 0;
            foreach (int transferId in list)
            {
                if (GameAgent.Instance().CheckTransfer(transferId, SiteInfo.ID))
                {
                    success++;
                }
                else
                {
                    faild++;
                }
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Success = success,
                Faild = faild
            });
        }

        /// <summary>
        /// 筹码兑换
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.筹码兑换.Value)]
        private void transfercasino(HttpContext context)
        {
            string code = QF("Code");
            TransferLog log;
            this.ShowResult(context, GameAgent.Instance().CheckTransferCasino(code, out log), "兑换成功", new
            {
                log.UserID,
                UserName = UserAgent.Instance().GetUserName(log.UserID),
                log.Money
            });
        }

        /// <summary>
        /// 操作转入或者转出
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.第三方游戏)]
        private void transfer(HttpContext context)
        {
            int userId = QF("UserID", 0);
            GameType type = QF("Type").ToEnum<GameType>();
            decimal money = QF("Money", decimal.Zero);

            this.ShowResult(context, GameAgent.Instance().Transfer(userId, money, type, QF("Action")), "操作成功");
        }

        [Admin(AdminPermission.第三方游戏.游戏配置.用户余额.Value)]
        private void creditlist(HttpContext context)
        {
            GameType type = QF("Type").ToEnum<GameType>();

            int recordCount;
            List<UserGameCredit> list = GameAgent.Instance().GetCreditList(type, this.PageIndex, this.PageSize, out recordCount);

            StringBuilder sb = new StringBuilder();
            sb.Append("{")
              .AppendFormat("\"RecordCount\":\"{0}\",", recordCount)
              .AppendFormat("\"PageIndex\":\"{0}\",", this.PageIndex)
              .AppendFormat("\"PageSize\":\"{0}\"", this.PageSize)
              .AppendFormat(",\"data\":{{{0}}}", string.Join(",", typeof(GameType).ToList().Select(t => "\"" + t.Name + "\":\"" + t.Description + "\"")))
              .AppendFormat(",\"list\":{0}", list.ConvertAll(t => new
              {
                  t.UserID,
                  UserName = UserAgent.Instance().GetUserName(t.UserID),
                  Credit = new JsonString(t.Credit.ToJson())
              }).ToJson())
              .Append("}");

            context.Response.Write(true, this.StopwatchMessage(context), sb.ToString());
        }

        /// <summary>
        /// 批量更新用户余额
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.第三方游戏.游戏配置.用户余额.Value)]
        private void updatecredit(HttpContext context)
        {
            var list = BDC.UserGame.Where(t => t.SiteID == SiteInfo.ID).OrderBy(t => t.UserID).ThenBy(t => t.Type);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.UserID,
                t.Type,
                Success = UserAgent.Instance().UpdateGameAccountMoney(t.UserID, t.Type) ? 1 : 0
            }));
        }

    }
}
