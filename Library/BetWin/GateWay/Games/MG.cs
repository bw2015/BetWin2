using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;

using BW.Agent;
using BW.Common.Systems;
using SP.Studio.Net;
using SP.Studio.Json;
using SP.Studio.Core;
using SP.Studio.Array;


namespace BW.GateWay.Games
{
    public class MG : IGame
    {
        public MG() : base() { }

        public MG(string setting) : base(setting) { }

        #region ========= 静态缓存 ==========

        /// <summary>
        /// token的超时时间（分钟）
        /// </summary>
        private const int EXPRESS_TIME = 30;

        /// <summary>
        /// MG的token key
        /// </summary>
        private const string TOKEN_KEY = "MG_TOKEN";

        #endregion

        private string _api = "https://api.adminserv88.com";
        /// <summary>
        /// API域名
        /// </summary>
        public string API
        {
            get
            {
                return this._api;
            }
            set
            {
                this._api = value;
            }
        }

        /// <summary>
        /// Auth用户名
        /// </summary>
        public string AuthUser { get; set; }

        /// <summary>
        /// Auth密码
        /// </summary>
        public string AuthPass { get; set; }

        /// <summary>
        /// API用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// API密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 上级ID
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// 获取token
        /// </summary>
        /// <returns></returns>
        private string getToken()
        {
            SystemKeyValue keyValue = SystemAgent.Instance().GetSystemValue(TOKEN_KEY);
            if (keyValue != null && keyValue.CreateAt.AddMinutes(EXPRESS_TIME) > DateTime.Now) return keyValue.Value;

            string url = this.API + "/oauth/token";
            string hashPincode = string.Format("{0}:{1}", this.AuthUser, this.AuthPass);
            byte[] bytes = Encoding.UTF8.GetBytes(hashPincode);
            string authValue = Convert.ToBase64String(bytes);
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", "Basic " + authValue);
            header.Add("X-DAS-TZ", "UTC+8");
            header.Add("X-DAS-CURRENCY", "CNY");
            header.Add("X-DAS-LANG", "zh-CN");
            header.Add("X-DAS-TX-ID", Guid.NewGuid().ToString("N"));
            header.Add("Content-Type", "application/x-www-form-urlencoded;charset=utf-8");
            string data = string.Format("grant_type=password&username={0}&password={1}", this.UserName, Uri.EscapeDataString(this.Password));
            string result;
            using (WebClient wc = new WebClient())
            {
                foreach (KeyValuePair<string, string> item in header)
                {
                    wc.Headers.Add(item.Key, item.Value);
                }
                result = Encoding.UTF8.GetString(wc.UploadData(url, "POST", Encoding.UTF8.GetBytes(data)));
            }
            //string result = NetAgent.UploadData(url, Encoding.UTF8, header, data);

            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("expires_in") || !ht.ContainsKey("access_token"))
            {
                base.Message(result);
                return null;
            }
            string token = ht["access_token"].ToString();
            SystemAgent.Instance().SaveSystemKeyValue(TOKEN_KEY, token);
            return token;
        }

        /// <summary>
        /// 创建请求头
        /// </summary>
        /// <param name="txId"></param>
        /// <returns></returns>
        private Dictionary<string, string> createHeader(out string txId)
        {
            txId = Guid.NewGuid().ToString("N");
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", "Basic" + " " + this.getToken());
            header.Add("X-DAS-TZ", "UTC+8");
            header.Add("X-DAS-CURRENCY", "CNY");
            header.Add("X-DAS-LANG", "zh-CN");
            header.Add("X-DAS-TX-ID", txId);
            return header;
        }

        /// <summary>
        /// 通用的post方法
        /// </summary>
        /// <param name="method">要提交到的地址</param>
        /// <param name="data">要发送的数据</param>
        /// <returns></returns>
        private string POST(string method, Dictionary<string, string> data)
        {
            string json = data == null ? string.Empty : JsonAgent.GetJson(data);
            return this.POST(method, json);
        }

        private string POST(string method, string data)
        {
            string token = this.getToken();
            if (string.IsNullOrEmpty(token)) return null;
            string txId = Guid.NewGuid().ToString("N");
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", "Bearer" + " " + token);
            header.Add("X-DAS-TZ", "UTC+8");
            header.Add("X-DAS-CURRENCY", "CNY");
            header.Add("X-DAS-LANG", "zh-CN");
            header.Add("X-DAS-TX-ID", txId);
            header.Add("Content-Type", "application/json;charset=UTF-8");
            string url = this.API + method;

            if (string.IsNullOrEmpty(data))
            {
                return NetAgent.GetWebContent(url, Encoding.UTF8, header);
            }
            return NetAgent.UploadData(url, Encoding.UTF8, header, data);
        }

