using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Linq;
using System.Web;
using System.IO;

using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Xml;
using SP.Studio.Net;
using SP.Studio.Model;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 威富通
    /// </summary>
    public class WFT : IPayment
    {
        public WFT() : base() { }

        public WFT(string setting) : base(setting) { }

        private string _gateway = "https://pay.swiftpass.cn/pay/gateway";
        [Description("支付网关")]
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

        /// <summary>
        /// 接口类型
        /// pay.alipay.native3
        /// </summary>
        private string _service = "pay.weixin.native";
        [Description("接口类型")]
        public string service
        {
            get
            {
                return this._service;
            }
            set
            {
                this._service = value;
            }
        }

        /// <summary>
        /// 版本号
        /// </summary>
        private string _version = "2.0";
        [Description("版本号")]
        public string version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }

        /// <summary>
        /// 字符集
        /// </summary>
        private const string charset = "UTF-8";

        /// <summary>
        /// 签名方式
        /// </summary>
        private const string sign_type = "MD5";

        [Description("商户号")]
        public string mch_id { get; set; }

        [Description("授权渠道编号")]
        public string sign_agentno { get; set; }

        private string _notify_url = "/handler/payment/WFT";
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

        [Description("密钥")]
        public string Key { get; set; }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("out_trade_no", this.OrderID);
            dic.Add("body", this.Name);
            dic.Add("attach", this.GetType().Name);
            dic.Add("total_fee", ((int)(this.Money * 100)).ToString());
            dic.Add("mch_create_ip", IPAgent.IP);
            dic.Add("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("time_expire", DateTime.Now.AddDays(1).ToString("yyyyMMddHHmmss"));
            dic.Add("service", service);
            dic.Add("mch_id", this.mch_id);
            dic.Add("version", version);
            dic.Add("notify_url", this.GetUrl(this.notify_url));
            dic.Add("nonce_str", Guid.NewGuid().ToString("N").ToLower().Substring(0, 32));
            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&key=" + this.Key;
            dic.Add("sign", MD5.toMD5(signStr).ToUpper());

            XElement root = new XElement("xml");
            dic.ToList().ForEach(t =>
            {
                XElement item = new XElement(t.Key);
                item.Value = t.Value;
                root.Add(item);
            });
            string result = NetAgent.UploadData(this.Gateway, root.ToString(), Encoding.UTF8);

            try
            {
                root = XElement.Parse(result);
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write(false, ex.Message);
            }
            string code = root.GetValue("code_img_url");
            if (string.IsNullOrEmpty(code))
            {
                HttpContext.Current.Response.Write(HttpUtility.HtmlEncode(result));
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

            money = root.GetValue("total_fee", decimal.Zero) / 100M;
            systemId = root.GetValue("transaction_id");
            return root.GetValue("out_trade_no");

        }
    }
}
