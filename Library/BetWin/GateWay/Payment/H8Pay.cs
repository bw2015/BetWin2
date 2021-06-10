using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;

using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Json;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Model;
using BW.Agent;
using SP.Studio.Array;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 讯付通
    /// </summary>
    public class H8Pay : IPayment
    {
        public H8Pay() { }

        public H8Pay(string setting) : base(setting) { }

        /// <summary>
        /// 微信网关
        /// </summary>
        private const string WXGATEWAY = "http://wx.h8pay.com/api/pay.action";

        /// <summary>
        /// 支付宝网关
        /// </summary>
        private const string ALIGATEWAY = "http://zfb.h8pay.com/api/pay.action";

        [Description("商户号")]
        public string merNo { get; set; }

        [Description("网关 (支付宝(ZFB) 微信(WX)")]
        public string netway { get; set; }

        private string _callbackUrl = "/handler/payment/H8Pay";
        [Description("通知地址")]
        public string callBackUrl
        {
            get
            {
                return this._callbackUrl;
            }
            set
            {
                this._callbackUrl = value;
            }
        }

        private string _callBackViewUrl = "/handler/payment/H8Pay";
        [Description("回显地址")]
        public string callBackViewUrl
        {
            get
            {
                return this._callBackViewUrl;
            }
            set
            {
                this._callBackViewUrl = value;
            }
        }


        [Description("密钥")]
        public string Key { get; set; }

        public override bool IsWechat()
        {
            return this.netway == "WX";
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("merNo", this.merNo);
            dic.Add("netway", this.netway);
            dic.Add("random", WebAgent.GetRandom(1000, 9999).ToString());
            dic.Add("orderNum", this.OrderID);
            dic.Add("amount", ((int)(this.Money * 100)).ToString());
            dic.Add("goodsName", this.Name);
            dic.Add("callBackUrl", this.GetUrl(this.callBackUrl));
            dic.Add("callBackViewUrl", this.GetUrl(this.callBackViewUrl));

            string data = string.Concat("{", string.Format("{0}", string.Join(",", dic.OrderBy(t => t.Key).Select(t => string.Format("\"{0}\":\"{1}\"", t.Key, t.Value)))), "}") + this.Key;

            string sign = MD5.toMD5(data).ToUpper();
            dic.Add("sign", sign);
            data = string.Format("{{{0}}}", string.Join(",", dic.OrderBy(t => t.Key).Select(t => string.Format("\"{0}\":\"{1}\"", t.Key, t.Value))));

            string erroeMsg = null;
            string result = string.Empty;
            Hashtable ht = null;
            switch (this.netway)
            {
                case "WX":
                    result = NetAgent.UploadData(WXGATEWAY, string.Format("data={0}", HttpUtility.UrlEncode(data)), Encoding.UTF8);
                    ht = JsonAgent.GetJObject(result);
                    if (ht == null || !ht.ContainsKey("stateCode"))
                    {
                        erroeMsg = "提交错误，请重试";
                    }
                    else if (ht["stateCode"].ToString() != "00")
                    {
                        erroeMsg = ht["msg"].ToString();
                    }
                    else
                    {
                        if (this.IsWechat() && (WebAgent.IsWechat() || WebAgent.GetParam("wechat", 0) == 1))
                        {
                            HttpContext.Current.Response.Write(true, erroeMsg, new
                            {
                                data = Utils.GetQRCode(ht["qrcodeUrl"].ToString())
                            });
                        }
                        else
                        {
                            this.CreateWXCode(ht["qrcodeUrl"].ToString());
                        }
                    }
                    if (!string.IsNullOrEmpty(erroeMsg))
                    {
                        HttpContext.Current.Response.Write(false, erroeMsg, new
                        {
                            data = result
                        });

                    }
                    break;
                case "ZFB":
                    result = NetAgent.UploadData(ALIGATEWAY, string.Format("data={0}", HttpUtility.UrlEncode(data)), Encoding.UTF8);
                    ht = JsonAgent.GetJObject(result);
                    if (ht == null || !ht.ContainsKey("stateCode"))
                    {
                        HttpContext.Current.Response.Write("提交错误，请重试");
                    }
                    else if (ht["stateCode"].ToString() != "00")
                    {
                        HttpContext.Current.Response.Write(ht["msg"]);
                    }
                    else
                    {
                        this.CreateAliCode(ht["qrcodeUrl"].ToString());
                    }
                    break;
            }

            HttpContext.Current.Response.End();

        }

        public override bool Verify(VerifyCallBack callback)
        {
            //{"amount":"20000","goodsName":"在线充值","merNo":"Mer201702261541","netway":"WX","orderNum":"20170309211322365","payDate":"2017-03-09 21:13:35","payResult":"00","sign":"4E450C2B68ABD91193D34FC3E4545AA8"}
            string data = WebAgent.GetParam("data");
            IDictionary<string, string> dic = JsonAgent.GetDictionary<string, string>(data);

            if (dic == null || !dic.ContainsKey("payResult") || dic["payResult"] != "00") return false;
            string sign = dic["sign"];
            dic.Remove("sign");

            string dicdata = JsonAgent.GetJson(dic);

            if (sign == MD5.toMD5(dicdata + this.Key).ToUpper())
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            //{"amount":"100","goodsName":"在线充值","merNo":"Mer201702261541","netway":"WX","orderNum":"20170311023846631","payDate":"2017-03-11 02:39:23","payResult":"00","sign":"B91BD993387FDC7A07FD803A5EB51BB5"}
            string data = WebAgent.GetParam("data");
            Hashtable ht = JsonAgent.GetJObject(data);
            if (ht == null || !ht.ContainsKey("payResult") || ht["payResult"].ToString() != "00")
            {
                money = decimal.Zero;
                systemId = string.Empty;
                return null;
            }

            systemId = ht["sign"].ToString();
            string orderId = ht["orderNum"].ToString();
            money = UserAgent.Instance().GetRechargeOrderInfo(long.Parse(orderId)).Money;
            return orderId.ToString();
        }
    }
}