        private Hashtable getData(string result)
        {
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null) { base.Message(result); return null; }
            if (!ht.ContainsKey("data"))
            {
                if (ht.Contains("error"))
                {
                    ht = JsonAgent.GetJObject(ht["error"].ToString());
                    if (ht == null || !ht.ContainsKey("message")) { base.Message(result); return null; }
                    base.Message(ht["message"].ToString());
                    return null;
                }
                base.Message(result);
                return null;
            }
            string data = ht["data"].ToString();
            if (data.StartsWith("["))
            {
                Hashtable[] list = JsonAgent.GetJList(data);
                if (list.Length == 0) return new Hashtable();
                return list.FirstOrDefault();
            }
            return JsonAgent.GetJObject(data);
        }

        public override TransferStatus CheckTransfer(int userId, string id)
        {
            string method = string.Format("/v1/transaction?ext_ref={0}&account_id={1}",
                id, UserAgent.Instance().GetPlayerName(userId, this.Type));
            string result = this.POST(method, string.Empty);
            Hashtable ht = this.getData(result);
            if (ht == null) return TransferStatus.None;
            if (ht.Count == 0) return TransferStatus.Faild;
            if (ht.ContainsKey("id") && ht["id"].ToString() == id) return TransferStatus.Success;
            return TransferStatus.Other;
        }

        public override bool CreateUser(int userId, params object[] args)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string username = GameAgent.Instance().CreatePlayerName(userId);
            string password = Guid.NewGuid().ToString("N").Substring(0, 10);

            dic.Add("parent_id", this.ParentId);
            dic.Add("username", username);
            dic.Add("password", password);
            dic.Add("ext_ref", username);
            string result = this.POST("/v1/account/member", dic);
            if (string.IsNullOrEmpty(result))
            {
                return false;
            }
            Hashtable ht = this.getData(result);
            if (ht == null || !ht.ContainsKey("id"))
            {
                base.Message(result);
                return false;
            }
            username = ht["id"].ToString();
            return UserAgent.Instance().AddGameAccount(userId, this.Type, username, dic["password"]);
        }

        public override bool Deposit(int userId, decimal money, string orderId, out decimal amount)
        {
            return this.Transfer("CREDIT", userId, money, orderId, out amount);
        }

        public override bool Withdraw(int userId, decimal money, string orderId, out decimal amount)
        {
            return this.Transfer("DEBIT", userId, money, orderId, out amount);
        }

        public override void FastLogin(int userId, string key)
        {
            Regex regex = new Regex(@"(?<AppID>\d+)\-(?<ItemID>\d+)$");
            if (!regex.IsMatch(key))
            {
                this.context.Response.Write("错误的游戏编号：" + key);
                return;
            }
            string itemId = regex.Match(key).Groups["ItemID"].Value;
            string appId = regex.Match(key).Groups["AppID"].Value;

            string method = "/v1/launcher/item";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("account_id", UserAgent.Instance().GetPlayerName(userId, this.Type));
            dic.Add("item_id", itemId);
            dic.Add("app_id", appId);

            string result = this.POST(method, dic);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("data"))
            {
                this.context.Response.Write(result);
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head><title>正在进入{0}大厅</title></head><body>", this.Type.GetDescription())
                .AppendFormat("<script> location.href=\"{0}\"; </script>", ht["data"])
                .Append("</body></html>");
            this.context.Response.ContentType = "text/html";
            this.context.Response.Write(sb);
        }

        public override decimal GetBalance(int userId)
        {
            string method = string.Format("/v1/wallet?account_id={0}", UserAgent.Instance().GetPlayerName(userId, this.Type));
            string result = this.POST(method, string.Empty);
            Hashtable ht = this.getData(result);
            if (ht == null || !ht.ContainsKey("credit_balance"))
            {
                base.Message(result);
                return decimal.MinusOne;
            }
            return ht.GetValue("credit_balance", decimal.Zero);
        }

        /// <summary>
        /// 转账
        /// </summary>
        /// <param name="action">转入：CREDIT 转出：DEBIT</param>
        /// <returns></returns>
        private bool Transfer(string type, int userId, decimal money, string orderid, out decimal amount)
        {
            amount = decimal.Zero;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("account_id", UserAgent.Instance().GetPlayerName(userId, this.Type));
            dic.Add("external_ref", orderid);
            dic.Add("amount", money.ToString("0.00"));
            dic.Add("type", type);
            dic.Add("balance_type", "CREDIT_BALANCE");
            dic.Add("category", "TRANSFER");

            string data = string.Format("[{0}]", JsonAgent.GetJson(dic));
            string result = this.POST("/v1/transaction", data);
            Hashtable ht = this.getData(result);
            if (ht == null || !ht.ContainsKey("balance"))
            {
                return false;
            }
            amount = ht.GetValue("balance", decimal.MinusOne);
            return true;
        }


        /// <summary>
        /// 获取游戏记录
        /// </summary>
        /// <returns></returns>
        public string GetLog(DateTime startAt, DateTime endAt)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("company_id", this.ParentId);
            dic.Add("start_time", startAt.ToString("yyyy-MM-ddTHH:mm:ss"));
            dic.Add("end_time", endAt.ToString("yyyy-MM-ddTHH:mm:ss"));
            dic.Add("include_transfers", "false");
            dic.Add("include_end_round", "false");
            dic.Add("page_size", "1024");

            string method = string.Format("/v1/feed/transaction?{0}", string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))));
            string result = this.POST(method, string.Empty);
            return result;
        }


    }
}
