using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.Agent;
using System.Net;
using SP.Studio.Net;
using SP.Studio.Json;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Web;

namespace BW.GateWay.Games
{
    /// <summary>
    /// 申博
    /// </summary>
    public class SunBet : IGame
    {
        public SunBet() : base() { }

        public SunBet(string setting) : base(setting) { }

        private string apiDomain = "https://api.gmaster8.com";
        /// <summary>
        /// API域名
        /// </summary>
        public string APIDomain
        {
            get
            {
                return this.apiDomain;
            }
            set
            {
                this.apiDomain = value;
            }
        }

        /// <summary>
        /// API用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// API密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        /// 前缀
        /// </summary>
        public string Prefix { get; set; }

        private string POST(string platform, string method, Dictionary<string, string> data)
        {
            string url = apiDomain + "/" + (string.IsNullOrEmpty(platform) ? "" : platform + "/") + method;
            using (WebClient wc = new WebClient())
            {
                string auth = WebAgent.StringToBase64(string.Format("{0}:{1}", this.UserName, this.PassWord));
                wc.Headers.Add(HttpRequestHeader.Authorization, "Basic " + auth);
                return NetAgent.UploadData(url, string.Join("&", data.Select(t => string.Format("{0}={1}", t.Key, t.Value))), Encoding.UTF8, wc);
            }
        }

        private string POST(string method, Dictionary<string, string> data)
        {
            return this.POST(this.Type.ToString(), method, data);
        }

        /// <summary>
        /// 检查转账状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public override TransferStatus CheckTransfer(int userId, string id)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("externalTransactionId", id.PadLeft(13, '0'));
            string result = this.POST("credit/check_transaction", dic);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null)
            {
                if (result.Contains("\"error\":1014")) return TransferStatus.Faild;
                base.Message(result);
                return TransferStatus.None;
            }
            if (!ht.ContainsKey("status"))
            {
                base.Message(result);
                return TransferStatus.None;
            }

            TransferStatus status = TransferStatus.Other;
            switch (ht["status"].ToString())
            {
                case "failed":
                    status = TransferStatus.Faild;
                    break;
                case "approved":
                    status = TransferStatus.Success;
                    break;
            }
            return status;

        }

        /// <summary>
        /// 注册一个用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override bool CreateUser(int userId, params object[] args)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string username = this.Prefix + GameAgent.Instance().CreatePlayerName(userId);
            string password = Guid.NewGuid().ToString("N").Substring(0, 10);
            dic.Add("username", username);
            dic.Add("password", password);

            string result = this.POST(null, "register", dic);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("player_name"))
            {
                base.Message(result);
                return false;
            }
            dic.Clear();
            dic.Add("username", ht["player_name"].ToString());
            result = this.POST("player/active", dic);
            ht = JsonAgent.GetJObject(result);
            if (ht == null || ht.GetValue("status", "faild") != "success")
            {
                base.Message(result);
                return false;
            }
            return UserAgent.Instance().AddGameAccount(userId, this.Type, username, password);
        }



        /// <summary>
        /// 转入
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public override bool Deposit(int userId, decimal money, string id, out decimal amount)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("username", UserAgent.Instance().GetPlayerName(userId, this.Type));
            dic.Add("amount", this.MoneyFormat(money));
            dic.Add("externalTransactionId", id.PadLeft(13, '0'));

            string result = this.POST("credit/deposit", dic);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("ending_balance"))
            {
                base.Message(result);
                amount = decimal.MinusOne;
                return false;
            }

            amount = ht.GetValue("ending_balance", decimal.Zero);
            return true;
        }

        public override void FastLogin(int userId, string key)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("username", UserAgent.Instance().GetPlayerName(userId, this.Type));
            dic.Add("game_code", key);
            if (WebAgent.IsMobile()) dic.Add("mobile", "yes");
            string result = this.POST("game/open", dic);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("url"))
            {
                HttpContext.Current.Response.Write(result);
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head><title>正在进入{0}大厅</title></head><body>", this.Type.GetDescription())
                .AppendFormat("<script> location.href=\"{0}\"; </script>", ht["url"])
                .Append("</body></html>");
            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Write(sb);
        }

        public override decimal GetBalance(int userId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("username", UserAgent.Instance().GetPlayerName(userId, this.Type));

            string result = this.POST("player/balance", dic);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("balance"))
            {
                base.Message(result);
                return decimal.MinusOne;
            }

            return ht.GetValue("balance", decimal.Zero);
        }

        public override bool Withdraw(int userId, decimal money, string orderId, out decimal amount)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("username", UserAgent.Instance().GetPlayerName(userId, this.Type));
            dic.Add("amount", this.MoneyFormat(money));
            dic.Add("externalTransactionId", orderId.PadLeft(13, '0'));

            string result = this.POST("credit/withdrawal", dic);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("ending_balance"))
            {
                base.Message(result);
                amount = decimal.MinusOne;
                return false;
            }

            amount = ht.GetValue("ending_balance", decimal.Zero);
            return true;
        }


        /// <summary>
        /// 存取款对金额的格式要求
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        protected virtual string MoneyFormat(decimal money)
        {
            return money.ToString("0.00");
        }
    }
}
