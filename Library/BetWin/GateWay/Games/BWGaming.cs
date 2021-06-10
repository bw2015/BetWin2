using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using BW.Agent;

using BW.Common.Games;
using BW.Common.Users;

namespace BW.GateWay.Games
{
    /// <summary>
    /// 贝盈电竞
    /// </summary>
    public class BWGaming : IGame
    {
        public BWGaming() { }

        public BWGaming(string setting) : base(setting) { }

        /// <summary>
        /// API网关
        /// </summary>
        public string GATEWAY { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        public string SECRETKEY { get; set; }

        public override TransferStatus CheckTransfer(int userId, string id)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("ID", id);
            string msg;
            JObject info;
            bool result = this.POST("user", "transferinfo", data, out msg, out info);
            TransferStatus status = TransferStatus.None;
            if (result)
            {
                switch (info["Status"].Value<string>())
                {
                    case "None":
                        status = TransferStatus.Faild;
                        break;
                    case "Finish":
                        status = TransferStatus.Success;
                        break;
                }
            }
            return status;
        }

        /// <summary>
        /// 创建一个用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override bool CreateUser(int userId, params object[] args)
        {
            string playerName = this.GetPlayerName(userId);
            if (!string.IsNullOrEmpty(playerName)) return true;

            playerName = GameAgent.Instance().CreatePlayerName(userId);
            string password = Guid.NewGuid().ToString("N").Substring(0, 8);

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("UserName", playerName);
            data.Add("Password", password);
            string msg;
            JObject info;
            bool result = this.POST("user", "register", data, out msg, out info);
            if (!result)
            {
                base.Message(msg);
                return false;
            }
            return UserAgent.Instance().AddGameAccount(userId, this.Type, playerName, password);
        }

        public override bool Deposit(int userId, decimal money, string id, out decimal amount)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("UserName", this.GetPlayerName(userId));
            data.Add("Money", money.ToString());
            data.Add("Type", "IN");
            data.Add("ID", id);
            string msg;
            JObject info;
            bool result = this.POST("user", "transfer", data, out msg, out info);
            if (result)
            {
                amount = this.GetBalance(userId);
                return true;
            }
            else
            {
                amount = decimal.MinusOne;
                base.Message(msg);
                return false;
            }
        }

        public override void FastLogin(int userId, string key)
        {
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null) return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("UserName", user.PlayerName);
            string msg;
            JObject info;
            bool result = this.POST("user", "login", data, out msg, out info);
            StringBuilder sb = new StringBuilder();
            if (result)
            {
                string url = info["Url"].Value<string>();
                sb.AppendFormat("<script> location.href='{0}'; </script>",
                    url);
            }
            else
            {
                sb.Append(msg);
            }

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        public override decimal GetBalance(int userId)
        {
            if (string.IsNullOrEmpty(this.GATEWAY)) return decimal.MinusOne;
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null) return decimal.MinusOne;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("UserName", user.PlayerName);
            string msg;
            JObject info;
            bool result = this.POST("user", "balance", data, out msg, out info);
            if (result)
            {
                return info["Money"].Value<decimal>();
            }
            else
            {
                base.Message(msg);
                return decimal.MinusOne;
            }
        }

        public override bool Withdraw(int userId, decimal money, string orderId, out decimal amount)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("UserName", this.GetPlayerName(userId));
            data.Add("Money", money.ToString());
            data.Add("Type", "OUT");
            data.Add("ID", orderId);
            string msg;
            JObject info;
            bool result = this.POST("user", "transfer", data, out msg, out info);
            if (result)
            {
                amount = this.GetBalance(userId);
                return true;
            }
            else
            {
                amount = decimal.MinusOne;
                base.Message(msg);
                return false;
            }
        }

        /// <summary>
        /// 发送信息到API
        /// </summary>
        /// <param name="action">接口</param>
        /// <param name="method">动作</param>
        /// <param name="data">要发送的数据</param>
        /// <param name="msg">返回的信息</param>
        /// <param name="info">返回的数据</param>
        /// <returns></returns>
        private bool POST(string action, string method, Dictionary<string, string> data, out string msg, out JObject info)
        {
            if (string.IsNullOrEmpty(this.GATEWAY))
            {
                msg = string.Empty;
                info = null;
                return false;
            }
            string url = this.GATEWAY + action + "/" + method;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers.Add("Authorization", SECRETKEY);
                    wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                    string postData = string.Join("&", data.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
                    byte[] resultData = wc.UploadData(url, "POST", Encoding.UTF8.GetBytes(postData));
                    string result = Encoding.UTF8.GetString(resultData);
                    JObject obj = (JObject)JsonConvert.DeserializeObject(result);
                    if ((int)obj["success"] != 1)
                    {
                        msg = (string)obj["msg"];
                        info = null;
                        return false;
                    }
                    msg = (string)obj["msg"];
                    if (obj["info"].GetType() == typeof(JValue))
                    {
                        info = null;
                    }
                    else
                    {
                        info = (JObject)obj["info"];
                    }
                    return true;
                }
                catch (WebException ex)
                {
                    info = null;
                    msg = string.Format("Error:{0}", ex.Message);
                    if (ex.Response != null)
                    {
                        StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8);
                        if (reader != null)
                        {
                            msg += string.Format("\n<hr />\n{0}", reader.ReadToEnd());
                        }
                    }
                    return false;
                }
            }
        }
    }
}
