using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SP.Studio.Xml;

using SP.Studio.Array;
using SP.Studio.Text;
using BW.Agent;
using SP.Studio.Core;
using SP.Studio.Json;

namespace BW.Common.Games
{
    partial class SlotLog
    {
        public SlotLog() { }

        public SlotLog(GameType type, object item)
        {
            this.Type = type;

            switch (type)
            {
                case GameType.AG:
                    XElement ag = (XElement)item;

                    switch (ag.GetAttributeValue("dataType"))
                    {
                        case "HSR": // 捕鱼王
                            //<row dataType="HSR" ID="H_57bdcc38fcb3b8c86a6153f2" tradeNo="57bdcc38fcb3b8c86a6153f2" platformType="HUNTER" sceneId="1991472055843" playerName="020230a50c" 
                            //type="1" SceneStartTime="2016-08-24 12:24:03" SceneEndTime="2016-08-24 12:29:39" Roomid="199" Roombet="1.0" Cost="7164" Earn="3962" Jackpotcomm="35.82" transferAmount="-3202" 
                            //previousAmount="3202.67" currentAmount="0.67" currency="CNY" exchangeRate="1" IP="" flag="0" creationTime="2016-08-24 12:29:39" gameCode="" />

                            this.UserID = UserAgent.Instance().GetUserID(type, ag.GetAttributeValue("playerName"));
                            this.BillNo = ag.GetAttributeValue("tradeNo");
                            this.GameName = "捕鱼王";
                            this.PlayAt = ag.GetAttributeValue("SceneEndTime", DateTime.MinValue).AddHours(12);
                            this.BetAmount = Math.Abs(ag.GetAttributeValue("transferAmount", decimal.Zero));
                            this.Money = ag.GetAttributeValue("transferAmount", decimal.Zero);
                            this.Balance = ag.GetAttributeValue("currentAmount", decimal.Zero);
                            this.Status = LogStatus.Finish;
                            this.ExtendXML = item.ToString();
                            break;
                        case "EBR": // 电子游戏
                        case "BR":  // 新老虎机
                            //<row dataType="EBR" billNo="160824054681115" playerName="020230a50c" agentCode="842001001001001" gameCode="" netAmount="30" betTime="2016-08-24 05:59:11"
                            //gameType="FRU" betAmount="120" validBetAmount="120" flag="1" playType="null" currency="CNY" tableCode="null" loginIP="111.73.46.151" recalcuTime="2016-08-24 05:59:11"
                            //platformType="XIN" remark="null" round="SLOT" slottype="1" result="4;1,Apple,150.00;30.00,30.00,20.00,20.00,0.00,0.00,20.00,0.00" 
                            //mainbillno="160824054681115" beforeCredit="" betAmountBase="120" betAmountBonus="0" netAmountBase="30" netAmountBonus="0" deviceType="0" />

                            //<row dataType="BR" billNo="9171016002961315" playerName="3553291f5c" agentCode="A88001001001001" gameCode="" netAmount="-2.1" betTime="2017-10-16 00:31:54"
                            //gameType ="YBIR" betAmount="2.1" validBetAmount="2.1" flag="1" playType="null" currency="CNY" tableCode="null" loginIP="114.238.182.26" recalcuTime ="2017-10-16 00:31:54" 
                            //platformType ="YOPLAY" remark="null" round="SLOT" slottype="1" result="0;3;23;20;11_2.10;" 
                            //mainbillno="" beforeCredit="84.13" betAmountBase="2.1" betAmountBonus="0" netAmountBase="-2.1" netAmountBonus="0" deviceType="0" />
                            this.UserID = UserAgent.Instance().GetUserID(type, ag.GetAttributeValue("playerName"));
                            this.BillNo = ag.GetAttributeValue("billNo");
                            this.GameName = GameAgent.Instance().GetValue(this.Type, "gameType", ag.GetAttributeValue("gameType"));
                            this.PlayAt = ag.GetAttributeValue("betTime", DateTime.MinValue).AddHours(12);
                            this.BetAmount = ag.GetAttributeValue("validBetAmount", decimal.Zero);
                            this.Money = ag.GetAttributeValue("netAmount", decimal.Zero);
                            this.Balance = ag.GetAttributeValue("beforeCredit", decimal.MinusOne);  // 获取不到余额
                            this.ExtendXML = item.ToString();
                            break;



                    }
                    break;
                case GameType.PT:
                    Hashtable ht = (Hashtable)item;
                    //{"PLAYERNAME":"HN8037511E6F7","WINDOWCODE":"0","GAMEID":"4","GAMECODE":"205004615358",
                    //    "GAMETYPE":"Slot Machines","GAMENAME":"Zhao Cai Tong Zi (zctz)","SESSIONID":"464800097412",
                    //    "BET":".2","WIN":".4","PROGRESSIVEBET":"0","PROGRESSIVEWIN":"0","BALANCE":".68"
                    //    ,"CURRENTBET":"0","GAMEDATE":"2016-08-22 21:28:56","LIVENETWORK":null}

                    this.UserID = UserAgent.Instance().GetUserID(type, ht["PLAYERNAME"].ToString());
                    this.BillNo = ht["GAMECODE"].ToString();
                    this.GameName = GameAgent.Instance().GetGameName(type, StringAgent.GetString(ht["GAMENAME"].ToString(), "(", ")"));
                    this.PlayAt = ht.GetValue("GAMEDATE", DateTime.MinValue);
                    this.BetAmount = ht.GetValue("BET", decimal.Zero);
                    this.Money = ht.GetValue("WIN", decimal.Zero) - this.BetAmount;
                    this.Balance = ht.GetValue("BALANCE", decimal.MinusOne);
                    this.Status = LogStatus.Finish;

                    XElement log = new XElement("root");
                    foreach (DictionaryEntry de in ht)
                    {
                        XElement ptItem = new XElement(de.Key.ToString());
                        ptItem.Value = de.Value.ToString();
                        log.Add(ptItem);
                    }
                    this.ExtendXML = log.ToString();
                    break;
                case GameType.BBIN:
                    XElement bbin = (XElement)item;
                    //<Record><UserName>310140c030</UserName><WagersID>130873237955</WagersID><WagersDate>2017-03-28 13:29:48</WagersDate>
                    //<GameType>5907</GameType><Result>1</Result><BetAmount>3</BetAmount><Commissionable>2.9991</Commissionable>
                    //<Payoff>5.1</Payoff><Currency>RMB</Currency><ExchangeRate>1.000000</ExchangeRate></Record>
                    this.UserID = UserAgent.Instance().GetUserID(type, bbin.GetValue("UserName", string.Empty));
                    this.BillNo = bbin.GetValue("WagersID");
                    this.GameName = GameAgent.Instance().GetGameName(type, bbin.GetValue("GameType"));
                    this.PlayAt = bbin.GetValue("WagersDate", DateTime.MinValue).AddHours(12);
                    this.BetAmount = bbin.GetValue("BetAmount", decimal.Zero);
                    this.Money = bbin.GetValue("Payoff", decimal.Zero);
                    this.ExtendXML = bbin.ToString();
                    switch (bbin.GetValue("Result"))
                    {
                        case "0":
                        case "-77":
                            this.Status = LogStatus.None;
                            break;
                        case "-1":
                            this.Status = LogStatus.Error;
                            break;
                        default:
                            this.Status = LogStatus.Finish;
                            break;
                    }
                    break;
                case GameType.MW:
                    Hashtable mw = (Hashtable)item;
                    this.ExtendXML = mw.ToJson();
                    this.BillNo = mw["record_id"].ToString();
                    this.UserID = UserAgent.Instance().GetUserID(type, mw["player_name"].ToString());
                    this.BetAmount = mw.GetValue("bet", decimal.Zero);
                    this.Money = mw.GetValue("win", decimal.Zero) - this.BetAmount;
                    this.GameName = GameAgent.Instance().GetValue(this.Type, "gameType", mw.GetValue("game_id", string.Empty));
                    this.Status = LogStatus.Finish;
                    this.PlayAt = mw.GetValue("time", DateTime.MinValue);
                    this.Balance = mw.GetValue("ending_balance", decimal.MinusOne);
                    break;
                case GameType.MG:
                    Hashtable mg = (Hashtable)item;
                    string mg_game = string.Empty;
                    if (mg.ContainsKey("meta_data"))
                    {
                        mg_game = string.Format("{0}-{1}", mg["application_id"], JsonAgent.GetJObject(mg["meta_data"].ToString()).GetValue("item_id", 0));
                    }
                    this.ExtendXML = mg.ToJson();
                    this.BillNo = mg["id"].ToString();
                    this.UserID = UserAgent.Instance().GetUserID(type, mg["account_id"].ToString());
                    switch (mg["category"].ToString())
                    {
                        case "WAGER":
                            this.BetAmount = mg.GetValue("amount", decimal.Zero);
                            this.Money = this.BetAmount * -1;
                            break;
                        case "PAYOUT":
                            this.Money = mg.GetValue("amount", decimal.Zero);
                            break;
                    }
                    this.GameName = GameAgent.Instance().GetGameName(this.Type, mg_game);
                    this.Status = LogStatus.Finish;
                    this.PlayAt = mg.GetValue("created", DateTime.MinValue);
                    this.Balance = mg.GetValue("balance", decimal.Zero);
                    break;
            }

            this.SiteID = UserAgent.Instance().GetSiteID(this.UserID);
            this.CreateAt = DateTime.Now;

        }
    }
}
