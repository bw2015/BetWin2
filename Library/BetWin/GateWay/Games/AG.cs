using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using System.Web;

using BW.Agent;
using BW.Common.Games;
using BW.Common.Users;

using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Xml;
using SP.Studio.Web;
using SP.Studio.Model;

namespace BW.GateWay.Games
{
    public class AG : IGame
    {
        public AG() : base() { }

        public AG(string setting) : base(setting) { }


        [Description("代理编码")]
        public string cagent { get; set; }


        [Description("MD5密钥")]
        public string MD5Key { get; set; }


        [Description("DES密钥")]
        public string DESKey { get; set; }

        /// <summary>
        /// 远程网关
        /// </summary>
        [Description("AG网关")]
        public string Server { get; set; }

        /// <summary>
        /// 第二网关
        /// </summary>
        [Description("AG网关2")]
        public string Server2 { get; set; }

        /// <summary>
        /// 授权域名
        /// </summary>
        [Description("授权域名")]
        public string Domain { get; set; }

        /// <summary>
        /// 日志服务器
        /// </summary>
        [Description("日志服务器")]
        public string LogServer { get; set; }

        [Description("日志用户名")]
        public string LogUser { get; set; }

        /// <summary>
        /// 日志密码
        /// </summary>
        [Description("日志密码")]
        public string LogPass { get; set; }

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
            string password = Guid.NewGuid().ToString("N").Substring(0, 10);

            List<string> list = new List<string>();
            list.Add("cagent=" + this.cagent);
            list.Add("loginname=" + playerName);
            list.Add("method=lg");
            list.Add("actype=1");
            list.Add("password=" + password);
            list.Add("oddtype=A");
            list.Add("cur=CNY");

            string des = string.Join(@"/\\\\/", list);

            string param = DES.Encode(des, this.DESKey);
            string key = MD5.Encryp(param + MD5Key).ToLower();
            string url = string.Format("{0}doBusiness.do?params={1}&key={2}", this.Server, param, key);

            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result, "URL", url);

            string msg;
            int code = this.getInfo(result, out msg);

