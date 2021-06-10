using BW.Agent;
using Newtonsoft.Json.Linq;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Web;
using SP.Studio.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace BW.Common.Games
{
    /// <summary>
    /// 日志管理
    /// </summary>
    public partial class VideoLog
    {
        public VideoLog() { }

        /// <summary>
        /// 从文本文件中读取日志内容
        /// </summary>
        /// <param name="type"></param>
        /// <param name="item"></param>
        /// <param name="siteId">当前站点ID，在非web程序中调用必须</param>
        public VideoLog(GameType type, object item)
        {
            this.Type = type;
            switch (this.Type)
            {
                case GameType.AG:

                    //<row dataType="BR"  billNo="160524003978917" playerName="000001" 
                    //agentCode="842001001001001" gameCode="GA0031652400Q" netAmount="-20" betTime="2016-05-24 00:43:48" gameType="LINK" 
                    //betAmount="20" validBetAmount="20" flag="1" playType="2" currency="CNY" tableCode="A003" loginIP="10.20.1.216" 
                    //recalcuTime="2016-05-24 00:44:06" platformType="AGIN" remark="" round="AGQ" result="" beforeCredit="20" deviceType="1" />

                    XElement ag = (XElement)item;
                    if (ag.GetAttributeValue("dataType") != "BR") return;
                    this.BillNo = ag.GetAttributeValue("billNo");
                    this.UserID = UserAgent.Instance().GetUserID(type, ag.GetAttributeValue("playerName"));
                    this.GameCode = ag.GetAttributeValue("gameCode");
                    this.Money = ag.GetAttributeValue("netAmount", 0.00M);
                    this.GameName = GameAgent.Instance().GetValue(this.Type, "gameType", ag.GetAttributeValue("gameType"));
                    this.BetAmount = ag.GetAttributeValue("validBetAmount", 0.00M);
                    this.Status = GameAgent.Instance().GetValue(this.Type, "flag", ag.GetAttributeValue("flag")).ToEnum<LogStatus>();
                    this.PlayType = GameAgent.Instance().GetValue(this.Type, "playType", ag.GetAttributeValue("playType"));
                    this.StartAt = ag.GetAttributeValue("betTime", DateTime.MinValue).AddHours(12);
                    this.EndAt = ag.GetAttributeValue("recalcuTime", DateTime.MinValue).AddHours(12);
                    this.Balance = ag.GetAttributeValue("beforeCredit", decimal.Zero) + this.Money;

                    break;

                case GameType.BBIN:

                    //<Record>    <UserName>hn8s991015</UserName>    <WagersID>9227090196</WagersID>    <WagersDate>2016-06-19 01:46:11</WagersDate>    
                    //<SerialID>87215305</SerialID>    <RoundNo>2-146</RoundNo>    <GameType>3008</GameType>    
                    //<WagerDetail>1,1:1,15.00,-15.00*47,1:1,1.00,1.00*48,1:1,1.00,-1.00*49,1:1,1.00,-1.00</WagerDetail>    <GameCode>3</GameCode>    <Result>2,6,6</Result>    
                    //<Card>2,6,6</Card>    <BetAmount>18</BetAmount>    <Origin></Origin>    <Commissionable>18</Commissionable>    
                    //<Payoff>-16</Payoff>    <Currency>RMB</Currency>    <ExchangeRate>1.000000</ExchangeRate>    <ResultType></ResultType>  </Record>

                    XElement bbin = (XElement)item;
                    this.BillNo = bbin.GetValue("WagersID");
                    this.UserID = UserAgent.Instance().GetUserID(type, bbin.GetValue("UserName"));
                    this.GameCode = bbin.GetValue("SerialID");
                    this.Money = bbin.GetValue("Payoff", 0.00M);
                    this.GameName = GameAgent.Instance().GetValue(this.Type, "gameType", bbin.GetValue("GameType"));
                    this.PlayType = bbin.GetValue("WagerDetail");
                    Regex playType = new Regex(@"^(?<Type>\d+)");
                    if (playType.IsMatch(this.PlayType))
                    {
                        this.PlayType = GameAgent.Instance().GetValue(this.Type, "playType", bbin.GetValue("GameType") + "-" + playType.Match(this.PlayType).Groups["Type"].Value);
                    }
                    this.BetAmount = bbin.GetValue("Commissionable", 0.00M);
                    this.Status = LogStatus.Finish;
                    this.StartAt = bbin.GetValue("WagersDate", DateTime.MinValue).AddHours(12);
                    this.EndAt = this.StartAt;
                    this.Balance = decimal.MinusOne;

                    break;

                case GameType.SunBet:
                    //"record_id": 4211,"player_name": "test0724","transaction_id": "1092875466","bet": 0.1,"win": 0.4,"ending_balance": 98.4,"jackpot_bet": 0,
                    //"jackpot_win": 0,"round_id": "544007560","session_id": "4933774","game_id": "eastereggs","time": "2017-07-24 13:38:43","platform": "GM"
                    Hashtable tgp = (Hashtable)item;
                    this.ExtendXML = tgp.ToJson();
                    this.BillNo = tgp["record_id"].ToString();
                    this.UserID = UserAgent.Instance().GetUserID(type, tgp["player_name"].ToString());
                    this.GameCode = tgp["session_id"].ToString();
                    this.BetAmount = tgp.GetValue("bet", decimal.Zero);
                    this.Money = tgp.GetValue("win", decimal.Zero) - this.BetAmount;
                    this.GameName = GameAgent.Instance().GetValue(this.Type, "gameType", tgp.GetValue("game_id", string.Empty));
                    this.Status = LogStatus.Finish;
                    this.StartAt = tgp.GetValue("time", DateTime.MinValue);
                    this.EndAt = this.StartAt;
                    this.Balance = tgp.GetValue("ending_balance", decimal.MinusOne);
                    break;
                //{ "gameprovider":"og","membername":"boqt_ceshi01404","gamename":"Baccarat","bettingcode":"10932556395","bettingdate":"\/Date(1552298303000)\/",
                // "gameid":"C1","roundno":"11-2","result":null,"bet":"101","winloseresult":"2","bettingamount":10.000,"validbet":10.000,"winloseamount":10.000,
                // "balance":30.000,"currency":"RMB","handicap":null,"status":"101^10.0^10.0^,","gamecategory":"live","settledate":null,"remark":null}
                case GameType.OG:

                    JObject og = (JObject)item;
                    this.ExtendXML = og.ToString();
                    this.BillNo = og["bettingcode"].Value<string>();
                    this.UserID = this.GetOGUserID(og["membername"].Value<string>());
                    this.GameCode = og["roundno"].Value<string>();
                    this.PlayType = WebAgent.LeftString(og["status"].Value<string>(), 50);
                    this.BetAmount = og["validbet"].Value<decimal>();
                    this.Money = og["winloseamount"].Value<decimal>();
                    this.GameName = og["gamename"].Value<string>();
                    this.Status = LogStatus.Finish;
                    this.EndAt = this.StartAt = og["bettingdate"].Value<DateTime>().AddHours(8);
                    this.Balance = og["balance"].Value<decimal>();

                    break;
            }

            this.SiteID = UserAgent.Instance().GetSiteID(this.UserID);
            this.CreateAt = DateTime.Now;
            if (string.IsNullOrEmpty(this.ExtendXML)) this.ExtendXML = item.ToString();
        }



        private int GetOGUserID(string membername)
        {
            Regex regex = new Regex(@"_(?<GameName>\w+)$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(membername)) return 0;
            string playerName = regex.Match(membername).Groups["GameName"].Value;
            return UserAgent.Instance().GetUserID(GameType.OG, playerName);
        }
    }
}
