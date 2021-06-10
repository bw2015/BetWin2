using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Payment
{
    public class MKPay : IPayment
    {
        [Description("网关")]
        public string Gateway { get; set; } = "	http://mkpay.westful.top/index/payApi.html";

        [Description("登录帐号")]
        public string apiAccount { get; set; }

        [Description("加密因子")]
        public string Key { get; set; } = "2019070302535531205242";

        [Description("类型")]
        public string payType { get; set; } = "3";

        public MKPay()
        {
        }

        public MKPay(string settingString) : base(settingString)
        {
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("tradeNo");
            money = WebAgent.GetParam("tradeAmount", decimal.Zero);
            return WebAgent.GetParam("orderNo");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"orderNo",this.OrderID },
                {"tradeAmount",this.Money.ToString("0.00") },
                {"payType",this.payType },
                {"apiAccount",this.apiAccount },
                {"backUrl",System.Web.HttpUtility.UrlEncode(this.GetUrl("/")) }
            };

            string signStr = dic["orderNo"] + dic["tradeAmount"] + dic["payType"] + dic["apiAccount"];

            string token = this.GetSign(signStr);
            dic.Add("token", token);
            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            //http://ges.geesfe.com:88/handler/payment/MKPay?orderNo=20190703153717551&tradeNo=I703394378093047&
            //tradeAmount =1.00&accountType=3&tradeTime=2019-07-03+15%3A39%3A43&token=1e2950c01e699a7fc6d363d3fec31625&appAccount=user

            string orderNo = WebAgent.GetParam("orderNo"); //代理订单号/ 流水号
            string tradeNo = WebAgent.GetParam("tradeNo"); //平台交易号/ 流水号
            string tradeAmount = WebAgent.GetParam("tradeAmount"); //收款金额
            string accountType = WebAgent.GetParam("accountType"); //收款类型
            string tradeTime = WebAgent.GetParam("tradeTime"); //收款时间
            string appAccount = WebAgent.GetParam("appAccount"); //收款APP账号
            //orderNo+tradeAmount+appAccount
            string signStr = orderNo + tradeAmount + appAccount;

            string token = this.GetSign(signStr);
            if (token == WebAgent.GetParam("token"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        private string GetSign(string signStr)
        {
            return MD5.toMD5(MD5.toMD5(signStr).ToLower() + this.Key).ToLower();
        }
    }
}
