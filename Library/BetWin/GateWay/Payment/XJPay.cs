using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;
using System.Web;

using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Web;
using SP.Studio.Json;
using SP.Studio.Security;

namespace BW.GateWay.Payment
{
    public class XJPay : IPayment
    {
        public XJPay() : base() { }

        public XJPay(string setting) : base(setting) { }

        /// <summary>
        /// 渠道号（商户号）
        /// </summary>
        [Description("渠道号")]
        public string cid { get; set; }

        [Description("密钥")]
        public string key { get; set; }

        private string _gateway = "https://pay.jingmugukj.com/pay";
        [Description("网关")]
        public string gateway
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
        /// 支付宝扫码:CR_ALI    支付宝：ALI 微信扫码:CR     QQ钱包:TEN_CR QQ扫码:CR_QQ
        /// 微信WAP :CR_WAP 银联网关 :YL_G 银联快捷：YL_KJ
        /// </summary>
        [Description("类型")]
        public string platform { get; set; }

        [Description("备注信息")]
        public string Remark { get; set; }

        private string _cburl = "/handler/payment/SUCCESS";
        [Description("跳转地址")]
        public string cburl
        {
            get
            {
                return this._cburl;
            }
            set
            {
                this._cburl = value;
            }
        }

        private string _token_url = "/handler/payment/XJPay";
        [Description("回调地址")]
        public string token_url
        {
            get
            {
                return this._token_url;
            }
            set
            {
                this._token_url = value;
            }
        }

        protected override string GetMark()
        {
            return this.Remark;
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("cid", this.cid);
            dic.Add("total_fee", (this.Money * 100).ToString());
            dic.Add("title", this.Name);
            dic.Add("attach", this.Name);
            dic.Add("platform", this.platform);
            dic.Add("cburl", this.GetUrl(this.cburl));
            dic.Add("orderno", this.OrderID);

            //attach+cburl+cid+orderno+platform+title+token_url+total_fee+key
            string signStr = string.Join(string.Empty, "attach+cburl+cid+orderno+platform+title+token_url+total_fee+key".Split('+').Select(t => dic.ContainsKey(t) ? dic[t] : string.Empty)) + this.key;
            dic.Add("sign", MD5.toMD5(signStr));

            string data = dic.ToJson();

            string result = NetAgent.UploadData(this.gateway, data, Encoding.UTF8);
            int err = JsonAgent.GetValue<int>(result, "err");
            if (err != 200)
            {
                context.Response.Write(JsonAgent.GetValue<string>(result, "msg") ?? result);
                return;
            }
            string code = JsonAgent.GetValue<string>(result, "code_img_url");
            if (string.IsNullOrEmpty(code))
            {
                code = JsonAgent.GetValue<string>(result, "code_url");
            }
            if (string.IsNullOrEmpty(code))
            {
                context.Response.Write(result);
                return;
            }

            switch (this.platform)
            {
                case "CR":
                case "CR_WAP":
                    this.CreateWXCode(code);
                    break;
                case "CR_ALI":
                case "ALI":
                    this.CreateAliCode(code);
                    break;
                case "TEN_CR":
                case "CR_QQ":
                default:
                    this.BuildForm(code);
                    break;
            }
        }

        public override string ShowCallback()
        {
            return "success";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            byte[] data = WebAgent.GetInputSteam(HttpContext.Current);
            string result = Encoding.UTF8.GetString(data);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("errcode") || ht["errcode"].ToString() != "0")
            {
                money = decimal.Zero;
                systemId = string.Empty;
                return string.Empty;
            }

            systemId = ht["sign"].ToString();
            money = decimal.Parse(ht["total_fee"].ToString()) / 100M;

            return ht["orderno"].ToString();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            byte[] data = WebAgent.GetInputSteam(HttpContext.Current);
            string result = Encoding.UTF8.GetString(data);
            //attach+errcode+orderno+total_fee +key
            Hashtable ht = JsonAgent.GetJObject(result);
            //{"errcode":"0","orderno":"20170701031402634","total_fee":"500","attach":"","sign":"829B151352DFDAF43D6336C1526295E1"}
            if (ht == null || !ht.ContainsKey("errcode") || ht["errcode"].ToString() != "0")
            {
                return false;
            }

            string attach = ht["attach"].ToString();
            string errcode = ht["errcode"].ToString();
            string orderno = ht["orderno"].ToString();
            string total_fee = ht["total_fee"].ToString();
            string sign = ht["sign"].ToString().ToLower();

            string value = MD5.toMD5(attach + errcode + orderno + total_fee + this.key).ToLower();
            if (value == sign)
            {
                callback.Invoke();
                return true;
            }
            return false;
        }
    }
}
