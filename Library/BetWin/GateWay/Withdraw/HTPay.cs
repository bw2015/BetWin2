using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BW.Common.Sites;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Security;

namespace BW.GateWay.Withdraw
{
    public class HTPay : IWithdraw
    {
        [Description("代付接口")]
        public string Gateway { get; set; } = "https://gateway.huitianpay.com/Payment/BatchTransfer.aspx";

        [Description("查询接口")]
        public string QueryWay { get; set; } = "https://gateway.huitianpay.com/Payment/BatchQuery.aspx";


        [Description("商户号")]
        public string agent_id { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        public HTPay()
        {
        }

        public HTPay(string setting) : base(setting)
        {
        }

        protected override Dictionary<BankType, string> InterfaceCode => new Dictionary<BankType, string>()
        {
            { BankType.ICBC, "1" },
            { BankType.CCB, "2" },
            { BankType.ABC, "3" },
            { BankType.PSBC, "4" },
            { BankType.BOC, "5" },
            { BankType.COMM, "6" },
            { BankType.CMB, "7" },
            { BankType.CEB, "8" },
            { BankType.SPDB, "9" },
            { BankType.HXBANK, "10" },
            { BankType.GDB, "11" },
            { BankType.CITIC, "12" },
            { BankType.CIB, "13" },
            { BankType.CMBC, "14" },
            { BankType.HZCB, "15" },
            { BankType.SHBANK, "16" },
            { BankType.NBBANK, "17" },
            { BankType.SPABANK, "18" }
        };

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"version","2" },
                {"agent_id",this.agent_id },
                {"batch_no",orderId },
                {"key",this.Key }
            };

            string signStr = SP.Studio.Security.MD5.toMD5(dic.OrderBy(t => t.Key).ToQueryString());
            dic.Remove("key");
            dic.Add("sign", signStr.ToLower());
            string result = string.Empty;
            WithdrawStatus status = WithdrawStatus.Error;
            try
            {
                result = NetAgent.UploadData(this.QueryWay, dic.ToQueryString(), Encoding.GetEncoding("gb2312"));
                XElement root = XElement.Parse(result);

                msg = root.Element("ret_msg").Value;
                string code = root.Element("ret_code").Value;
                if (code != "0000") return WithdrawStatus.Error;
                string data = root.Element("detail_data").Value;
                if (data.Contains("^S"))
                {
                    status = WithdrawStatus.Success;
                }
                else if (data.Contains("^F"))
                {
                    status = WithdrawStatus.Return;
                }
                else if (data.Contains("^P"))
                {
                    status = WithdrawStatus.Paymenting;
                };
            }
            catch (Exception ex)
            {
                msg = ex.Message + "\n" + result;
                return status;
            }
            if (status == WithdrawStatus.Error) msg = result;
            return status;

        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                { "version","2" },
                { "agent_id",this.agent_id },
                { "batch_no",this.OrderID },
                { "batch_amt",this.Money.ToString("0.00") },
                { "batch_num","1" },
                { "detail_data",$"{this.OrderID}^{this.GetBankCode(this.BankCode)}^0^{this.CardNo}^{this.Account}^{this.Money.ToString("0.00")}^payment^北京市^北京市^{this.BankCode.GetDescription()}" },
                { "notify_url","https://www.baidu.com" },
                { "ext_param1","payment" },
                { "key",this.Key }
            };
            string signStr = dic.OrderBy(t => t.Key).ToQueryString();
            dic.Remove("key");
            string sign = MD5.Encryp(signStr).ToLower();
            dic.Add("sign", sign);
            string result = string.Empty;
            try
            {
                string data = dic.ToQueryString();
                result = NetAgent.UploadData(this.Gateway, data, Encoding.GetEncoding("gb2312"));
                XElement root = XElement.Parse(result);

                msg = root.Element("ret_msg").Value;
                return root.Element("ret_code").Value == "0000";
            }
            catch (Exception ex)
            {
                msg = result + "\n" + ex.Message;
                return false;
            }
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