            if (code == 0)
            {
                return UserAgent.Instance().AddGameAccount(userId, this.Type, playerName, password);
            }
            else
            {
                this.Message(msg);
                return false;
            }

        }

        public override decimal GetBalance(int userId)
        {
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null) return decimal.Zero;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("cagent", this.cagent);
            dic.Add("loginname", user.PlayerName);
            dic.Add("method", "gb");
            dic.Add("actype", "1");
            dic.Add("password", user.Password);
            dic.Add("cur", "CNY");

            string url = this.getUrl("doBusiness", dic);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result, "URL", url);

            string msg;
            int success = this.getInfo(result, out msg);
            if (success == -1) return decimal.Zero;

            return (decimal)success;
        }

        public override bool Deposit(int userId, decimal money, string id, out decimal amount)
        {
            return this.transfer(userId, money, id, "IN", out amount);
        }

        public override bool Withdraw(int userId, decimal money, string orderId, out decimal amount)
        {
            return this.transfer(userId, money, orderId, "OUT", out amount);
        }

        /// <summary>
        /// 检查是否转账成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public override TransferStatus CheckTransfer(int userId, string id)
        {
            TransferStatus status = TransferStatus.None;
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null) return status;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("cagent", this.cagent);
            dic.Add("billno", id);
            dic.Add("method", "qos");
            dic.Add("actype", "1");
            dic.Add("cur", "CNY");

            string url = this.getUrl("doBusiness", dic);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result, "URL", url);
            string msg;
            int code = this.getInfo(result, out msg);
            switch (code)
            {
                case 0:
                    status = TransferStatus.Success;
                    break;
                case 1:
                case 2:
                    status = TransferStatus.Faild;
                    break;
                case -1:
                    status = TransferStatus.Other;
                    break;
            }
            return status;
        }

        public override void FastLogin(int userId, string key)
        {
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null)
            {
                HttpContext.Current.Response.Write(false, UserAgent.Instance().Message());
            }

            if (string.IsNullOrEmpty(key)) key = "0";

            List<string> list = new List<string>();
            list.Add("cagent=" + this.cagent);
            list.Add("loginname=" + user.PlayerName);
            list.Add("actype=1");
            list.Add("password=" + user.Password);
            list.Add("dm=" + this.Domain);
            list.Add("sid=" + this.cagent + DateTime.Now.ToString("yyMMddHHmmss") + WebAgent.GetRandom(10, 9999));
            list.Add("lang=1");
            list.Add("gameType=" + key);
            list.Add("oddtype=A");
            list.Add("cur=CNY");

            string url = this.getUrl("forwardGame", list);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<form action=\"{0}\" method=\"post\" id=\"{1}\"></form><script> if(document.getElementById('{1}') != null) document.getElementById('{1}').submit(); </script>",
                url, this.Type);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        #region ===========  私有方法 =============

        /// <summary>
        /// 获取返回信息
        /// </summary>
        /// <param name="result"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private int getInfo(string result, out string msg)
        {
            //刷新AG余额，返回值：<?xml version="1.0" encoding="utf-8"?><result info="900.00" msg=""/>,-1
            if (!result.StartsWith("<"))
            {
                msg = result;
                return -2;
            }
            try
            {
                XElement root = XElement.Parse(result);
                msg = root.GetAttributeValue("msg");
                return (int)root.GetAttributeValue("info", -1M);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return -1;
            }
        }

        private string getUrl(string action, List<string> list)
        {
            string des = string.Join(@"/\\\\/", list);

            string param = DES.Encode(des, this.DESKey);
            string key = MD5.Encryp(param + MD5Key).ToLower();

            string server = this.Server;
            switch (action)
            {
                case "forwardGame":
                    server = this.Server2;
                    break;
            }

            return string.Format("{0}{1}.do?params={2}&key={3}", server, action, param, key);
        }

        private string getUrl(string action, Dictionary<string, string> dic)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> keyValue in dic)
            {
                list.Add(string.Format("{0}={1}", keyValue.Key, keyValue.Value));
            }
            return this.getUrl(action, list);
        }

        /// <summary>
        /// 转账（转入或者转出）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="id"></param>
        /// <param name="type">IN: 从网站账号转款到游戏账号;    OUT: 從遊戲账號转款到網站賬號</param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private bool transfer(int userId, decimal money, string id, string type, out decimal amount)
        {
            amount = 0.00M;
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null)
            {
                this.Message("用户未注册");
                return false;
            }

            string billno = id;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("cagent", this.cagent);
            dic.Add("loginname", user.PlayerName);
            dic.Add("method", "tc");
            dic.Add("billno", billno);
            dic.Add("type", type);
            dic.Add("credit", type == "OUT" ? ((int)money).ToString() : money.ToString("0.00"));
            dic.Add("actype", "1");
            dic.Add("password", user.Password);
            dic.Add("cur", "CNY");
            string url = this.getUrl("doBusiness", dic);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result, dic, "预备转账", billno, "URL", url);

            string msg;
            int success = this.getInfo(result, out msg);
            if (success < 0)
            {
                UserAgent.Instance().Message(msg);
                amount = decimal.MinusOne;
                return false;
            }
            if (success != 0)
            {
                UserAgent.Instance().Message(msg);
                return false;
            }

            dic["method"] = "tcc";
            dic.Add("flag", "1");
            url = this.getUrl("doBusiness", dic);
            result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result, dic, "确认转账", billno, "URL", url);

            success = this.getInfo(result, out msg);
            if (success != 0 && !msg.Contains("error:10008"))
            {
                this.Message(msg);
                return false;
            }

            amount = this.GetBalance(userId);
            return true;
        }

        #endregion
    }
}
