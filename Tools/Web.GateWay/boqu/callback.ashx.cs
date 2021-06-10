using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Net;
using System.Net;

namespace Web.GateWay.boqu
{
    /// <summary>
    /// 通汇卡的回调
    /// </summary>
    public class callback : IHttpHandler
    {
        //http://a8.to/boqu/callback.ashx?
        //merchant_code=12481996&notify_type=page_notify&order_no=282314099482824704&
        //order_amount=1.0&order_time=2016-12-16%2016%3A16%3A30&return_params=
        //&trade_no=3063649124194617&trade_time=2016-12-16%2016%3A16%3A34&trade_status=success&
        //sign=071fedbb64a13fc204c0a58fc88fe199

        /// <summary>
        /// 参数字符集编码
        /// </summary>
        private const string input_charset = "UTF-8";

        private Dictionary<string, string> data = new Dictionary<string, string>();

        /// <summary>
        /// 根据商户号自动获取密钥
        /// </summary>
        private string Key
        {
            get
            {
                string member = WebAgent.GetParam("merchant_code");
                return data[member];
            }
        }

        /// <summary>
        /// 奇迹的口袋回调通知地址
        /// </summary>
        private const string KD_RESULT_URL = "http://www.boqu777.com/kz/pfb/kdpay";

        /// <summary>
        /// 奇迹的口袋回调访问地址
        /// </summary>
        private const string KD_NOTIFY_URL = "http://www.boqu777.com/wallet/deposit";

        /// <summary>
        /// 口袋账户名
        /// </summary>
        private const string KD_ID = "1008888";

        /// <summary>
        /// 口袋密钥
        /// </summary>
        private const string KD_KEY = "a35dd5c675870933c3b9a49f42204728";

        public void ProcessRequest(HttpContext context)
        {
            // 微信
            data.Add("12481996", "c2d260a3d2644b298496e35667526e8a");
            // 网银
            data.Add("17118371", "d04e9b971d2a49f4a7ef338e6e5cc969");


            context.Response.ContentType = "text/html";

            if (!this.Verify(() =>
            {
                decimal money;
                string systemId;
                string orderId = this.GetTradeNo(out money, out systemId);

                context.Response.Redirect(this.Notify(orderId, systemId, money));
            }))
            {
                context.Response.Write("密钥验证错误");
            }

        }

        /// <summary>
        /// 检查通汇卡的回调信息是否正确
        /// </summary>
        /// <returns></returns>
        private bool Verify(Action callback)
        {
            string trade_status = WebAgent.GetParam("trade_status");
            if (trade_status != "success") return false;

            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            foreach (string key in new string[] { "merchant_code", "notify_type", "order_no", "order_amount", "order_time", "return_params", "trade_no", "trade_time", "trade_status" })
            {
                dic.Add(key, WebAgent.GetParam(key));
            }
            string sign = WebAgent.GetParam("sign");
            if (sign == this.Sign(dic))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("order_amount", 0.00M);
            systemId = WebAgent.GetParam("trade_no");
            return WebAgent.GetParam("order_no");
        }

        private string Sign(SortedDictionary<string, string> dic)
        {
            string queryString = string.Join("&", dic.Where(t => !string.IsNullOrEmpty(t.Value)).Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            return SP.Studio.Security.MD5.Encryp(string.Concat(queryString, "&key=", this.Key), input_charset).ToLower();
        }

        /// <summary>
        /// 通知奇迹后台(构造口袋的通知函数）
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="systemId"></param>
        /// <param name="money"></param>
        private string Notify(string orderId, string systemId, decimal money)
        {
            // 提交的数据
            //P_UserId=1004637&P_OrderId=282324042072068096&P_FaceValue=1.00&P_ChannelId=21&P_Price=1&P_Quantity=100&P_Result_URL=http://www.boqu777.com/kz/pfb/kdpay&P_Notify_URL=http://www.boqu777.com/wallet/deposit&P_PostKey=4826c2a5734a1a9cfd97e0cbbea6f001&P_IsSmart=0


            // 回调的数据
            Dictionary<string, string> post = new Dictionary<string, string>();
            //P_UserId=1002188&P_OrderId=20161216165306601&P_CardId=636175039862775864&P_CardPass=40f7cd113c2c4945&P_FaceValue=1.00000&P_ChannelId=21&


            //http://a8.to/boqu/callback.ashx?merchant_code=12481996&order_no=285300540282376192&order_amount=1.0&
            //order_time=2016-12-24%2022%3A03%3A14&notify_type=back_notify&return_params=&trade_no=3063649836198720&
            //trade_time=2016-12-24%2022%3A03%3A18&trade_status=success&sign=cb24eaaff846fe48dfb2d9fff8f358be

            post.Add("P_UserId", KD_ID);
            post.Add("P_OrderId", orderId);
            post.Add("P_CardId", "");
            post.Add("P_CardPass", "");
            //00000
            post.Add("P_FaceValue", money.ToString("0.00"));
            post.Add("P_ChannelId", "21");

            //P_PayMoney=1.00&P_Subject=test&P_Price=1.0000&P_Quantity=1&P_Description=&P_Notic=1481878386&P_ErrCode=0&P_PostKey=26ab5d5d10b40f7a6491507b04414951&P_ErrMsg=%d6%a7%b8%b6%b3%c9%b9%a6
            post.Add("P_PayMoney", money.ToString("0.00"));
            post.Add("P_Subject", "");
            post.Add("P_Price", money.ToString("0.00"));
            post.Add("P_Quantity", "1");
            post.Add("P_Description", "");
            post.Add("P_ErrCode", "0");

            post.Add("P_Notic", WebAgent.GetTimeStamp().ToString());
            //P_PostKey=md5_32(P_UserId|P_OrderId|P_CardId|P_CardPass|P_FaceValue|P_ChannelId|P_PayMoney|P_ErrCode|SalfStr)
            string P_PostKey = MD5.toMD5(string.Join("|", new string[]{
                post["P_UserId"],
                post["P_OrderId"],
                post["P_CardId"],
                post["P_CardPass"],
                post["P_FaceValue"],
                post["P_ChannelId"],
                post["P_PayMoney"],
                post["P_ErrCode"],
                KD_KEY
            })).ToLower();

            post.Add("P_PostKey", P_PostKey);

            post.Add("P_ErrMsg", "支付成功");


            //

            string data = string.Join("&", post.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string url = KD_NOTIFY_URL + "?" + data;

            string result;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("X-Forwarded-For", "120.26.8.227");
                result = NetAgent.DownloadData(KD_RESULT_URL + "?" + data, Encoding.UTF8, wc);
            }

            return url;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}