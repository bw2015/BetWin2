using BW.Common.Sites;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Withdraw
{
    public class WLPay : IWithdraw
    {
        public WLPay()
        {
        }

        public WLPay(string setting) : base(setting)
        {
        }

        [Description("商户号")]
        public string merId { get; set; }

        [Description("通知地址")]
        public string notifyUrl { get; set; } = "http://betwin.ph/payment/callback/WLPay";

        [Description("密钥")]
        public string Key { get; set; }

        [Description("代付网关")]
        public string Gateway { get; set; } = "http://paygate.chongshengwei.cn:9090/powerpay-gateway-onl/txn";


        [Description("查询网关")]
        public string QueryGateway { get; set; } = "http://paygate.chongshengwei.cn:9090/powerpay-gateway-onl/txn";
        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                return new Dictionary<BankType, string>()
                {
                    { BankType.ICBC,"01020000" },
                    { BankType.ABC,"01030000" },
                    { BankType.BOC,"01040000" },
                    { BankType.CCB,"01050000" },
                    { BankType.COMM,"03010000" },
                    { BankType.CITIC,"03020000" },
                    { BankType.CEB,"03030000" },
                    { BankType.HXBANK,"03040000" },
                    { BankType.CMBC,"03050000" },
                    { BankType.GDB,"03060000" },
                    { BankType.SPABANK,"03070000" },
                    { BankType.CMB,"03080000" },
                    { BankType.CIB,"03090000" },
                    { BankType.SPDB,"03100000" },
                    { BankType.EGBANK,"03110000" },
                    { BankType.SHBANK,"03130000" },
                    { BankType.BJBANK,"03131000" },
                    { BankType.PSBC,"04030000" },
                    { BankType.GCB,"04135810" }
                };
            }
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"txnType","00" },
                {"txnSubType","50" },
                {"secpVer","icp3-1.1" },
                {"secpMode","perm" },
                {"macKeyId",this.merId },
                {"merId",this.merId },
                {"orderId",orderId },
                {"orderDate",DateTime.Now.ToString("yyyyMMdd") },
                {"timeStamp",DateTime.Now.ToString("yyyyMMddHHmmss") }
            };
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&k=" + this.Key;
            dic.Add("mac", MD5.toMD5(signStr).ToLower());

            string result = NetAgent.UploadData(this.QueryGateway, dic.ToQueryString(), Encoding.UTF8);
            WithdrawStatus status = WithdrawStatus.Error;

            try
            {
                JObject info = (JObject)JsonConvert.DeserializeObject(result);
                string respCode = info["respCode"].Value<string>();
                msg = info["respMsg"].Value<string>();
                string txnStatus = info["txnStatus"].Value<string>();
                if (respCode != "0000") { return WithdrawStatus.Error; }
                switch (txnStatus)
                {
                    case "01":
                        status = WithdrawStatus.Paymenting;
                        break;
                    case "10":
                        status = WithdrawStatus.Success;
                        break;
                    case "20":
                        status = WithdrawStatus.Return;
                        break;
                    default:
                        status = WithdrawStatus.Error;
                        break;
                }
            }
            catch
            {
                msg = result;
            }
            return status;
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
           {
               {"txnType","52" },
               {"txnSubType","10" },
               {"secpVer","icp3-1.1" },
               {"secpMode","perm" },
               {"macKeyId",this.merId },
               {"orderDate",DateTime.Now.ToString("yyyyMMdd") },
               {"orderTime",DateTime.Now.ToString("HHmmss") },
               {"merId",this.merId },
               {"orderId",this.OrderID },
               {"txnAmt",((int)(this.Money*100M)).ToString() },
               {"currencyCode","156" },
               {"accName",this.Account },
               {"accNum",this.CardNo },
               {"bankNum",this.GetBankCode(this.BankCode) ?? "99999999" },
               {"bankName",this.BankCode.GetDescription() },
               {"phoneNumber","13800138000" },
               {"notifyUrl",this.notifyUrl },
               {"timeStamp",DateTime.Now.ToString("yyyyMMddHHmmss") }
           };
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&k=" + this.Key;
            dic.Add("mac", MD5.toMD5(signStr).ToLower());

            string result = NetAgent.UploadData(this.Gateway, dic.ToQueryString(), Encoding.UTF8);

            try
            {
                JObject info = (JObject)JsonConvert.DeserializeObject(result);
                string respCode = info["respCode"].Value<string>();
                msg = info["respMsg"].Value<string>();
                string txnStatus = info["txnStatus"].Value<string>();
                if (respCode != "0000") { return false; }
                return txnStatus == "01" || txnStatus == "10";
            }
            catch
            {
                msg = result;
                return false;
            }
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
