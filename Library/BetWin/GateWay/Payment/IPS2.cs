using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;
using System.Xml.Linq;

using SP.Studio.Xml;
using SP.Studio.Web;
using SP.Studio.Model;
using SP.Studio.Text;
using BW.Common.Sites;

using System.Text.RegularExpressions;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class IPS2 : IPayment
    {
        public IPS2() : base() { }

        public IPS2(string setting) : base(setting) { }

        [Description("商户号")]
        public string MerCode { get; set; }

        [Description("交易账户号")]
        public string Account { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        [Description("返回地址")]
        public string Merchanturl { get; set; }

        [Description("通知地址")]
        public string ServerUrl { get; set; }

        private string _url = "https://newpay.ips.com.cn/psfp-entry/gateway/payment.do";
        [Description("支付网关")]
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }


        /// <summary>
        /// 商城域名
        /// </summary>
        [Description("商城域名")]
        public string Shop { get; set; }

        private string _gatewayType = "01";
        [Description("01网银 10微信 11支付宝")]
        public string GatewayType
        {
            get
            {
                return this._gatewayType;
            }
            set
            {
                this._gatewayType = value;
            }
        }

        /// <summary>
        /// 微信的支付网关
        /// </summary>
        private const string WXGATEWAY = "https://thumbpay.e-years.com/psfp-webscan/onlinePay.do";

        /// <summary>
        /// 创建一个微信充值的提交内容
        /// </summary>
        /// <returns></returns>
        private string CreateBody()
        {
            StringBuilder body = new StringBuilder();
            body.Append("<body>");
            switch (this.GatewayType)
            {
                case "01":
                    body
                        .AppendFormat("<MerBillNo>{0}</MerBillNo>", this.OrderID)
                        .AppendFormat("<Amount>{0}</Amount>", this.Money.ToString("0.00"))
                        .AppendFormat("<Date>{0}</Date>", DateTime.Now.ToString("yyyyMMdd"))
                        .AppendFormat("<CurrencyType>156</CurrencyType>")
                        .AppendFormat("<GatewayType>{0}</GatewayType>", this.GatewayType)
                        .AppendFormat("<Lang>GB</Lang>")
                        .AppendFormat("<FailUrl><![CDATA[]]></FailUrl>")
                        .AppendFormat("<Attach><![CDATA[]]></Attach>")
                        .AppendFormat("<OrderEncodeType>5</OrderEncodeType>")
                        .AppendFormat("<RetEncodeType>17</RetEncodeType>")
                        .AppendFormat("<RetType>1</RetType>")
                        .AppendFormat("<ServerUrl><![CDATA[{0}]]></ServerUrl>", this.GetUrl(this.ServerUrl))
                        .AppendFormat("<BillEXP></BillEXP>")
                        .AppendFormat("<GoodsName>{0}</GoodsName>", this.Name)
                        .AppendFormat("<Merchanturl><![CDATA[{0}]]></Merchanturl>", this.GetUrl(this.Merchanturl))
                        .AppendFormat("<IsCredit>1</IsCredit>")
                        .AppendFormat("<BankCode>{0}</BankCode>", this.BankValue)
                        .AppendFormat("<ProductType>1</ProductType>");
                    break;
                case "10":
                    body
                        .AppendFormat("<MerBillno>{0}</MerBillno>", this.OrderID)
                        .AppendFormat("<GoodsInfo><GoodsName>{0}</GoodsName><GoodsCount ></GoodsCount></GoodsInfo>", this.Name)
                        .AppendFormat("<OrdAmt>{0}</OrdAmt>", (int)this.Money)
                        .AppendFormat("<OrdTime>{0}</OrdTime>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        .AppendFormat("<MerchantUrl>{0}</MerchantUrl>", this.GetUrl(this.Merchanturl))
                        .AppendFormat("<ServerUrl>{0}</ServerUrl>", this.GetUrl(this.ServerUrl))
                        .AppendFormat("<BillEXP></BillEXP>")
                        .AppendFormat("<ReachBy>{0}</ReachBy>", this.Description)
                        .AppendFormat("<ReachAddress>{0}</ReachAddress>", this.Description)
                        .AppendFormat("<CurrencyType>156</CurrencyType>")
                        .AppendFormat("<Attach>{0}</Attach>", this.Name)
                        .AppendFormat("<RetEncodeType>17</RetEncodeType>");
                    break;
                case "11":
                    body
                        .AppendFormat("<MerBillNo>{0}</MerBillNo>", this.OrderID)
                        .AppendFormat("<GatewayType>{0}</GatewayType>", this.GatewayType)
                        .AppendFormat("<Date>{0}</Date>", DateTime.Now.ToString("yyyyMMdd"))
                        .AppendFormat("<CurrencyType>156</CurrencyType>")
                        .AppendFormat("<Amount>{0}</Amount>", this.Money.ToString("0.00"))
                        .AppendFormat("<Lang>GB</Lang>")
                        .AppendFormat("<Attach>{0}</Attach>", this.Description)
                        .AppendFormat("<RetEncodeType>17</RetEncodeType>")
                        .AppendFormat("<ServerUrl>{0}</ServerUrl>", this.GetUrl(this.ServerUrl))
                        .AppendFormat("<BillEXP>2</BillEXP>")
                        .AppendFormat("<GoodsName>{0}</GoodsName>", this.Name);
                    break;
            }
            body.Append("</body>");
            return body.ToString();
        }

        /// <summary>
        /// 创建要提交的充值数据
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private string CreateXml()
        {
            StringBuilder xml = new StringBuilder();
            StringBuilder head = new StringBuilder();
            string body = this.CreateBody();

            head.Append("<head>")
                .AppendFormat("<Version>{0}</Version>", "v1.0.0")
                .AppendFormat("<MerCode>{0}</MerCode>", this.MerCode)
                .AppendFormat("<MerName>{0}</MerName>", this.Description)
                .AppendFormat("<Account>{0}</Account>", this.Account)
                .AppendFormat("<MsgId>{0}</MsgId>", Guid.NewGuid().ToString("N").Substring(0, 8))
                .AppendFormat("<ReqDate>{0}</ReqDate>", DateTime.Now.ToString("yyyyMMddHHmmss"))
                .AppendFormat("<Signature>{0}</Signature>", SP.Studio.Security.MD5.toMD5(body + this.MerCode + this.Key).ToLower())
                .Append("</head>");

            switch (this.GatewayType)
            {
                case "01":
                    xml.Append("<Ips>")
                       .Append("<GateWayReq>")
                       .Append(head.ToString())
                       .Append(body.ToString())
                       .Append("</GateWayReq>")
                       .Append("</Ips>");
                    break;
                case "10":
                    xml.Append("<Ips>")
                      .Append("<WxPayReq>")
                      .Append(head.ToString())
                      .Append(body.ToString())
                      .Append("</WxPayReq>")
                      .Append("</Ips>");
                    break;
                case "11":
                    xml.Append("<Ips>")
                      .Append("<GateWayReq>")
                      .Append(head.ToString())
                      .Append(body.ToString())
                      .Append("</GateWayReq>")
                      .Append("</Ips>");
                    break;
            }
            return xml.ToString();
        }

        public override void GoGateway()
        {
            string body = this.CreateXml();

            string gateway = this.Url;
            if (!string.IsNullOrEmpty(this.Shop))
            {
                gateway = this.Shop.Contains('/') ? this.Shop : string.Format("//{0}/handler/payment/Redirect", this.Shop);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>正在提交...</title></head><body>");
            sb.AppendFormat("<form name=\"{1}\" method=\"post\" action=\"{0}\" id=\"{1}\">", gateway, this.GetType().Name);
            switch (this.GatewayType)
            {
                case "01":
                    sb.Append(this.CreateInput("pGateWayReq", body));
                    sb.Append(this.CreateInput(_GATEWAY, this.Url));
                    break;
                case "10":
                    sb.Append(this.CreateInput("wxPayReq", body));
                    sb.Append(this.CreateInput(_GATEWAY, WXGATEWAY));
                    sb.Append(this.CreateInput(_GATETYPE, "wx"));
                    sb.Append(this.CreateInput("_money", this.Money));
                    sb.Append(this.CreateInput("_orderid", this.OrderID));
                    break;
                case "11":
                    string result = new Remote.Payment.IPS2.WSScan().scanPay(body);
                    Regex regex = new Regex(@"\<QrCode\>(?<Code>[^\<]+)</QrCode>");
                    if (regex.IsMatch(result))
                    {
                        result = regex.Match(result).Groups["Code"].Value;
                    }
                    else
                    {
                        regex = new Regex(@"\<RspMsg\>(?<Msg>.+?)</RspMsg>");
                        if (regex.IsMatch(result))
                        {
                            result = regex.Match(result).Groups["Msg"].Value;
                        }
                        else
                        {
                            result = HttpUtility.HtmlEncode(result);
                        }
                    }
                    sb.Append(this.CreateInput("_qrcode", result));
                    sb.Append(this.CreateInput(_GATETYPE, "alipay"));
                    sb.Append(this.CreateInput("_money", this.Money));
                    sb.Append(this.CreateInput("_orderid", this.OrderID));
                    break;
            }
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);
            sb.Append("</body></html>");


            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }



        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.GatewayType == "01")
                {
                    Dictionary<BankType, string> _code = new Dictionary<BankType, string>();
                    _code.Add(BankType.ICBC, "1100");
                    _code.Add(BankType.ABC, "1101");
                    _code.Add(BankType.CMB, "1102");
                    _code.Add(BankType.CIB, "1103");
                    _code.Add(BankType.CITIC, "1104");
                    _code.Add(BankType.CCB, "1106");
                    _code.Add(BankType.BOC, "1107");
                    _code.Add(BankType.COMM, "1108");
                    _code.Add(BankType.SPDB, "1109");
                    _code.Add(BankType.CMBC, "1110");
                    _code.Add(BankType.HXBANK, "1111");
                    _code.Add(BankType.CEB, "1112");
                    _code.Add(BankType.BJBANK, "1113");
                    _code.Add(BankType.GDB, "1114");
                    _code.Add(BankType.NJCB, "1115");
                    _code.Add(BankType.SHBANK, "1116");
                    _code.Add(BankType.HZCB, "1117");
                    _code.Add(BankType.NBBANK, "1118");
                    _code.Add(BankType.PSBC, "1119");
                    _code.Add(BankType.CZBANK, "1120");
                    _code.Add(BankType.SPABANK, "1121");
                    _code.Add(BankType.HKBEA, "1122");
                    _code.Add(BankType.BOHAIB, "1123");
                    _code.Add(BankType.BJRCB, "1124");
                    _code.Add(BankType.ZJTLCB, "1127");
                    return _code;
                }
                else
                {
                    return null;
                }
            }
        }


        public override bool Verify(VerifyCallBack callback)
        {
            //paymentResult = 
            //<Ips><WxPayRsp><head><ReferenceID>accf2675</ReferenceID><RspCode>000000</RspCode><RspMsg><![CDATA[交易成功！]]></RspMsg><ReqDate>20170213112631</ReqDate><RspDate>20170213112643</RspDate><Signature>d810b208f253bbeada65f544db60b807</Signature></head></WxPayRsp></Ips>

            //<body><MerBillno>20170213112450362</MerBillno><MerCode>192113</MerCode><Account>1921130017</Account>
            //<IpsBillno>BO20170213112631091768</IpsBillno><IpsBillTime>2017-02-13 11:26:43</IpsBillTime><OrdAmt>1</OrdAmt><Status>Y</Status><RetEncodeType>17</RetEncodeType></body>

            string paymentResult = WebAgent.GetParam("paymentResult")
               .Replace("WxPayRsp", "GateWayRsp");

            XElement xml = XElement.Parse(paymentResult);

            string status = xml.GetValue("GateWayRsp/body/Status");
            if (status != "Y") return false;

            string sign = xml.GetValue("GateWayRsp/head/Signature");
            string body = string.Format("<body>{0}</body>", StringAgent.GetString(paymentResult, "<body>", "</body>"));
            string signCode = SP.Studio.Security.MD5.toMD5(body + this.MerCode + this.Key).ToLower();
            if (sign == signCode)
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            string paymentResult = WebAgent.GetParam("paymentResult");
            XElement xml = XElement.Parse(paymentResult);
            string orderId = string.Empty;

            if (paymentResult.Contains("WxPayRsp"))
            {
                money = xml.GetValue("WxPayRsp/body/OrdAmt", 0.00M);
                systemId = xml.GetValue("WxPayRsp/body/IpsBillno");
                orderId = xml.GetValue("WxPayRsp/body/MerBillno");
            }
            else
            {
                money = xml.GetValue("GateWayRsp/body/Amount", 0.00M);
                systemId = xml.GetValue("GateWayRsp/body/IpsBillNo");
                orderId = xml.GetValue("GateWayRsp/body/MerBillNo");
            }


            return orderId;
        }
    }
}
