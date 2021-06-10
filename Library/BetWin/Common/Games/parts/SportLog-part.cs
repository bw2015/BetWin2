using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using BW.Agent;
using BW.Common.Users;
using SP.Studio.Core;
using SP.Studio.Xml;

namespace BW.Common.Games
{
    partial class SportLog
    {
        public SportLog() { }

        public SportLog(GameType type, XElement item)
        {
            this.Type = type;
            switch (type)
            {
                //BB体育
                //                    <Record>
                //<UserName>3100109463</UserName>
                //<WagersID>454013283</WagersID>
                //<WagersDate>2017-03-28 08:11:37</WagersDate>
                //<GameType>FT</GameType>
                //<Result>LL</Result>
                //<BetAmount>8000</BetAmount>
                //<Payoff>-4000.00</Payoff>
                //<Commissionable>4000</Commissionable>
                //<Currency>RMB</Currency>
                //<ExchangeRate>1.00000000</ExchangeRate>
                //<Origin>P</Origin>
                //</Record>
                case Games.GameType.BBIN:
                    this.UserID = UserAgent.Instance().GetUserID(type, item.GetValue("UserName"));
                    this.WagersID = item.GetValue("WagersID");
                    this.SiteID = UserAgent.Instance().GetSiteID(this.UserID);
                    this.PlayAt = item.GetValue("WagersDate", DateTime.MinValue).AddHours(12);
                    this.GameType = item.GetValue("GameType");
                    this.BetAmount = item.GetValue("BetAmount", decimal.Zero);
                    this.BetMoney = item.GetValue("Commissionable", decimal.Zero);
                    this.Money = item.GetValue("Payoff", decimal.Zero);
                    this.Result = GameAgent.Instance().GetValue(type, "sportResult", item.GetValue("Result"));

                    switch (item.GetValue("Result"))
                    {
                        case "X":
                        case "S":
                            this.Status = LogStatus.None;
                            break;
                        case "N":
                        case "C":
                        case "F":
                            this.Status = LogStatus.Error;
                            this.ResultAt = DateTime.Now;
                            break;
                        default:
                            this.Status = LogStatus.Finish;
                            this.ResultAt = DateTime.Now;
                            break;
                    }
                    break;
                case Games.GameType.BWGaming:
                    //{"OrderID":"2229" ,"UserName":"你长的丑你先说You丑NoBB" ,"Category":"英雄联盟" ,"League":"LDL 2018" ,
                    //"Match":"SYX vs SN" ,"Bet":"谁会赢得比赛的胜利" ,"Content":"SYX" ,"Result":"" ,"BetAmount":"100.0000" ,
                    //"Money":"0.0000" ,"Status":"None" ,"CreateAt":"2018/3/16 10:51:43" ,"UpdateAt":"1900/1/1 0:00:00" }

                    this.UserID = UserAgent.Instance().GetUserID(type, item.GetAttributeValue("UserName"));
                    this.WagersID = item.GetAttributeValue("OrderID");
                    this.SiteID = UserAgent.Instance().GetSiteID(this.UserID);
                    this.PlayAt = item.GetAttributeValue("CreateAt", new DateTime(1900, 1, 1));
                    this.GameType = item.GetAttributeValue("Category");
                    this.BetAmount = item.GetAttributeValue("BetAmount", decimal.Zero);
                    this.BetMoney = item.GetAttributeValue("BetAmount", decimal.Zero);
                    this.Money = item.GetAttributeValue("Money", decimal.Zero);
                    this.Result = string.Format("{0} | {1}", item.GetAttributeValue("Content"), item.GetAttributeValue("Result"));

                    switch (item.GetAttributeValue("Status"))
                    {
                        case "None":
                            this.Status = LogStatus.None;
                            break;
                        case "Revoke":
                            this.Status = LogStatus.Cancel;
                            this.ResultAt = item.GetAttributeValue("UpdateAt", DateTime.Now);
                            break;
                        case "Win":
                        case "Lose":
                        case "WinHalf":
                        case "LoseHalf":
                            this.Status = LogStatus.Finish;
                            this.ResultAt = item.GetAttributeValue("UpdateAt", DateTime.Now);
                            break;
                    }

                    break;
            }
            this.ExtendXML = item.ToString();
        }
    }
}
