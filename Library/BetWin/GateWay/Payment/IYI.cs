using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;
using System.Xml.Linq;

using SP.Studio.Net;
using SP.Studio.Model;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Xml;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 爱益支付
    /// </summary>
    public class IYI : IPayment
    {
        public IYI() : base() { }

        public IYI(string setting) : base(setting) { }

        private const string version = "1.0";

        private const string charset = "UTF-8";

        private const string sign_type = "MD5";

        [Description("接口类型")]
        public string service { get; set; }

        [Description("商户号")]
        public string mch_id { get; set; }

        private string _notify_url = "/handler/payment/IYI";
        [Description("通知地址")]
        public string notify_url
        {
            get
            {
                return this._notify_url;
            }
            set
            {
                this._notify_url = value;
            }
        }

        private string _callback_url = "/handler/payment/IYI";
        [Description("前台地址")]
        public string callback_url
        {
            get
            {
                return this._callback_url;
            }
            set
            {
                this._callback_url = value;
            }
        }

        [Description("密钥")]
        public string Key { get; set; }

        private string _gateway = "https://vip.iyibank.com/pay/gateway";
        [Description("网关地址")]
        public string Gateway
        {
            get
            {
                return this._gateway;
            }
            set
            {
                this._gateway = value;
            }
        }

        [Description("备注提醒")]
        public string Remark { get; set; }

        public override bool IsWechat()
        {
            return WebAgent.GetParam("wechat", 0) == 1;
        }
        public override void GoGateway()
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("service", this.service);
            dic.Add("version", version);
            dic.Add("charset", charset);
            dic.Add("sign_type", sign_type);
            dic.Add("mch_id", this.mch_id);
            dic.Add("out_trade_no", this.OrderID);
            dic.Add("body", this.Name);
            dic.Add("attach", this.GetType().Name);
            dic.Add("total_fee", this.Money.ToString("0.00"));
            dic.Add("mch_create_ip", IPAgent.IP);
            dic.Add("notify_url", this.GetUrl(this.notify_url));
            dic.Add("callback_url", this.GetUrl(this.callback_url));
            dic.Add("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("time_expire", DateTime.Now.AddDays(1).ToString("yyyyMMddHHmmss"));
            dic.Add("nonce_str", Guid.NewGuid().ToString("N").ToLower().Substring(0, 32));
            string signStr = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&key=" + this.Key;
            dic.Add("sign", MD5.Encryp(signStr).ToUpper());


            StringBuilder sb = new StringBuilder();
            sb.Append("<xml>");
            dic.ToList().ForEach(t =>
            {
                sb.AppendFormat("<{0}><![CDATA[{1}]]></{0}>", t.Key, t.Value);
            });
            sb.Append("</xml>");
            string result = NetAgent.UploadData(this.Gateway, sb.ToString(), Encoding.UTF8);
            XElement root = null;
            try
            {
                root = XElement.Parse(result);
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write(false, ex.Message);
                return;
            }

            string code = root.GetValue("token_id");
            if (string.IsNullOrEmpty(code))
            {
                if (IsWechat())
                {
                    HttpContext.Current.Response.Write(false, "二维码生成失败", new
                    {
                        data = result
                    });
                }
                HttpContext.Current.Response.Write(HttpUtility.HtmlEncode(result));
            }
            else
            {
                if (this.IsWechat())
                {
                    HttpContext.Current.Response.Write(true, "二维码生成成功", new
                    {
                        data = code,
                        remark = this.Remark
                    });
                }
                else
                {
                    switch (this.service)
                    {
                        case "pay.alipay.nativev3":
                        case "pay.alipay.micropayv3":
                        case "cibalipay":
                            this.CreateAliCode(code);
                            break;
                        case "pay.weixin.native":
                        case "pay.weixin.micropay":
                        case "pay.weixin.jspay":
                        case "cibweixin":
                            this.CreateWXCode(code);
                            break;
                        default:
                            HttpContext.Current.Response.Write(this.service);
                            break;
                    }
                }
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string data = Encoding.UTF8.GetString(WebAgent.GetInputSteam(HttpContext.Current));
            XElement root = XElement.Parse(data);

            string signStr = string.Join("&", root.Elements().OrderBy(t => t.Name.ToString()).Where(t => t.Name != "sign" && t.Name != "key" && !string.IsNullOrEmpty(t.Value)).Select(t => string.Format("{0}={1}", t.Name, t.Value)));
            signStr += "&key=" + this.Key;

            if (MD5.toMD5(signStr).ToUpper() == root.GetValue("sign"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            string data = Encoding.UTF8.GetString(WebAgent.GetInputSteam(HttpContext.Current));

            XElement root = XElement.Parse(data);

            money = root.GetValue("total_fee", decimal.Zero);
            systemId = root.GetValue("orderid");
            return root.GetValue("out_trade_no");
        }
    }
}
