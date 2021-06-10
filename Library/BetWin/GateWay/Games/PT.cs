using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;

using SP.Studio.Json;
using BW.Agent;
using BW.Common.Users;
using SP.Studio.Net;
using SP.Studio.Text;

namespace BW.GateWay.Games
{
    public sealed class PT : IGame
    {
        public PT() : base() { }

        public PT(string setting) : base(setting) { }

        /// <summary>
        ///  游戏接口URL前缀，带/
        ///  https://kioskpublicapi.redhorse88.com/player/create/playername/VBETCNYTEST/adminname/YOURNAME/kioskname/YOURNAME/password/123456
        /// </summary>
        private const string ApiUrl = "https://kioskpublicapi.redhorse88.com/";

        public override bool CreateUser(int userId, params object[] args)
        {
            string playerName = this.GetPlayerName(userId);
            if (!string.IsNullOrEmpty(playerName)) return true;

            string playername = string.Concat(this.Prefix, userId.ToString().PadLeft(6, '0'), Guid.NewGuid().ToString("N").Substring(0, 4)).ToUpper();
            string password = Guid.NewGuid().ToString("N").Substring(0, 6);

            string url = this.GetUrl("player", "create", "playername", playername, "adminname", this.AgentKey, "kioskname", this.AgentCode, "password", password);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result, "URL", url);

            string msg;
            bool isSuccess = this.GetInfo(result, out msg);
            if (!isSuccess)
            {
                this.Message(msg);
                return false;
            }

            return UserAgent.Instance().AddGameAccount(userId, this.Type, playername, password);
        }

