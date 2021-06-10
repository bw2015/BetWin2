using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;


using BW.Common.Sites;
using SP.Studio.Security;
using SP.Studio.Web;
using SP.Studio.Model;
using SP.Studio.Net;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 爱扬网络
    /// </summary>
    public class Admin523 : IPayment
    {
        public Admin523() { }

        public Admin523(string setting)
            : base(setting)
        {

        }

        [Description("商户ID")]
        public string parter { get; set; }

        private string _callbackurl = "/handler/payment/Admin523";

        [Description("异步通知")]
        public string callbackurl { get { return this._callbackurl; } set { this._callbackurl = value; } }


        private string _hrefbackurl = "/handler/payment/Admin523";
        [Description("同步通知")]
        public string hrefbackurl { get { return this._hrefbackurl; } set { this._hrefbackurl = value; } }

        [Description("密钥")]
        public string key { get; set; }

        [Description("类型 支付宝:992 微信:1004")]
        public string type { get; set; }

        private string _gateway = "http://pay.admin523.cn/bank/";
        [Description("网关")]
        public string GATEWAY
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

        public override bool IsWechat()
        {
            return this.type == "1004" || this.type == "1007";
        }

        public override string ShowCallback()
        {
            return "opstate=0";
        }

        public override void GoGateway()
        {
            string type = this.BankCode == null ? this.type : this.BankValue;

            string sign = MD5.toMD5(string.Format("parter={0}&type={1}&value={2}&orderid={3}&callbackurl={4}{5}", this.parter, type, this.Money.ToString("0.00"), this.OrderID, this.GetUrl(this.callbackurl), this.key)).ToLower();

            //http://pay.admin523.cn/bank/?parter=99&type=963&value=100.00&orderid=1234567890&
            //callbackurl=http://www.0n2.com/backAction&refbackurl=http://www.0n2.com/notifyAction&
            //payerIp =127.0.0.1&attach=ceshi&sign=fde74a4c040b5022cd9c4d9e6b917fcc

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("parter", this.parter);
            dic.Add("type", type);
            dic.Add("value", this.Money.ToString("0.00"));
            dic.Add("orderid", this.OrderID);
            dic.Add("callbackurl", this.GetUrl(this.callbackurl));
            dic.Add("refbackurl", this.GetUrl(this.hrefbackurl));
            dic.Add("payerIp", IPAgent.IP);
            dic.Add("attach", this.Description);
            dic.Add("sign", sign);

            if (this.IsWechat() && (WebAgent.IsWechat() || WebAgent.GetParam("wechat", 0) == 1))
            {
                string url = string.Format("{0}?{1}", GATEWAY, string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))));
                string result = NetAgent.DownloadData(url, Encoding.GetEncoding("GB2312"));
                Regex regex = new Regex(@"<img id=""Image2"" src=""(?<Code>[^""]+)""");
                if (regex.IsMatch(result))
                {
                    HttpContext.Current.Response.Write(true, "订单提交成功", new
                    {
                        data = string.Format("http://pay.admin523.cn/Weixin/{0}", regex.Match(result).Groups["Code"].Value)
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

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<form action=\"{0}\" method=\"get\" id=\"{1}\">", GATEWAY, this.GetType().Name);
                sb.Append(this.CreateInput(_GATEWAY, GATEWAY));
                sb.Append(string.Join(string.Empty, dic.Select(t => this.CreateInput(t.Key, t.Value))));
                sb.Append("</form>");
                sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

                HttpContext.Current.Response.Write(sb);
                HttpContext.Current.Response.End();
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            decimal money;
            string systemId;
            string ordertId = this.GetTradeNo(out money, out systemId);
            string opstate = WebAgent.GetParam("opstate");
            if (opstate != "0") return false;

            string sign = MD5.toMD5(string.Format("orderid={0}&opstate={1}&ovalue={2}{3}", ordertId, opstate, money.ToString("0.00"), this.key)).ToLower();
            if (sign == WebAgent.GetParam("sign"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("ovalue", 0.00M);
            systemId = WebAgent.GetParam("sysorderid");
            return WebAgent.GetParam("orderid");
        }

        private Dictionary<BankType, string> _code;

        /// <summary>
        /// 银行代码转换
        /// </summary>
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (!string.IsNullOrEmpty(this.type)) return null;
                if (_code == null)
                {
                    _code = new Dictionary<BankType, string>();
                    _code.Add(BankType.CITIC, "962");
                    _code.Add(BankType.BOC, "963");
                    _code.Add(BankType.ABC, "964");
                    _code.Add(BankType.CCB, "965");
                    _code.Add(BankType.ICBC, "967");
                    _code.Add(BankType.CZBANK, "968");
                    _code.Add(BankType.CZCB, "969");
                    _code.Add(BankType.CMB, "970");
                    _code.Add(BankType.PSBC, "971");
                    _code.Add(BankType.CIB, "972");
                    _code.Add(BankType.SDEB, "973");
                    _code.Add(BankType.SPABANK, "978");
                    _code.Add(BankType.SHBANK, "975");
                    _code.Add(BankType.SPDB, "977");
                    _code.Add(BankType.NJCB, "979");
                    _code.Add(BankType.CMBC, "980");
                    _code.Add(BankType.COMM, "981");
                    _code.Add(BankType.HXBANK, "982");
                    _code.Add(BankType.HZCB, "983");
                    _code.Add(BankType.GDB, "985");
                    _code.Add(BankType.CEB, "986");
                    _code.Add(BankType.HKBEA, "987");
                    _code.Add(BankType.BOHAIB, "988");
                    _code.Add(BankType.BJBANK, "989");
                }
                return _code;
            }
        }
    }
}
