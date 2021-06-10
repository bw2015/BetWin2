using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;

using SP.Studio.Model;
using SP.Studio.Web;
using BW.Framework;


namespace BW.GateWay.Payment
{
    /// <summary>
    /// 线下汇款
    /// </summary>
    public class None : IPayment
    {
        [Description("开户行")]
        public string BankName { get; set; }

        [Description("账号")]
        public string Account { get; set; }

        [Description("账户名")]
        public string AccountName { get; set; }

        private string _gateway = "/handler/payment/Redirect";
        [Description("网银网址")]
        public string GateWay
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

        [Description("二维码")]
        public string QRCode { get; set; }

        [Description("备注信息")]
        public string Remark { get; set; }

        private string _method = "GET";
        [Description("动作")]
        public string Method
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value;
            }
        }

        [Description("类型 微信：WX 支付宝：ALIPAY 网银：BANK")]
        public string Type { get; set; }

        public None() : base() { }

        public None(string setting) : base(setting) { }

        public override bool IsWechat()
        {
            if (this.Type == "WX" || this.Type == "ALIPAY")
            {
                return base.IsWechat();
            }
            return false;
        }

        protected override string GetMark()
        {
            return this.Remark;
        }

        public override void GoGateway()
        {
            if (this.IsWechat())
            {
                this.CreateWechatPayment(true, this.QRCode);
            }

            Dictionary<string, string> data = new Dictionary<string, string>();

            string gateway = "/handler/payment/Redirect";
            string method = "POST";
            switch (this.Type)
            {
                case "WX":
                    data.Add("_gatetype", "wx");
                    data.Add("_money", this.Money.ToString("0.00"));
                    data.Add("_orderid", this.OrderID);
                    data.Add("_code", this.QRCode);
                    break;
                case "ALIPAY":
                    data.Add("_gatetype", "alipay");
                    data.Add("_money", this.Money.ToString("0.00"));
                    data.Add("_orderid", this.OrderID);
                    data.Add("_qrcode", this.QRCode);
                    data.Add("_code", this.QRCode);
                    break;
                case "BANK":
                    data.Add("_gatetype", "bank");
                    data.Add("_money", this.Money.ToString("0.00"));
                    data.Add("_orderid", this.OrderID);
                    data.Add("_remark", this.Remark);
                    data.Add("_gateway", this.GateWay);
                    data.Add("_accountname", this.AccountName);
                    data.Add("_account", this.Account);
                    data.Add("_bankname", this.BankName);
                    data.Add("_name", this.Name);
                    break;
                default:
                    gateway = this.GateWay;
                    method = this.Method;
                    data.Add("BankName", this.BankName);
                    data.Add("AccountName", this.AccountName);
                    data.Add("Account", this.Account);
                    break;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>")
              .AppendFormat("<head><title>正在加载...</title></head>")
              .Append("</head><body>");
            sb.AppendFormat("<form action=\"{0}\" method=\"{1}\" id=\"form1\" />", gateway, method);
            sb.Append(string.Join(string.Empty, data.Select(t => this.CreateInput(t.Key, t.Value))));
            sb.Append("</form>");
            sb.Append("<script type=\"text/javascript\"> if(document.getElementById('form1')) document.getElementById('form1').submit(); </script>");
            sb.Append("</body></html>");


            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            throw new NotImplementedException();
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            throw new NotImplementedException();
        }
    }
}