        public override decimal GetBalance(int userId)
        {
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName))
            {
                base.Message("当前用户未开通游戏账户");
                return decimal.MinusOne;
            }
            string url = this.GetUrl("player", "info", "playername", playerName);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result);
            string msg;
            if (!this.GetInfo(result, out msg))
            {
                base.Message(msg);
                return decimal.MinusOne;
            }
            return StringAgent.GetString(result, @"""BALANCE"":""([\d\.]+)""", Decimal.MinusOne);
        }

        public override bool Deposit(int userId, decimal money, string id, out decimal amount)
        {
            amount = 0;
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName)) return false;

            string url = this.GetUrl("player", "deposit", "playername", playerName, "adminname", this.AgentCode,
              "amount", money.ToString("0.00"), "externaltranid", id);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            string msg;
            bool isSuccess = this.GetInfo(result, out msg);
            this.SaveLog(userId, result, "URL", url);
            if (!isSuccess)
            {
                this.Message(msg);
                return false;
            }
            // {"result":{"amount":"166.00","currentplayerbalance":"166","executiontime":"227.898 ms","externaltransactionid":"160619201516914057","instantcashtype":null,"kiosktransactionid":"410844686","kiosktransactiontime":"2016-06-19 20:15:19","ptinternaltransactionid":"48104742603","result":"Deposit OK"}}
            // 调用查询接口获取是否充值成功
            url = this.GetUrl("player", "checktransaction", "externaltransactionid", id);
            result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result, "URL", url);
            amount = StringAgent.GetString(result, @"""currentplayerbalance"":""([\d\.]+)""", decimal.MinusOne);
            return true;
        }

        public override bool Withdraw(int userId, decimal money, string orderId, out decimal amount)
        {
            amount = 0.00M;
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName)) return false;

            string url = this.GetUrl("player", "withdraw", "playername", playerName, "adminname", this.AgentKey, "amount", money.ToString("0.00"), "isForce", "1", "externaltranid", orderId);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result);
            string msg;
            if (!this.GetInfo(result, out msg))
            {
                base.Message(msg);
                return false;
            }
            //{"result":{"amount":"270.00","currentplayerbalance":"4.5","executiontime":"267.632 ms","externaltransactionid":"160619212006854349","instantcashtype":null,"kiosktransactionid":"410879265","kiosktransactiontime":"2016-06-19 21:20:09","ptinternaltransactionid":"48114526323","result":"Withdraw OK"}}

            amount = StringAgent.GetString(result, @"""currentplayerbalance"":""([\d\.]+)""", decimal.MinusOne);
            if (amount == decimal.MinusOne && result.Contains("\"currentplayerbalance\":null"))
            {
                amount = decimal.Zero;
            }
            return true;
        }

        public override TransferStatus CheckTransfer(int userId, string id)
        {
            //https://kioskpublicapi.redhorse88.com/player/checktransaction/externaltransactionid/w123456
            string url = this.GetUrl("player", "checktransaction", "externaltransactionid", id);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result);

            if (result.Contains("\"approved\""))
            {
                return TransferStatus.Success;
            }
            else if (result.Contains("\"missing\""))
            {
                return TransferStatus.Faild;
            }

            return TransferStatus.None;
        }

        public override void FastLogin(int userId, string key)
        {
            GameAccount gameAccount = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (gameAccount == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>")
                .Append("<head>")
                .Append("<title>正在进入游戏</title>")
                .Append("</head><body>")
                .AppendFormat("<form id=\"form1\" action=\"{0}ac=game\" method=\"post\">", this.Gateway)
                .AppendFormat("<input type=\"hidden\" name=\"PlayerName\" value=\"{0}\" />", gameAccount.PlayerName)
                .AppendFormat("<input type=\"hidden\" name=\"Password\" value=\"{0}\" />", gameAccount.Password)
                .AppendFormat("<input type=\"hidden\" name=\"Key\" value=\"{0}\" />", key)
                .AppendFormat("</form>")
                .AppendFormat("<script type=\"text/javascript\">")
                .Append("document.getElementById('form1').submit();")
                .Append("</script>")
                .Append("正在进入游戏...")
                .Append("</body></html>");

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }



        #region ==========  私有方法 ==============

        /// <summary>
        /// 生成PT所需要的参数格式
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        private string GetUrl(params string[] paras)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < paras.Length; i += 2)
            {
                list.Add(string.Format("{0}={1}", paras[i], paras[i + 1]));
            }
            return this.Gateway + string.Join("&", list);

        }

        /// <summary>
        /// 获取返回json里面的内容
        /// </summary>
        /// <param name="json"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool GetInfo(string json, out string msg)
        {
            // success: {"playername":"ZELTESTPLAYER","password":"macwon62"}
            // error:   {"error":"Player name is incorrect or player already exists in system","errorcode":42}
            Hashtable ht = JsonAgent.GetJObject(json);
            msg = string.Empty;
            json = json.ToLower();
            // 远程服务器返回错误
            if (ht == null)
            {
                msg = json;
                return false;
            }
            if (ht.ContainsKey("error"))
            {
                msg = this.getErrorMsg(ht["errorcode"].ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// 错误信息转为友好提示信息
        /// </summary>
        /// <param name="errorcode"></param>
        /// <returns></returns>
        private string getErrorMsg(string errorcode)
        {
            String msg = "未知明错误";

            // 输出友好信息
            switch (errorcode.ToLower())
            {
                case "1":
                    // declined
                    msg = "操作被拒绝";
                    break;
                case "16":
                    // Real money casino account already exists with this serial
                    msg = "游戏账号已存在";
                    break;
                case "19":
                    //The username you requested is already being used by another player
                    msg = "开户的游戏帐号已被其他账号使用";
                    break;
                case "7":
                    // Internal login error
                    msg = "内部登录错误";
                    break;
                case "8":
                    // Unknown username
                    msg = "账号不存在";
                    break;
                case "202":
                    // Invalid secret key
                    msg = "secret key无效";
                    break;
                case "4":
                    // declined
                    msg = "取款超过余额";
                    break;
                case "5":
                    // declined。存款与取款的时候
                    msg = "单笔存款或取款超过限额";
                    break;
                case "2002":
                    // declined。存款与取款的时候
                    msg = "总存款或取款超过限额";
                    break;
                case "local_001":
                    // 自定义
                    msg = "获取账户余额失败";
                    break;
                case "was not found in the haystack":
                    msg = "参数不存在";
                    break;
                case "42":
                    // Player name is incorrect or player already exists in system
                    msg = "账号不正确或者账号已存在";
                    break;
                case "41":
                    // Player does not exist
                    msg = "账号不存在";
                    break;
                case "48":
                    //{"error":"Kiosk admin does not belong to TLE","errorcode":48}
                    msg = "Kiosk admin does not belong to TLE";
                    break;
                case "72":
                    // Amount is over current player balance
                    msg = "账号余额不足";
                    break;
                case "97":
                    msg = "平台额度不足，请与客服联系";
                    break;
                case "99":
                    // Player is in game
                    msg = "正在游戏中，不允许提款";
                    break;
                case "110":
                    msg = "Entity not from specified TLE";
                    break;
                case "302":
                    msg = "转账编号已存在，请重试";
                    break;
                default:
                    msg = errorcode;
                    break;
            }

            return msg;
        }


        #endregion

        #region =========== 接口配置属性  ==============

        [Description("adminname")]
        public string AgentKey { get; set; }

        [Description("kioskname")]
        public string AgentCode { get; set; }

        [Description("用户名前缀")]
        public string Prefix { get; set; }

        /// <summary>
        /// 远程接口地址
        /// </summary>
        [Description("远程接口")]
        public string Gateway { get; set; }

        #endregion
    }
}
