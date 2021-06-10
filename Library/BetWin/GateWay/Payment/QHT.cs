using SP.Studio.Array;
using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 启航通
    /// </summary>
    public class QHT : IPayment
    {

        public QHT(string settingString) : base(settingString)
        {
        }

        [Description("网关")]
        public string Gateway { get; set; } = "http://www.wbh7.com/pay_index.html";

        [Description("商户号")]
        public string pay_memberid { get; set; }

        [Description("通道编码")]
        public string pay_bankcode { get; set; }

        [Description("通知地址")]
        public string pay_notifyurl { get; set; } = "/handler/payment/QHT";


        [Description("跳转地址")]
        public string pay_callbackurl { get; set; } = "/handler/payment/QHT";

        [Description("密钥")]
        public string KEY { get; set; }

        public override string ShowCallback()
        {
            return "OK";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("amount", decimal.Zero);
            systemId = WebAgent.GetParam("transaction_id");
            return WebAgent.GetParam("orderid");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("pay_memberid", this.pay_memberid);
            dic.Add("pay_orderid", this.OrderID);
            dic.Add("pay_applydate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dic.Add("pay_bankcode", this.pay_bankcode);
            dic.Add("pay_notifyurl", this.GetUrl(this.pay_notifyurl));
            dic.Add("pay_callbackurl", this.GetUrl(this.pay_callbackurl));
            dic.Add("pay_amount", this.Money.ToString("0.00"));
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + $"&key={this.KEY}";
            dic.Add("pay_md5sign", MD5.toMD5(signStr));
            dic.Add("pay_productname", "PAYMENT");
            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            //memberid : 10395      orderid : 20190102201546896      transaction_id : 2019010220154615464313468114      amount : 10.0000      datetime : 2019-01-02 20:16:21      returncode : 00      sign : 748CB5D13D1E8F42371DE40F98CAA440      attach :  
            //memberid : 10395      orderid : 20181228225337442      transaction_id : 2018122822533715460088179345      amount : 30.0000      datetime : 2018-12-28 23:07:47      returncode : 00      sign : D0E34A620C032E59C19F0B3B66AC1D95 

            if (WebAgent.GetParam("returncode") != "00") return false;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in this.context.Request.Form.AllKeys)
            {
                string value = WebAgent.QF(key);
                if (!string.IsNullOrEmpty(value)) dic.Add(key, WebAgent.QF(key));
            }
            string sign = dic.Get("sign", string.Empty);
            if (dic.ContainsKey("sign")) dic.Remove("sign");
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + $"&key={this.KEY}";
            if (sign.Equals(MD5.toMD5(signStr), StringComparison.CurrentCultureIgnoreCase))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }
    }
}
