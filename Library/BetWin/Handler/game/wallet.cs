using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BW.Agent;
using SP.Studio.Array;
using BW.GateWay.Games;
using SP.Studio.Model;
using BW.Common.Games;
using BW.Common.Users;
using SP.Studio.Security;

using SP.Studio.Web;
using SP.Studio.Json;

namespace BW.Handler.game
{
    /// <summary>
    /// 免转钱包接口
    /// </summary>
    public class wallet : IHandler
    {
        /// <summary>
        /// 贝盈电竞的免转钱包
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void bwgaming(HttpContext context)
        {
            string data = Encoding.UTF8.GetString(WebAgent.GetInputSteam(context));
            if (string.IsNullOrEmpty(data))
            {
                context.Response.Write(false, "请求内容错误");
            }
            JObject info = (JObject)JsonConvert.DeserializeObject(data);
            BWGaming game = (BWGaming)GameAgent.Instance().GetGameSettingInfo(GameType.BWGaming, SiteInfo.ID).Setting;
            string key = game.SECRETKEY;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (KeyValuePair<string, JToken> item in info)
            {
                dic.Add(item.Key, item.Value.Value<string>());
            }
            if (!dic.ContainsKey("sign"))
            {
                context.Response.Write(false, "没有密钥字段");
            }
            string signStr = dic.Where(t => t.Key != "sign").OrderBy(t => t.Key).ToQueryString() + game.SECRETKEY;
            if (MD5.toMD5(signStr).ToLower() != dic["sign"])
            {
                context.Response.Write(false, "密钥错误");
            }

            string type = info["Type"].Value<string>();
            string userName = info["UserName"].Value<string>();
            int userId = UserAgent.Instance().GetUserID(GameType.BWGaming, userName);
            if (userId == 0)
            {
                context.Response.Write(false, "找不到游戏帐号" + userName);
            }
            switch (type)
            {
                case "Balance":
                    context.Response.Write(true, "查询余额", new
                    {
                        Balance = UserAgent.Instance().GetUserMoney(userId)
                    });
                    break;
                // 投注
                case "Bet":
                    //{"UserName":"00000216" ,"Money":"10" ,"Type":"Bet" ,"OrderID":"2468" ,"timestamp":"1525670153" ,"sign":"b641568c998bc9bdc9c0f69a2c148d75" }
                    if (!UserAgent.Instance().ExistsMoneyLog(userId, MoneyLog.MoneyType.GameBWBet, info["OrderID"].Value<int>()))
                    {
                        this.ShowResult(context, UserAgent.Instance().AddMoneyLog(userId, info["Money"].Value<decimal>() * -1, MoneyLog.MoneyType.GameBWBet,
                            info["OrderID"].Value<int>(), string.Format("电竞投注，订单编号：{0}", info["OrderID"].Value<string>())),
                       "交易成功", new
                       {
                           Balance = UserAgent.Instance().GetUserMoney(userId)
                       });
                    }
                    else
                    {
                        context.Response.Write(true, "交易成功");
                    }
                    break;
                case "Reward":
                    //{"UserName":"00000216" ,"Money":"19.00000000" ,"Type":"Reward" ,"OrderID":"2418" ,"timestamp":"1525688218" ,"sign":"9ed8ade116e323c3b34c7ecf775674ce" }
                    if (!UserAgent.Instance().ExistsMoneyLog(userId, MoneyLog.MoneyType.GameBWReward, info["OrderID"].Value<int>()))
                    {
                        this.ShowResult(context, UserAgent.Instance().AddMoneyLog(userId, info["Money"].Value<decimal>(), MoneyLog.MoneyType.GameBWReward,
                                info["OrderID"].Value<int>(), string.Format("电竞奖金，订单编号：{0}", info["OrderID"].Value<string>())),
                           "交易成功", new
                           {
                               Balance = UserAgent.Instance().GetUserMoney(userId)
                           });
                    }
                    else
                    {
                        context.Response.Write(true, "交易成功");
                    }
                    break;
                case "money":
                    //{"UserName":"00000216" ,"Money":"-10" ,"Description":"全场比赛输赢,订单编号：1043" ,"SourceID":"2390" ,"Type":"Bet" }
                    MoneyLog.MoneyType moneyType = MoneyLog.MoneyType.None;
                    switch (info["Type"].Value<string>())
                    {
                        case "Bet":
                            moneyType = MoneyLog.MoneyType.GameBWBet;
                            break;
                        case "Reward":
                            moneyType = MoneyLog.MoneyType.GameBWReward;
                            break;
                    }
                    if (moneyType == MoneyLog.MoneyType.None)
                    {
                        context.Response.Write(false, "未指定资金类型");
                    }
                    this.ShowResult(context, UserAgent.Instance().AddMoneyLog(userId, info["Money"].Value<decimal>(), moneyType, info["SourceID"].Value<int>(), info["Description"].Value<string>()),
                        "交易成功", new
                        {
                            Balance = UserAgent.Instance().GetUserMoney(userId)
                        });
                    break;
                default:
                    context.Response.Write(false, "未指定执行类型");
                    break;
            }
        }
    }
}
