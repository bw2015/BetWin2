using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Resources;

using BW.Agent;
using BW.Common.Games;
using BW.Common.Users;
using SP.Studio.Security;

using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Web;
using SP.Studio.PageBase;

namespace BW.Handler.game
{
    /// <summary>
    /// 第三方游戏
    /// </summary>
    public class gateway : IHandler
    {
        /// <summary>
        /// 获取用户在游戏中信息
        /// </summary>
        /// <param name="context" name="</param>
        [Guest]
        private void getinfo(HttpContext context)
        {
            int userId = 0;
            if (UserInfo != null)
            {
                userId = UserInfo.ID;
            }
            else
            {
                userId = UserAgent.Instance().GetUserID(QF("Session", Guid.Empty));
            }
            if (userId == 0) { this.CheckUserLogin(context); }
            GameType type = QF("Type").ToEnum<GameType>();
            GameAccount gameAccount = UserAgent.Instance().GetGameAccountInfo(userId, type);

            if (gameAccount == null)
            {
                GameSetting setting = GameAgent.Instance().GetGameSettingInfo(type);
                if (!setting.IsOpen || !setting.IsSystemOpen)
                {
                    context.Response.Write(false, "当前游戏接口未开放");
                }
                if (!setting.Setting.CreateUser(userId))
                {
                    context.Response.Write(false, GameAgent.Instance().Message());
                }
                gameAccount = UserAgent.Instance().GetGameAccountInfo(userId, type);
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                UserMoney = UserAgent.Instance().GetUserMoney(userId),
                Type = type,
                Game = type.GetDescription(),
                gameAccount.Money,
                gameAccount.Password,
                gameAccount.PlayerName,
                gameAccount.UpdateAt,
                Session = UserAgent.Instance().GetUserSession(userId).ToString("N")
            });

        }

        /// <summary>
        /// 游戏开户
        /// </summary>
        /// <param name="context" name="</param>
        [Guest]
        private void register(HttpContext context)
        {
            GameSetting setting = GameAgent.Instance().GetGameSettingInfo(QF("Type").ToEnum<GameType>());
            if (!setting.IsOpen || !setting.IsSystemOpen)
            {
                context.Response.Write(false, "当前游戏接口未开放");
            }
            int userId = UserInfo == null ? UserAgent.Instance().GetUserID(QF("Session", Guid.Empty)) : UserInfo.ID;

            this.ShowResult(context, setting.Setting.CreateUser(userId), "注册成功");
        }

