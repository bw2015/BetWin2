using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

using SP.Studio.Web;

namespace Web.GateWay.boqu
{
    /// <summary>
    /// ppid=1014&deposituser=%E7%AC%AC%E4%B8%89%E6%96%B9%E6%94%AF%E4%BB%98&amount=100&cardAmt=&frpcardNo=&cardPwd=&bank=10001&orderid=282098963946016768
    /// ppid=1014&deposituser=%E7%AC%AC%E4%B8%89%E6%96%B9%E6%94%AF%E4%BB%98&amount=100&cardAmt=&frpcardNo=&cardPwd=&bank=21&orderid=282104716438151168
    /// </summary>
    public class pay : IHttpHandler
    {
        private const string Gateway = "https://pay.41.cn/gateway";

        /// <summary>
        /// 参数字符集编码
        /// </summary>
        private const string input_charset = "UTF-8";

        private const string pay_type = "1";

        Dictionary<string, string> data = new Dictionary<string, string>();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            string url = string.Format("http://{0}/boqu/callback.ashx", context.Request.Url.Authority);
            string bankType = this.GetBankType(WebAgent.QS("bank"));

            #region =========== 判断是微信还是网银 ============

            switch (bankType)
            {
                case "WEIXIN":
                    // 网银
                    data.Add("merchant_code", "12481996");
                    data.Add("key", "c2d260a3d2644b298496e35667526e8a");
                    break;
                default:
                    // 网银
                    data.Add("merchant_code", "17118371");
                    data.Add("key", "d04e9b971d2a49f4a7ef338e6e5cc969");
                    break;
            }

            #endregion

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<form name=\"{1}\" method=\"post\" action=\"{0}\" id=\"{1}\">", Gateway, this.GetType().Name);

            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();

            dic.Add("input_charset", input_charset);
            dic.Add("notify_url", url);
            dic.Add("return_url", url);
            dic.Add("pay_type", pay_type);
            dic.Add("bank_code", bankType);
            dic.Add("merchant_code", data["merchant_code"]);
            dic.Add("order_no", WebAgent.QS("orderid"));
            dic.Add("order_amount", WebAgent.QS("amount", 0).ToString("0.00"));
            dic.Add("order_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dic.Add("product_name", WebAgent.QS("deposituser"));
            dic.Add("req_referer", HttpContext.Current.Request.Url.ToString());
            dic.Add("customer_ip", IPAgent.IP);

            sb.Append(string.Join("", dic.Select(t => this.CreateInput(t.Key, t.Value))));
            sb.Append(this.CreateInput("sign", this.Sign(dic)));


            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 银行类型转换
        /// </summary>
        /// <param name="bank"></param>
        /// <returns></returns>
        private string GetBankType(string bank)
        {
            string result = "";
            switch (bank)
            {
                case "21":
                    result = "WEIXIN";
                    break;
                case "10001":
                    result = "ICBC";
                    break;
                case "10002":
                    result = "ABC";
                    break;
                case "10003":
                    result = "CMB";
                    break;
                case "10004":
                    result = "BOC";
                    break;
                case "10005":
                    result = "CCB";
                    break;
                case "10006":
                    result = "CMBCS";
                    break;
                case "10007":
                    result = "CMBCS";
                    break;
                case "10008":
                    result = "BOCOM";
                    break;
                case "10009":
                    result = "CIB";
                    break;
                case "10010":
                    result = "CEBBANK";
                    break;
                case "10011":
                case "10014":
                    result = "PINGAN";
                    break;
                case "10012":
                    result = "PSBC";
                    break;
                case "10015":
                    result = "SPDB";
                    break;
                case "10016":
                    result = "CGB";
                    break;
            }
            return result;
        }

        private string CreateInput(string name, object value)
        {
            return string.Format("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", name, value);
        }

        private string Sign(SortedDictionary<string, string> dic)
        {
            string queryString = string.Join("&", dic.Where(t => !string.IsNullOrEmpty(t.Value)).Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            return SP.Studio.Security.MD5.Encryp(string.Concat(queryString, "&key=", this.data["key"]), input_charset).ToLower();
        }
    }
}