using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BW.Common.Sites;
using SP.Studio.Net;
using SP.Studio.Core;
using SP.Studio.Security;
using SP.Studio.Array;
using SP.Studio.Web;
using SP.Studio.Json;
using System.Text.RegularExpressions;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 汇畅代付
    /// </summary>
    public class HuiChang : IWithdraw
    {
        public HuiChang() : base() { }

        public HuiChang(string setting) : base(setting) { }

        /// <summary>
        /// 支持的银行
        /// </summary>
        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                return dic;
            }
        }

        private const string p0_Cmd = "TransPay";

        private string _gateway = "https://gateway.senhuayu.com/controller.action";
        [Description("代付网关")]
        public string Gateway
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


        [Description("商户号")]
        public string p1_MerId { get; set; }

        [Description("商户私钥")]
        public string merchantKey { get; set; }


        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("p0_Cmd", "TransQuery");
            dic.Add("p1_MerId", this.p1_MerId);
            dic.Add("p2_Order", orderId);
            string signStr = string.Join(string.Empty, dic.Select(t => t.Value));
            dic.Add("hmac", MD5.HMACMD5(signStr, this.merchantKey).ToLower());

            string data = dic.ToQueryString();
            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);

            msg = JsonAgent.GetValue<string>(result, "r7_Desc");
            string code = JsonAgent.GetValue<string>(result, "r1_Code");
            WithdrawStatus status = WithdrawStatus.Error;
            switch (code)
            {
                case "0000":
                    status = WithdrawStatus.Success;
                    break;
                case "3003":
                case "3004":
                    status = WithdrawStatus.Paymenting;
                    break;
                case "3002":
                    status = WithdrawStatus.Return;
                    break;
                default:
                    status = WithdrawStatus.Error;
                    break;
            }
            return status;
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            //           $sbOld = $sbOld.$p0_Cmd;
            //#加入商家ID
            //$sbOld = $sbOld.$p1_MerId;
            //$sbOld = $sbOld.$p2_Order;
            //$sbOld = $sbOld.$p3_CardNo;
            //$sbOld = $sbOld.$p4_BankName;
            //$sbOld = $sbOld.$p5_AtName;
            //$sbOld = $sbOld.$p6_Amt;

            dic.Add("p0_Cmd", p0_Cmd);
            dic.Add("p1_MerId", this.p1_MerId);
            dic.Add("p2_Order", this.OrderID);
            dic.Add("p3_CardNo", this.CardNo);
            dic.Add("p4_BankName", this.BankCode.GetDescription());
            dic.Add("p5_AtName", this.Account);
            dic.Add("p6_Amt", this.Money.ToString("0.00"));
            dic.Add("pc_NewType", "PRIVATE");
            string signStr = string.Join(string.Empty, dic.Select(t => t.Value));
            dic.Add("hmac", MD5.HMACMD5(signStr, this.merchantKey).ToLower());
            string data = dic.ToQueryString();
            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);

            BW.Agent.SystemAgent.Instance().AddSystemLog(SiteInfo == null ? 0 : SiteInfo.ID,
             string.Format("代付发起\n类型:{0}\n网关:{1}\n加密前:{2}\n提交内容:{3}\n返回:{4}",
             this.GetType().Name, this.Gateway, signStr, data, result));

            //r0_Cmd=TransPay
            //p1_MerId=CHANG1525773633215
            //r1_Code=0000
            //r7_Desc=
            //r2_TrxId=9164625778846ACC
            //hmac=c67e20fffd3389e03da99b199a54b746

            Regex regex = new Regex(@"r1_Code=(?<Code>\d+)", RegexOptions.Multiline);
            Regex desc = new Regex(@"r7_Desc=(?<Code>.+?)[\n\r]", RegexOptions.Multiline);
            if (!regex.IsMatch(result))
            {
                msg = result;
                return false;
            }

            string code = regex.Match(result).Groups["Code"].Value;
            if (desc.IsMatch(result))
            {
                msg = desc.Match(result).Groups["Code"].Value;
            }
            else
            {
                msg = string.Empty;
            }
            return code == "0000";
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
