using BW.Agent;
using BW.Common.Games;
using BW.Common.Users;
using SP.Studio.Model;
using SP.Studio.Net;
using SP.Studio.Security;
using SP.Studio.Web;
using SP.Studio.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace BW.GateWay.Games
{
    public class BBIN : IGame
    {
        public BBIN() : base() { }

        public BBIN(string setting) : base(setting) { }


        [Description("会员注册前缀")]
        public string PreName { get; set; }

        [Description("转账订单号前缀")]
        public string PreOrder { get; set; }

        /// <summary>
        /// Oriental
        /// </summary>
        [Description("网站名称")]
        public string website { get; set; }

        /// <summary>
        /// 代理号 Oriental
        /// </summary>
        [Description("代理号")]
        public string uppername { get; set; }

        /// <summary>
        /// 创建用户的密钥
        /// </summary>
        [Description("KEY:CreateMember")]
        public string CreateMember { get; set; }

        /// <summary>
        /// 登录密钥
        /// </summary>
        [Description("KEY:Login")]
        public string Login { get; set; }

        /// <summary>
        /// 转账的Key
        /// </summary>
        [Description("KEY:Transfer")]
        public string Transfer { get; set; }

        /// <summary>
        /// 检查转账记录的Key
        /// </summary>
        [Description("KEY:CheckTransfer")]
        public string CheckTransferKey { get; set; }

        /// <summary>
        /// 查询用户余额的KEY
        /// </summary>
        [Description("KEY:CheckUsrBalance")]
        public string CheckUsrBalance { get; set; }

        /// <summary>
        /// 查询日志的密钥
        /// </summary>
        [Description("KEY:BetRecord")]
        public string BetRecord { get; set; }

        /// <summary>
        /// 下注记录(注单变更时间)(不分体系、限定5分钟)(无法捞取7天前，被异动的资料)
        /// </summary>
        [Description("KEY:BetRecordByModifiedDate3")]
        public string BetRecordByModifiedDate3 { get; set; }

        /// <summary>
        /// api网关地址
        /// </summary>
        [Description("API网关")]
        public string GateWay { get; set; }

        /// <summary>
        /// 登录的网关地址 
        /// </summary>
        [Description("登录网关")]
        public string LoginGateway { get; set; }

        /// <summary>
        /// 美东时间的日期格式（GMT-4）
        /// </summary>
        private string Date
        {
            get
            {
                return DateTime.Now.AddHours(-12).ToString("yyyyMMdd");
            }
        }

        public override bool CreateUser(int userId, params object[] args)
        {
            string playerName = this.GetPlayerName(userId);
            if (!string.IsNullOrEmpty(playerName)) return true;
            string url = this.getUrl("CreateMember");

            playerName = string.Concat(this.PreName, userId.ToString().PadLeft(4, '0'), Guid.NewGuid().ToString("N").Substring(0, 4)).ToLower();
            string password = Guid.NewGuid().ToString("N").Substring(0, 8);

            string source = this.website + playerName + this.CreateMember + this.Date;
            string key = string.Format("{0}{1}{2}", this.random(7),
               MD5.toMD5(source), this.random(1)).ToLower();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("website", this.website);
            dic.Add("username", playerName);
            dic.Add("uppername", this.uppername);
            dic.Add("password", password);
            dic.Add("key", key);

            string result = this.POST(url, dic, userId);

            string msg;
            if (this.getInfo(result, 21100, out msg))
            {
                return UserAgent.Instance().AddGameAccount(userId, this.Type, playerName, password);
            }
            else
            {
                base.Message(msg);
                return false;
            }
        }

        public override decimal GetBalance(int userId)
        {
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName)) return decimal.Zero;

            string url = this.getUrl("CheckUsrBalance");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("website", this.website);
            dic.Add("username", playerName);
            dic.Add("uppername", this.uppername);
            dic.Add("key", string.Format("{0}{1}{2}", this.random(4),
                MD5.toMD5(this.website + playerName + this.CheckUsrBalance + DateTime.Now.AddHours(-12).ToString("yyyyMMdd")),
                this.random(7)));

            string result = this.POST(url, dic, userId);
            XElement root = XElement.Parse(result);

            try
            {
                return root.GetValue("Record/TotalBalance[0]", decimal.MinusOne);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n" + result);
            }
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
        /// 检查转账
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public override TransferStatus CheckTransfer(int userId, string id)
        {
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName)) return TransferStatus.None;

            string url = this.getUrl("CheckTransfer");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("website", this.website);
            dic.Add("transid", id);
            dic.Add("key", string.Format("{0}{1}{2}", 
                this.random(8), 
                MD5.toMD5(this.website + this.CheckTransferKey + DateTime.Now.AddHours(-12).ToString("yyyyMMdd")),
                this.random(8)));
            string result = this.POST(url, dic, userId);
            XElement root = XElement.Parse(result);

            TransferStatus status = TransferStatus.None;
            switch (root.GetValue("Record/Status", int.MinValue))
            {
                case 1:
                    status = TransferStatus.Success;
                    break;
                case -1:
                    status = TransferStatus.Faild;
                    break;
                case int.MinValue:
                    status = TransferStatus.None;
                    break;
                default:
                    status = TransferStatus.Other;
                    break;
            }
            return status;

        }

        public override void FastLogin(int userId, string key)
        {
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (string.IsNullOrEmpty(user.PlayerName))
            {
                HttpContext.Current.Response.Write(false, "尚未开户");
                return;
            }
            //http://888.bbinplayers.com/app/WebService/XML/display.php/Login
            //?website=cccasino&username=bw20150wnsrab&uppername=dcg01&password=4CD570CF&lang=zh-cn&page_site=live&page_present=live&key=131087731699135a6496b08a189adba7308924613108775


            //PreName=&PreOrder=&website=avia&uppername=dboqu&CreateMember=3QcgFxyY0&Login=fV98jAu&Transfer=10WyHdOdZ&CheckTransferKey=5Jr57Ya8c7&CheckUsrBalance=7pxyd9c0a&BetRecord=6kqBB1&BetRecordByModifiedDate3=6kqBB1&GateWay=http://linkapi.bw-gaming.com/app/WebService/XML/display.php/&LoginGateway=http://888.bw-gaming.com/app/WebService/XML/display.php/Login
            string url = this.LoginGateway;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("website", this.website);
            dic.Add("username", user.PlayerName);
            dic.Add("uppername", this.uppername);
            dic.Add("password", user.Password);
            dic.Add("lang", "zh-cn");
            dic.Add("page_site", key);
            if (key == "live") dic.Add("page_present", key);
            dic.Add("key", string.Format("{0}{1}{2}", this.random(8), MD5.toMD5(this.website + user.PlayerName + this.Login + Date), this.random(1)).ToLower());

            this.BuildForm(url, dic);
        }

        #region ==============  私有方法  ===============

        private string getUrl(string methodName)
        {
            return string.Format("{0}{1}", this.GateWay, methodName);
        }

        private string random(int length)
        {
            return Guid.NewGuid().ToString("N").ToLower().Substring(0, length);
        }

        private string POST(string url, Dictionary<string, string> dic, int userId)
        {
            if (dic.ContainsKey("key")) dic["key"] = dic["key"].ToLower();
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            url += "?" + data;
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            this.SaveLog(userId, result, "url", url);
            return result;
        }

        /// <summary>
        /// 获取返回信息判断是否正确
        /// </summary>
        /// <param name="result"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool getInfo(string result, int successCode, out string msg)
        {
            msg = string.Empty;
            try
            {
                XElement root = XElement.Parse(result);
                msg = root.GetValue("Record/Message[0]");
                return root.GetValue("Record/Code[0]", 0) == successCode;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 转账
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="money">转账金额</param>
        /// <param name="orderId">自定义的订单编号</param>
        /// <param name="action">方向 充值为IN 提款为OUT</param>
        /// <returns></returns>
        private bool transfer(int userId, decimal money, string orderId, string action, out decimal amount)
        {
            amount = decimal.MinusOne;
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName))
            {
                base.Message("暂未开户");
                return false;
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("website", this.website);
            dic.Add("username", playerName);
            dic.Add("uppername", this.uppername);
            dic.Add("remitno", orderId);
            dic.Add("action", action);
            dic.Add("remit", ((int)money).ToString());
            dic.Add("key", string.Format("{0}{1}{2}", this.random(9), MD5.toMD5(this.website + playerName + orderId + this.Transfer + this.Date), this.random(4)));

            string url = this.getUrl("Transfer");

            string result = this.POST(url, dic, userId);
            string msg;
            if (!this.getInfo(result, 11100, out msg))
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
