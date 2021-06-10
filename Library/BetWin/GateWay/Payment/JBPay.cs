using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Array;
using SP.Studio.Security;
using SP.Studio.Web;
using BW.Common.Sites;
using SP.Studio.Net;
using SP.Studio.Json;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 金贝支付 1009 泛亚电竞使用
    /// 支付密钥：310b96d37a72607315d8585e6af6b75b
    /// 代付密钥：9a75e4fcbd48a61c3d06ff7c8d934283
    /// 支付密钥自加密结果：f255387cd9fef687af9b68d2e3294823
    /// 代付密钥自加密结果：16cf052d19ac12044313a8e95ac41e73
    /// </summary>
    public class JBPay : IPayment
    {
        public JBPay() : base() { }

        public JBPay(string setting) : base(setting) { }

        [Description("商户号")]
        public string amchid { get; set; }

        private string _enotifyurl = "/handler/payment/JBPay";
        [Description("异步通知")]
        public string enotifyurl
        {
            get
            {
                return this._enotifyurl;
            }
            set
            {
                this._enotifyurl = value;
            }
        }


        private string _freturl = "/handler/payment/JBPay";
        [Description("同步通知")]
        public string freturl
        {
            get
            {
                return this._freturl;
            }
            set
            {
                this._freturl = value;
            }
        }

        /// <summary>
        /// 1 网关PC，2 网关快捷，3 网关手机快捷，4 微信扫码，5 微信公众号，6 微信H5，
        /// 7 微信WAP，8 支付宝扫码，9 支付宝WAP，10 支付宝H5，11QQ 扫码，12QQH5，
        /// 13 京东支付，14 百度支付，15 银联扫码，16 扫码聚合，17 手机聚合、18 其他
        /// </summary>
        [Description("支付类型")]
        public string gpaytype { get; set; }

        private string _gateway = "https://${amchid}.jbpay.org/pay/";
        /// <summary>
        /// 网关地址
        /// </summary>
        [Description("网关")]
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

        [Description("密钥")]
        public string Key { get; set; }

        public override string ShowCallback()
        {
            return "ok";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("aorder");
            money = WebAgent.GetParam("bmoney", decimal.Zero);
            return systemId;
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("amchid", this.amchid);
            dic.Add("border", this.OrderID);
            dic.Add("cpacc", this.Name);
            dic.Add("dmoney", this.Money.ToString("0.00"));
            dic.Add("enotifyurl", this.GetUrl(this.enotifyurl, "http"));
            dic.Add("freturl", this.GetUrl(this.freturl));
            dic.Add("gpaytype", this.gpaytype);
            dic.Add("hbcode", this.BankValue);
            dic.Add("iclientip", IPAgent.IP);
            ///请求明文串
            ///amchid=10000000&border=AA100000&cpacc=打火机&dmoney=100.01&enotifyurl=http://www.xxx.com/notify/&
            ///freturl=http://www.xxx.com/returl/&gpaytype=6&hbcode=&iclientip=121.18.8.118商户密钥
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.Key;
            dic.Add("sign", MD5.Encryp(signStr, "GBK"));

            string url = this.Gateway.Replace("${amchid}", this.amchid) + "?" + dic.ToQueryString();

            string result = this.GetGatewayResult(url);

            //{"order":"20180509133556830","data":"DQoNCg0KDQo8IURPQ1RZUEUgaHRtbCBQVUJMSUMgIi0vL1czQy8vRFREIFhIVE1MIDEuMCBUcmFuc2l0aW9uYWwvL0VOIiAiaHR0cDovL3d3dy53My5vcmcvVFIveGh0bWwxL0RURC94aHRtbDEtdHJhbnNpdGlvbmFsLmR0ZCI+DQo8aHRtbCB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94aHRtbCI+DQoJPGhlYWQ+DQoJCTxtZXRhIGh0dHAtZXF1aXY9IkNvbnRlbnQtVHlwZSIgY29udGVudD0idGV4dC9odG1sOyBjaGFyc2V0PVVURi04Ii8+DQogIAk8L2hlYWQ+DQoJPGJvZHk+DQoJCTxzY3JpcHQgbGFuZ3VhZ2U9ImphdmFzY3JpcHQiPndpbmRvdy5vbmxvYWQ9ZnVuY3Rpb24oKXtkb2N1bWVudC5wYXlfZm9ybS5zdWJtaXQoKTt9PC9zY3JpcHQ+Cjxmb3JtIGlkPSJwYXlfZm9ybSIgbmFtZT0icGF5X2Zvcm0iIGFjdGlvbj0iaHR0cDovL2FwaS5sdWNreXBheWluZy5jb20vdjEvZ2F0ZXdheS5kbyIgbWV0aG9kPSJwb3N0Ij4KPGlucHV0IHR5cGU9ImhpZGRlbiIgbmFtZT0iZ29vZHNObyIgaWQ9Imdvb2RzTm8iIHZhbHVlPSI5OTQwODg1NjQyNjAxNDMxMDQxIj4KPGlucHV0IHR5cGU9ImhpZGRlbiIgbmFtZT0iYmFua0NvZGUiIGlkPSJiYW5rQ29kZSIgdmFsdWU9IklDQkMiPgo8aW5wdXQgdHlwZT0iaGlkZGVuIiBuYW1lPSJvcmRlckFtb3VudCIgaWQ9Im9yZGVyQW1vdW50IiB2YWx1ZT0iMTAwMDAiPgo8aW5wdXQgdHlwZT0iaGlkZGVuIiBuYW1lPSJyZXF1ZXN0T3JkZXIiIGlkPSJyZXF1ZXN0T3JkZXIiIHZhbHVlPSI5OTQwODg1NjQyNjAxNDMxMDQiPgo8aW5wdXQgdHlwZT0iaGlkZGVuIiBuYW1lPSJjYXJkVHlwZSIgaWQ9ImNhcmRUeXBlIiB2YWx1ZT0iREMiPgo8aW5wdXQgdHlwZT0iaGlkZGVuIiBuYW1lPSJzaWduIiBpZD0ic2lnbiIgdmFsdWU9IjdGMzUxMTBEQzBCMUVBOTdBRjZBOUU1NDgyMDg0MTAxIj4KPGlucHV0IHR5cGU9ImhpZGRlbiIgbmFtZT0ibm90aWZ5VXJsIiBpZD0ibm90aWZ5VXJsIiB2YWx1ZT0iaHR0cDovL29ubGluZS50aWFuY2VudC5jb206ODA4MC9wYXltZW50L2JhY2tFbmRDYWxsQmFja193aWtpcGF5X2p5Lmh0bSI+CjxpbnB1dCB0eXBlPSJoaWRkZW4iIG5hbWU9Im9yZGVyU3ViamVjdCIgaWQ9Im9yZGVyU3ViamVjdCIgdmFsdWU9IlBheSI+CjxpbnB1dCB0eXBlPSJoaWRkZW4iIG5hbWU9InJlcXVlc3RJcCIgaWQ9InJlcXVlc3RJcCIgdmFsdWU9IjEyNy4wLjAuMSI+CjxpbnB1dCB0eXBlPSJoaWRkZW4iIG5hbWU9InJldHVyblVybCIgaWQ9InJldHVyblVybCIgdmFsdWU9Imh0dHA6Ly9vbmxpbmUudGlhbmNlbnQuY29tOjgwODAvcGF5bWVudC9iYWNrRW5kQ2FsbEJhY2tfd2lraXBheV9UQi5odG0iPgo8aW5wdXQgdHlwZT0iaGlkZGVuIiBuYW1lPSJtZXJjaGFudE5vIiBpZD0ibWVyY2hhbnRObyIgdmFsdWU9IjYyRkFFQzQ5MDIiPgo8L2Zvcm0+Cg0KDQogICAJPC9ib2R5Pg0KPC9odG1sPg0K","state":1,"err":"请求成功"}
            int state = JsonAgent.GetValue<int>(result, "state");
            string error = JsonAgent.GetValue<string>(result, "err");
            if (state != 1)
            {
                context.Response.Write(error + "<!-- " + url + " -->");
                return;
            }
            string data = JsonAgent.GetValue<string>(result, "data");
            try
            {
                data = WebAgent.Base64ToString(data, Encoding.UTF8);
                if (data.StartsWith("<"))
                {
                    this.context.Response.Write(data);
                    return;
                }
            }
            catch
            {

            }
            context.Response.Write("<!-- " + data + " -->");
            if (this.gpaytype == "1")
            {
                this.BuildForm(data);
            }
            this.CreateQRCode(data);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            //aorder : 20180627182203859      bmoney : 10      cpacc : 2018      state : 2      sign : 8a9d8538a87d2132b78fbebe29b9a400
            string aorder = WebAgent.GetParam("aorder");
            string bmoney = WebAgent.GetParam("bmoney");
            string cpacc = WebAgent.GetParam("cpacc");
            string state = WebAgent.GetParam("state");
            string sign = WebAgent.GetParam("sign");
            if (state != "2") return false;

            string signStr = string.Concat(aorder, bmoney, cpacc, state, this.Key);
            if (MD5.toMD5(signStr).ToLower() != sign) return false;
            callback.Invoke();
            return true;
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (!new string[] { "1", "2", "3" }.Contains(this.gpaytype)) return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.BOC, "1000");
                dic.Add(BankType.ABC, "1001");
                dic.Add(BankType.ICBC, "1002");
                dic.Add(BankType.CCB, "1003");
                dic.Add(BankType.CMB, "1004");
                dic.Add(BankType.CMBC, "1005");
                dic.Add(BankType.CIB, "1006");
                dic.Add(BankType.COMM, "1007");
                dic.Add(BankType.CEB, "1008");
                dic.Add(BankType.SPDB, "1009");
                dic.Add(BankType.SPABANK, "1010");
                dic.Add(BankType.GDB, "1011");
                dic.Add(BankType.CITIC, "1012");
                dic.Add(BankType.BJBANK, "1013");
                dic.Add(BankType.PSBC, "1014");
                dic.Add(BankType.SHBANK, "1015");
                dic.Add(BankType.HXBANK, "1016");
                dic.Add(BankType.BOHAIB, "1017");
                dic.Add(BankType.NBBANK, "1018");
                dic.Add(BankType.HZCB, "1019");
                return dic;
            }
        }
    }
}
