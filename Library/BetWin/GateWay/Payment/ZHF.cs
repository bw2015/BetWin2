using BW.Common.Sites;
using SP.Studio.Array;
using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 真好付
    /// </summary>
    public class ZHF : IPayment
    {
        public ZHF()
        {
        }

        public ZHF(string settingString) : base(settingString)
        {
        }

        [Description("网关")]
        public string Gateway { get; set; } = "http://pay1.527460.cn/pay";

        [Description("商户号")]
        public string merchantNo { get; set; }

        [Description("异步通知")]
        public string notifyUrl { get; set; } = "/handler/payment/ZHF";

        [Description("密钥")]
        public string Key { get; set; }

        [Description("产品编码")]
        public string productCode { get; set; }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("orderNo");
            money = WebAgent.GetParam("amount", decimal.Zero) / 100M;
            return WebAgent.GetParam("outOrderNo");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"nonceStr",Guid.NewGuid().ToString("N") },
                {"startTime",DateTime.Now.ToString("yyyyMMddHHmmss") },
                {"merchantNo",this.merchantNo },
                {"outOrderNo",this.OrderID },
                {"amount",((int)this.Money*100M).ToString() },
                {"client_ip",IPAgent.IP },
                {"timestamp",WebAgent.GetTimeStamps().ToString() },
                {"description","PAY" },
                {"notifyUrl",this.GetUrl(this.notifyUrl) },
                {"returnUrl",this.GetUrl(this.notifyUrl) },
                {"extra","PAYMENT" },
                {"productCode",this.productCode }
            };
            if (this.productCode == "1601") data.Add("bankCode", this.BankValue);
            string signStr = data.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.Key;
            data.Add("sign", MD5.toMD5(signStr));

            this.BuildForm(data, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.QF("orderStatus", 0) != 1) return false;
            callback.Invoke();
            return true;
        }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.productCode != "1601") return null;
                return new Dictionary<BankType, string>()
                {
                    {BankType.ABC,"ABC" },
                    {BankType.BOC,"BOC" },
                    {BankType.CCB,"CCB" },
                    {BankType.CEB,"CEB" },
                    {BankType.CIB,"CIB" },
                    {BankType.CITIC,"CITIC" },
                    {BankType.CMB,"CMB" },
                    {BankType.CMBC,"CMBC" },
                    {BankType.COMM,"COMM" },
                    {BankType.HXBANK,"HXB" },
                    {BankType.ICBC,"ICBC" },
                    {BankType.PSBC,"POST" },
                    {BankType.SPDB,"SPDB" }
                };
            }
        }
    }
}
