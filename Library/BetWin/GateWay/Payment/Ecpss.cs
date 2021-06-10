using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;
using SP.Studio.Security;

using SP.Studio.Web;
using SP.Studio.Model;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 回潮
    /// </summary>
    public class Ecpss : IPayment
    {
        public Ecpss() : base() { }

        public Ecpss(string setting) : base(setting) { }

        /// <summary>
        /// 私钥
        /// </summary>
        [Description("私钥")]
        public string MD5key { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [Description("商户号")]
        public string MerNo { get; set; }

        /// <summary>
        /// 返回地址[]
        /// </summary>
        [Description("返回地址")]
        public string ReturnURL { get; set; }

        /// <summary>
        /// [必填]支付完成后，后台接收支付结果，可用来更新数据库值
        /// </summary>
        [Description("服务器通知地址")]
        public string AdviceURL { get; set; }

        private string _url = "https://pay.ecpss.com/sslpayment";
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

        public override void GoGateway()
        {
            string sign = MD5.toMD5(string.Join("&", new string[]{
                this.MerNo,
                this.OrderID,
                this.Money.ToString("0.00"),
                this.ReturnURL,
                this.MD5key
            }));

            string TradeDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>正在提交...</title></head><body>");
            sb.AppendFormat("<form name=\"{1}\" method=\"post\" action=\"{0}\" id=\"{1}\">", this.Url, this.GetType().Name);
            sb.Append(this.CreateInput("MerNo", this.MerNo));
            sb.Append(this.CreateInput("BillNo", this.OrderID));
            sb.Append(this.CreateInput("InterfaceVersion", "4.0"));    ////版本 当前为4.0请勿修改 
            sb.Append(this.CreateInput("KeyType", "1"));   //加密方式默认1 MD5
            sb.Append(this.CreateInput("OrderTime", TradeDate));
            sb.Append(this.CreateInput("Amount", this.Money.ToString("0.00")));
            sb.Append(this.CreateInput("ProductName", this.Name));
            sb.Append(this.CreateInput("Username", ""));
            sb.Append(this.CreateInput("AdditionalInfo", ""));
            sb.Append(this.CreateInput("AdviceURL", this.AdviceURL));
            sb.Append(this.CreateInput("ReturnURL", this.ReturnURL));
            sb.Append(this.CreateInput("SignInfo", sign));
            sb.Append(this.CreateInput("products", this.Description));
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);
            sb.Append("</body></html>");


            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string BillNo = WebAgent.GetParam("BillNo");//商户终端号
            string result = WebAgent.GetParam("Result");//支付结果(1:成功,0:失败)
            string resultDesc = WebAgent.GetParam("ResultDesc");//支付结果描述
            string Amount = WebAgent.GetParam("Amount");//实际成交金额
            string Succeed = WebAgent.GetParam("Succeed");//支付状态:该值说明见于word说明文档[商户根据该值来修改数据库中相应订单的状态]
            string SignMD5info = WebAgent.GetParam("SignMD5info");//md5签名

            string md5sign = MD5.toMD5(string.Join("&", new string[]{
                 BillNo, Amount,Succeed, MD5key}));


            if (SignMD5info.Equals(md5sign, StringComparison.CurrentCultureIgnoreCase))
            {
                if (Succeed == "88")
                {
                    callback.Invoke();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("Amount", 0.00M);
            systemId = "";
            return WebAgent.GetParam("BillNo");
        }


     
    }
}
