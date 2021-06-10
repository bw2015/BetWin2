using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.GateWay.Games;
using BW.Common.Games;
using BW.Common.Users;
using BW.Agent;

using SP.Studio.Web;
using SP.Studio.Model;
using SP.Studio.Core;

namespace BW.Handler.user
{
    public class game : IHandler
    {
        /// <summary>
        /// 第三方游戏账户的信息
        /// </summary>
        /// <param name="context"></param>
        private void accountinfo(HttpContext context)
        {
            GameType type = QF("Type").ToEnum<GameType>();
            GameAccount account = UserAgent.Instance().GetGameAccountInfo(UserInfo.ID, type);
            if (account == null)
            {
                context.Response.Write(false, string.Format("暂未开通{0}账户", type.GetDescription()));
            }

            GameSetting setting = GameAgent.Instance().GetGameSettingInfo(type);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Money = UserInfo.Money,
                GameMoney = account.Money,
                GameWithdraw = setting.Turnover == decimal.Zero ? account.Money : account.Withdraw,
                Type = type,
                TypeName = type.GetDescription()
            });
        }

        /// <summary>
        /// 获取第三方游戏的账户密码
        /// </summary>
        /// <param name="context"></param>
        private void accountpassword(HttpContext context)
        {
            GameType type = QF("Type").ToEnum<GameType>();
            GameAccount account = UserAgent.Instance().GetGameAccountInfo(UserInfo.ID, type);
            if (account == null)
            {
                context.Response.Write(false, string.Format("暂未开通{0}账户", type.GetDescription()));
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Type = type,
                account.PlayerName,
                account.Password
            });
        }
    }
}
