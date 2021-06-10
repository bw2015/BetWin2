using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.ComponentModel;

using SP.Studio.Model;
using SP.Studio.Web;
using BW.Common.Sites;
using BW.Common.Users;
using BW.Agent;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 新U支付
    /// </summary>
    public class UZhiFu : IPayment
    {
        public UZhiFu() : base() { }

        public UZhiFu(string setting) : base(setting) { }

        /// <summary>
        /// 1:支付宝 2：QQ钱包 3：微信
        /// </summary>
        [Description("支付方式")]
        public string payType { get; set; }

        [Description("appid")]
        public string appid { get; set; }

        [Description("收款邮箱")]
        public string selleremail { get; set; }

        [Description("收款手机")]
        public string mobile { get; set; }

        [Description("网关地址")]
        public string Gateway { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        [Description("金额类型")]
        public string MoneyValue
        {
            get
            {
                return this._moneyValues;
            }
            set
            {
                this._moneyValues = value;
            }
        }

        /// <summary>
        /// XML格式的二维码数据
        /// </summary>
        [Description("收款二维码")]
        public string Data { get; set; }

        public override bool IsWechat()
        {
            return false;
        }

        private static Dictionary<string, DateTime> lockData = new Dictionary<string, DateTime>();

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(this.Data))
            {
                dic.Add("total_fee", ((int)this.Money).ToString());
                dic.Add("pay", this.payType);
                dic.Add("out_trade_no", this.OrderID);
                dic.Add("appid", this.appid);
                dic.Add("seller_email", this.selleremail);
                dic.Add("mobile", this.mobile);
                this.BuildForm(dic, this.Gateway);
            }
            else
            {
                string type = string.Empty;
                switch (this.payType)
                {
                    case "1":
                        type = "alipay";
                        break;
                    case "2":
                        type = "wx";
                        break;
                    case "3":
                        type = "qq";
                        break;
                }
                if (string.IsNullOrEmpty(type))
                {
                    context.Response.Write(false, "未设定支付类型");
                }
                Dictionary<string, List<string>> data = this.getData();
                string key = type + (int)this.Money;
                if (!data.ContainsKey(key) || data[key].Count == 0)
                {
                    context.Response.Write(false, key + "不在支持范围");
                }
                string code = null;
                DateTime lastTime = DateTime.Now;
                foreach (string item in data[key])
                {
                    if (!lockData.ContainsKey(item))
                    {
                        code = item;
                        lockData.Add(item, DateTime.Now);
                        break;
                    }
                    if (lockData[item] < DateTime.Now.AddMinutes(-5))
                    {
                        code = item;
                        lockData[item] = DateTime.Now;
                        break;
                    }
                    if (lockData[item] < lastTime) lastTime = lockData[item];
                }
                if (string.IsNullOrEmpty(code))
                {
                    TimeSpan timeSpan = (TimeSpan)(lastTime.AddMinutes(5) - DateTime.Now);
                    context.Response.Write(false, string.Format("收单系统繁忙，请等待{0}分{1}秒再试", timeSpan.Minutes, timeSpan.Seconds));
                }

                int index = data[key].IndexOf(code);
                bool updateMoney = index == 0;
                if (!updateMoney)
                {
                    this.Money += (decimal)index / 100M;
                    updateMoney = UserAgent.Instance().UpdateRechargeMoney(long.Parse(this.OrderID), this.Money);
                }
                if (updateMoney)
                {
                    this.CreateQRCode(code, type);
                }
                else
                {
                    context.Response.Write(false, "订单金额发生错误，请重试");
                }
            }
        }

        /// <summary>
        /// 把二维码数据转换成为字典格式
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<string>> getData()
        {
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            Regex regex = new Regex(@"^(?<Type>(alipay|wx|qq)\d+):(?<Value>.+)");
            foreach (string item in this.Data.Split('|'))
            {
                if (!regex.IsMatch(item)) continue;
                Match match = regex.Match(item);
                string type = match.Groups["Type"].Value;
                string value = match.Groups["Value"].Value;
                if (!dic.ContainsKey(type)) dic.Add(type, new List<string>());
                foreach (string code in value.Split(',').Where(t => !string.IsNullOrEmpty(t)))
                {
                    if (!dic[type].Contains(code))
                        dic[type].Add(code);
                }
            }
            return dic;
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (this.Key != WebAgent.GetParam("key")) return false;
            if (this.appid != WebAgent.GetParam("appid")) return false;

            callback.Invoke();
            return true;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("PayJe", decimal.Zero);
            systemId = WebAgent.GetParam("ddh");
            if (string.IsNullOrEmpty(systemId)) systemId = WebAgent.GetParam("PayNO");
            string orderId = WebAgent.GetParam("PayMore");
            if (!string.IsNullOrEmpty(orderId)) return orderId;

            string type = WebAgent.GetParam("payfangshi");
            DateTime time = WebAgent.GetParam("PayTime", DateTime.Now);

            PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingList().Where(t => t.Type == PaymentType.UZhiFu && ((UZhiFu)t.PaymentObject).payType == type).FirstOrDefault();
            if (payment == null) return null;
            RechargeOrder order = UserAgent.Instance().GetRechargeOrderInfo(payment.ID, money, time.AddMinutes(-5), time);
            if (order == null) return null;
            return order.ID.ToString();
        }

        public override string ShowCallback()
        {
            return "Success";
        }
    }
}