        /// <summary>
        /// 游戏列表
        /// </summary>
        /// <param name="context" name="</param>
        [Guest]
        private void gamelist(HttpContext context)
        {
            IEnumerable<GamePlayer> list = GameAgent.Instance().GetGamePlayerList(QF("Type").ToEnum<GameType>()).Where(t => t.IsOpen);
            int count = QF("Count", 0);
            if (QF("mobile", 0) == 1)
            {
                list = list.Where(t => t.IsMobile);
            }
            else
            {
                list = list.Where(t => t.IsPC);
            }
            if (count != 0) list = list.OrderByDescending(t => t.IsTop).Take(count);
            if (list.Count() == 0)
            {
                context.Response.Write(false, "不支持游戏列表");
            }
            string[] categoryList = list.GroupBy(t => t.Category).Select(t => string.Concat("\"", t.Key, "\"")).ToArray();
            string category = QF("Category");
            if (!string.IsNullOrEmpty(category)) list = list.Where(t => t.GameInfo.Category == category);
            string key = QF("Key");
            if (!string.IsNullOrEmpty(key)) list = list.Where(t => t.GameInfo.Name.Contains(key));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.IsTop).ThenBy(t => t.ID), t => new
            {
                t.Code,
                t.Name,
                t.Cover
            }, new
            {
                Category = category,
                List = new JsonString(string.Concat("[", string.Join(",", categoryList), "]"))
            }));
        }

        /// <summary>
        /// 获取开放的游戏类型列表
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void opengamelist(HttpContext context)
        {
            List<GameType> types = new List<GameType>();

            string category = QF("Category");
            if (string.IsNullOrEmpty(category))
            {
                foreach (string key in context.Request.Form.AllKeys)
                {
                    if (Enum.IsDefined(typeof(GameType), key)) types.Add(key.ToEnum<GameType>());
                }
            }
            else
            {
                GameCategory cate = category.ToEnum<GameCategory>();
                foreach (GameType type in Enum.GetValues(typeof(GameType)))
                {
                    GameAttribute game = type.GetAttribute<GameAttribute>();
                    if (game != null && game.Category.Contains(cate)) types.Add(type);
                }
            }
            var list = BDC.GameSetting.Where(t => t.SiteID == SiteInfo.ID && t.IsOpen);
            if (types.Count != 0) list = list.Where(t => types.Contains(t.Type));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list,
                t => new
                {
                    t.Type,
                    Name = t.Type.GetDescription(),
                    Category = new JsonString("[", string.Join(",", t.Type.GetAttribute<GameAttribute>().Category.Select(p => "\"" + p + "\"")), "]")
                }));
        }

        /// <summary>
        /// 游戏设置列表
        /// </summary>
        /// <param name="context" name="</param>
        private void settinglist(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(GameAgent.Instance().GetGameSetting(), t => new
            {
                t.Type,
                Name = t.Type.GetDescription(),
                t.IsOpen
            }));
        }

        /// <summary>
        /// 随机推荐的游戏
        /// </summary>
        /// <param name="context" name="</param>
        [Guest]
        private void gamelistrandom(HttpContext context)
        {
            IEnumerable<GamePlayer> list = GameAgent.Instance().GetGamePlayerList(QF("Type").ToEnum<GameType>()).Where(t => t.IsOpen);
            if (WebAgent.IsMobile())
            {
                list = list.Where(t => t.IsMobile);
            }
            else
            {
                list = list.Where(t => t.IsPC);
            }
            if (list.Count() == 0)
            {
                context.Response.Write(false, "不支持游戏列表");
            }

            int count = QF("Count", 3);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderBy(t => Guid.NewGuid()).Take(count), t => new
            {
                t.Code,
                t.Name,
                t.Cover
            }));
        }

        /// <summary>
        /// 游戏登录
        /// </summary>
        /// <param name="context" name="</param>
        [Guest]
        private void login(HttpContext context)
        {
            GameSetting setting = GameAgent.Instance().GetGameSettingInfo(QF("Type").ToEnum<GameType>());
            if (!setting.IsOpen)
            {
                context.Response.Write(false, "游戏未开放");
            }

            int userid = 0;
            if (UserInfo != null)
            {
                userid = UserInfo.ID;
            }
            else
            {
                userid = UserAgent.Instance().GetUserID(QF("Session", Guid.Empty));
            }
            if (userid == 0) { context.Response.Write(false, "请先登录"); }

            setting.Setting.FastLogin(userid, QF("Key"));
        }

        /// <summary>
        /// 额度转账信息
        /// </summary>
        /// <param name="context" name="</param>
        private void transferinfo(HttpContext context)
        {

            List<GameAccount> accountList = UserAgent.Instance().GetGameAccount(UserInfo.ID);
            IEnumerable<GameType> gameList = GameAgent.Instance().GetGameSetting().FindAll(t => t.IsOpen).Select(t => t.Type);

            List<string> list = new List<string>();

            list.Add(string.Concat("{\"Name\":\"主账户\",\"Type\":\"\",\"Money\":\"", UserInfo.Money, "\",\"Withdraw\":\"", UserInfo.Money, "\"}"));

            foreach (GameAccount account in accountList)
            {
                GameSetting setting = GameAgent.Instance().GetGameSettingInfo(account.Type);
                if (setting.Turnover == decimal.Zero) account.Withdraw = account.Money;
                list.Add(string.Concat("{\"Name\":\"", account.Type.GetDescription(), "\",\"Type\":\"", account.Type, "\",\"Money\":\"", account.Money, "\", \"Withdraw\":\"", Math.Max(decimal.Zero, Math.Min(account.Money, account.Withdraw)), "\"}"));
            }
            foreach (GameType game in gameList.Where(t => !accountList.Exists(p => p.Type == t)))
            {
                list.Add(string.Concat("{\"Name\":\"", game.GetDescription(), "\",\"Type\":\"", game, "\",\"disabled\":\"true\"}"));
            }

            context.Response.Write(true, this.StopwatchMessage(context), string.Format("[{0}]", string.Join(",", list)));
        }

        /// <summary>
        /// 账户间转账
        /// </summary>
        /// <param name="context" name="</param>
        [Guest]
        private void transfer(HttpContext context)
        {
            decimal money = QF("Money", decimal.Zero);

            string action = null;
            GameType type = (GameType)0;
            string outAccount = QF("OUT");
            string inAccount = QF("IN");

            if (string.IsNullOrEmpty(outAccount))
            {
                action = "IN";
                type = inAccount.ToEnum<GameType>();
            }
            else if (string.IsNullOrEmpty(inAccount))
            {
                action = "OUT";
                type = outAccount.ToEnum<GameType>();
            }
            int userId = UserInfo == null ? UserAgent.Instance().GetUserID(QF("Session", Guid.Empty)) : UserInfo.ID;
            if (userId == 0)
            {
                context.Response.Write(false, "请先登录");
            }
            this.ShowResult(context, GameAgent.Instance().Transfer(userId, money, type, action), "转账成功");
        }

        /// <summary>
        /// 批量转出至主账户
        /// </summary>
        /// <param name="context" name="</param>
        private void transferall(HttpContext context)
        {
            List<GameAccount> accountList = UserAgent.Instance().GetGameAccount(UserInfo.ID);
            List<string> result = new List<string>();
            foreach (GameAccount account in accountList.Where(t => t.Money > 1M && t.Withdraw > 1))
            {
                decimal money = Math.Min(account.Money, account.Withdraw);
                GameAgent.Instance().MessageClean();
                if (GameAgent.Instance().Transfer(UserInfo.ID, money, account.Type, "OUT"))
                {
                    result.Add(string.Format("{0}转出成功{1}元", account.Type.GetDescription(), money.ToString("c")));
                }
                else
                {
                    result.Add(string.Format("{0}转出失败，原因：{1}", account.Type.GetDescription(), GameAgent.Instance().Message()));
                }
            }

            context.Response.Write(true, string.Join("<br />", result));
        }

        /// <summary>
        /// 刷新余额
        /// </summary>
        /// <param name="context" name="</param>
        private void updatemoney(HttpContext context)
        {
            GameType type = QF("Type").ToEnum<GameType>();

            this.ShowResult(context, UserAgent.Instance().UpdateGameAccountMoney(UserInfo.ID, type), "刷新成功", new
            {
                Money = UserAgent.Instance().GetGameAccountInfo(UserInfo.ID, type).Money
            });
        }

        /// <summary>
        /// 真人游戏日志
        /// </summary>
        /// <param name="context" name="</param>
        private void videolog(HttpContext context)
        {
            IQueryable<VideoLog> list = BDC.VideoLog.Where(t => t.SiteID == SiteInfo.ID && t.StartAt > QF("StartAt", SiteInfo.StartDate) && t.StartAt < QF("EndAt", DateTime.Now).AddDays(1));

            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<GameType>());

            switch (QF("View", 0))
            {
                case 1:
                    // 直属下级
                    list = list.Join(BDC.UserDepth.Where(t => t.UserID == UserInfo.ID && t.Depth == 1), t => t.UserID, t => t.UserID, (log, depth) => log);
                    break;
                case 2:
                    // 所有下级
                    list = list.Join(BDC.UserDepth.Where(t => t.UserID == UserInfo.ID), t => t.UserID, t => t.ChildID, (log, depth) => log);
                    break;
                case 3:
                    // 指定下级
                    list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")) && BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                default:
                    list = list.Where(t => t.UserID == UserInfo.ID);
                    break;
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.BillNo,
                t.GameName,
                t.PlayType,
                t.BetAmount,
                t.Money,
                t.Balance,
                t.StartAt
            }, new
            {
                BetAmount = this.Show(list.Sum(t => (decimal?)t.BetAmount)),
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 真人视讯信息
        /// </summary>
        /// <param name="context" name="</param>
        private void videoinfo(HttpContext context)
        {
            VideoLog log = BDC.VideoLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == QF("ID", 0)).FirstOrDefault();

            if (log == null || !UserAgent.Instance().IsUserChild(UserInfo.ID, log.UserID))
            {
                context.Response.Write(false, "编号错误");
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Type = log.Type.GetDescription(),
                UserName = UserAgent.Instance().GetUserName(log.UserID),
                log.BillNo,
                log.GameName,
                log.PlayType,
                log.BetAmount,
                log.Money,
                log.Balance,
                log.StartAt
            });
        }

        /// <summary>
        /// 电子游戏日志
        /// </summary>
        /// <param name="context" name="</param>
        private void slotlog(HttpContext context)
        {
            IQueryable<SlotLog> list = BDC.SlotLog.Where(t => t.SiteID == SiteInfo.ID && t.PlayAt > QF("StartAt", SiteInfo.StartDate) && t.PlayAt < QF("EndAt", DateTime.Now).AddDays(1));

            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<GameType>());

            switch (QF("View", 0))
            {
                case 1:
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.Depth == 1).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 2:
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 3:
                    list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")) && BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                default:
                    list = list.Where(t => t.UserID == UserInfo.ID);
                    break;
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.BillNo,
                t.GameName,
                t.BetAmount,
                t.Money,
                t.Balance,
                t.PlayAt
            }, new
            {
                BetAmount = this.Show(list.Sum(t => (decimal?)t.BetAmount)),
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 电子游艺详情
        /// </summary>
        /// <param name="context" name="</param>
        private void slotinfo(HttpContext context)
        {
            SlotLog log = BDC.SlotLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == QF("ID", 0)).FirstOrDefault();
            if (log == null || !UserAgent.Instance().IsUserChild(UserInfo.ID, log.UserID))
            {
                context.Response.Write(false, "编号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                log.ID,
                Type = log.Type.GetDescription(),
                UserName = UserAgent.Instance().GetUserName(log.UserID),
                log.BillNo,
                log.GameName,
                log.BetAmount,
                log.Money,
                log.Balance,
                log.PlayAt
            });

        }

        /// <summary>
        /// 体育日志
        /// </summary>
        /// <param name="context" name="</param>
        private void sportlog(HttpContext context)
        {
            IQueryable<SportLog> list = BDC.SportLog.Where(t => t.SiteID == SiteInfo.ID && t.PlayAt > QF("StartAt", SiteInfo.StartDate) && t.PlayAt < QF("EndAt", DateTime.Now).AddDays(1));
            switch (QF("View", 0))
            {
                case 1:
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.Depth == 1).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 2:
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 3:
                    list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")) && BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                default:
                    list = list.Where(t => t.UserID == UserInfo.ID);
                    break;
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.PlayAt), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.WagersID,
                t.GameType,
                t.BetAmount,
                t.BetMoney,
                t.Money,
                t.PlayAt,
                t.Result
            }, new
            {
                BetMoney = this.Show(list.Sum(t => (decimal?)t.BetMoney)),
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 体育注单详情
        /// </summary>
        /// <param name="context" name="</param>
        private void sportinfo(HttpContext context)
        {
            SportLog log = BDC.SportLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == QF("ID", 0)).FirstOrDefault();
            if (log == null || !UserAgent.Instance().IsUserChild(UserInfo.ID, log.UserID))
            {
                context.Response.Write(false, "订单错误");
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Type = log.Type.GetDescription(),
                UserName = UserAgent.Instance().GetUserName(log.UserID),
                log.WagersID,
                log.GameType,
                log.BetAmount,
                log.BetMoney,
                log.Money,
                log.PlayAt,
                log.Result
            });
        }

        /// <summary>
        /// 获取兑换筹码的订单详情
        /// </summary>
        /// <param name="context" name="</param>
        private void casinoinfo(HttpContext context)
        {
            int id = QF("ID", 0);
            TransferLog log = GameAgent.Instance().GetTransferInfo(id, SiteInfo.ID);
            if (log == null || log.UserID != UserInfo.ID || log.Type != GameType.Casino || log.Status != TransferLog.TransferStatus.None)
            {
                context.Response.Write(false, "编号错误");
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                log.ID,
                Key = string.Concat(log.ID, "-", MD5.Encrypto(log.ID.ToString())),
                Money = log.Money
            });
        }

        /// <summary>
        /// 获取转账订单状态
        /// </summary>
        /// <param name="context" name="</param>
        private void gettransferstatus(HttpContext context)
        {
            int id = QF("ID", 0);
            TransferLog.TransferStatus? status = BDC.TransferLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).Select(t => (TransferLog.TransferStatus?)t.Status).FirstOrDefault();
            if (status == null)
            {
                context.Response.Write(false, "编号错误");
            }
            context.Response.Write(true, status.Value.GetDescription(), new
            {
                Status = status.Value
            });
        }
    }
}
