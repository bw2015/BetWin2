using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Web;

using SP.Studio.Web;
using SP.Studio.Model;
using SP.Studio.Security;
using BW.Common.Sites;
using SP.Studio.Core;
using SP.Studio.Net;
using System.Net;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 口袋通
    /// </summary>
    public class KDPay : IPayment
    {
        public KDPay() { }

        public KDPay(string setting)
            : base(setting)
        {

        }

        [Description("商户ID")]
        public int P_UserId { get; set; }

        [Description("安全码")]
        public string SalfStr { get; set; }

        [Description("回调地址")]
        public string P_Result_url { get; set; }

        [Description("通知地址")]
        public string P_Notify_URL { get; set; }

        [Description("充值类型 1:网银 2:支付宝 21:微信")]
        public int P_ChannelId { get; set; }

        public override bool IsWechat()
        {
            return this.P_ChannelId == 21;
        }

        [Description("商城域名")]
        public string Shop { get; set; }

        /// <summary>
        /// 网银网关地址
        /// </summary>
        private const string GATEWAY = "https://api.duqee.com/pay/KDBank.aspx";

        public override void GoGateway()
        {
            Guid guid;
            Guid.TryParse(this.OrderID, out guid);

            string orderId = guid == Guid.Empty ? this.OrderID : guid.ToString("N");

            string cardId = DateTime.Now.Ticks.ToString();
            string cardPass = Guid.NewGuid().ToString("N").Substring(0, 16);

            //P_PostKey=md5_32(P_UserId|P_OrderId|P_CardId|P_CardPass|P_FaceValue|P_ChannelId|P_PayMoney|P_ErrCode|SalfStr)
            string postKey = MD5.toMD5(string.Join("|",
                new object[]{
                    this.P_UserId,
                    orderId,
                    cardId,
                    cardPass,
                    this.Money.ToString("0.00"),
                    P_ChannelId,
                    this.SalfStr
                })).ToLower();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("P_UserId", this.P_UserId.ToString());
            dic.Add("P_OrderId", orderId);
            dic.Add("P_CardId", cardId);
            dic.Add("P_CardPass", cardPass);
            dic.Add("P_FaceValue", this.Money.ToString("0.00"));
            dic.Add("P_ChannelId", P_ChannelId.ToString());
            dic.Add("P_Subject", this.Name);
            dic.Add("P_Price", this.Money.ToString("0.00"));
            dic.Add("P_Quantity", "1");
            dic.Add("P_Description", this.BankValue);
            dic.Add("P_Notic", WebAgent.GetTimeStamp().ToString());
            dic.Add("P_PostKey", postKey);
            dic.Add("P_Result_url", this.GetUrl(this.P_Result_url));
            dic.Add("P_Notify_URL", this.GetUrl(this.P_Notify_URL));


            string gateUrl = GATEWAY;
            if (!string.IsNullOrEmpty(this.Shop))
            {
                gateUrl = this.Shop.Contains('/') ? this.Shop : string.Format("http://{0}/handler/payment/Redirect", this.Shop);
            }

            WebClient wc = null;
            if (!string.IsNullOrEmpty(this.Shop))
            {
                wc = NetAgent.CreateWebClient(gateUrl);
            }

            StringBuilder sb = new StringBuilder();
            if (this.IsWechat())
            {
                string url = string.Format("{0}?{1}", GATEWAY, string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))));

                string result = NetAgent.DownloadData(url, Encoding.UTF8, wc);
                Regex errorRegex = new Regex(@"""errmsg"": ""(?<Error>.+?)""");
                Regex successRegex1 = new Regex(@"name=""(?<Name>.+?)"" value=""(?<Value>.+?)""");
                Regex successRegex2 = new Regex(@"<img class=""qrcode"" src=""(?<Data>.+?)""");
                if (errorRegex.IsMatch(result))
                {
                    HttpContext.Current.Response.Write(false, errorRegex.Match(result).Groups["Error"].Value);
                }
                else if (successRegex1.IsMatch(result))
                {
                    url = "https://api.duqee.com/pay/wx/qrcodeshow.aspx";
                    dic.Clear();
                    foreach (Match match in successRegex1.Matches(result))
                    {
                        dic.Add(match.Groups["Name"].Value, match.Groups["Value"].Value);
                    }
                    result = NetAgent.UploadData(url, string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))), Encoding.UTF8);
                    if (successRegex2.IsMatch(result))
                    {
                        HttpContext.Current.Response.Write(true, "订单提交成功", new
                        {
                            data = string.Format("//api.duqee.com{0}", successRegex2.Match(result).Groups["Data"].Value)
                        });
                    }
                    else
                    {
                        HttpContext.Current.Response.Write(false, "发生错误", new
                        {
                            data = result
                        });
                    }
                }
                else
                {
                    HttpContext.Current.Response.Write(false, "发生错误", new
                    {
                        data = result
                    });
                }
            }

            sb.AppendFormat("<form action=\"{0}\" method=\"get\" id=\"{1}\">", gateUrl, this.GetType().Name);
            sb.Append(this.CreateInput(_GATEWAY, GATEWAY));
            sb.Append(string.Join(string.Empty, dic.Select(t => this.CreateInput(t.Key, t.Value))));
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();

        }

        public override bool Verify(VerifyCallBack callback)
        {
            //md5_32(P_UserId|P_OrderId|P_CardId|P_CardPass|P_FaceValue|P_ChannelId|P_PayMoney|P_ErrCode|SalfStr)
            string P_PostKey = MD5.toMD5(string.Join("|", new string[]{
                WebAgent.GetParam("P_UserId"),
                WebAgent.GetParam("P_OrderId"),
                WebAgent.GetParam("P_CardId"),
                WebAgent.GetParam("P_CardPass"),
                WebAgent.GetParam("P_FaceValue"),
                WebAgent.GetParam("P_ChannelId"),
                WebAgent.GetParam("P_PayMoney"),
                WebAgent.GetParam("P_ErrCode"),
                this.SalfStr
            })).ToLower();

            if (WebAgent.GetParam("P_PostKey") != P_PostKey) return false;
            int P_ErrCode = WebAgent.GetParam("P_ErrCode", 0);
            if (P_ErrCode != 0) return false;

            callback.Invoke();
            return true;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            string orderId = WebAgent.GetParam("P_OrderId");
            money = WebAgent.GetParam("P_PayMoney", 0.00M);
            systemId = Guid.NewGuid().ToString("N");
            return orderId;
        }

        private Dictionary<BankType, string> _code;

        /// <summary>
        /// 银行代码转换
        /// </summary>
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.P_ChannelId != 1) return null;
                if (_code == null)
                {
                    _code = new Dictionary<BankType, string>();
                    _code.Add(BankType.ICBC, "10001");
                    _code.Add(BankType.ABC, "10002");
                    _code.Add(BankType.CMB, "10003");
                    _code.Add(BankType.BOC, "10004");
                    _code.Add(BankType.CCB, "10005");
                    _code.Add(BankType.CMBC, "10006");
                    _code.Add(BankType.CITIC, "10007");
                    _code.Add(BankType.COMM, "10008");
                    _code.Add(BankType.CIB, "10009");
                    _code.Add(BankType.CEB, "10010");
                    _code.Add(BankType.SPABANK, "10014");
                    _code.Add(BankType.PSBC, "10012");
                    _code.Add(BankType.BJBANK, "10013");
                    _code.Add(BankType.SPDB, "10015");
                    _code.Add(BankType.GDB, "10016");
                    _code.Add(BankType.BOHAIB, "10017");
                    _code.Add(BankType.HKBEA, "10018");
                    _code.Add(BankType.NBBANK, "10019");
                    _code.Add(BankType.BJRCB, "10020");
                    _code.Add(BankType.NJCB, "10021");
                    _code.Add(BankType.CZBANK, "10022");
                    _code.Add(BankType.SHBANK, "10023");
                    _code.Add(BankType.SHRCB, "10024");
                    _code.Add(BankType.HXBANK, "10025");
                    _code.Add(BankType.HZCB, "10027");
                }
                return _code;
            }
        }

    }
}
