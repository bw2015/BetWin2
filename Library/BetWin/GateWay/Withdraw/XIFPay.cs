using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using BW.Agent;
using SP.Studio.Array;
using SP.Studio.Web;
using BW.Common.Sites;
using SP.Studio.Net;
using SP.Studio.Json;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 喜付
    /// </summary>
    public class XIFPay : IWithdraw
    {
        public XIFPay() : base() { }

        public XIFPay(string setting) : base(setting) { }

        private string _gateway = "https://client.xifpay.com";
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
        public string merchantId { get; set; }

        [Description("密钥")]
        public string KEY { get; set; }


        public override WithdrawStatus Query(string orderId, out string msg)
        {
            WithdrawStatus status = WithdrawStatus.Error;
            DateTime datetime = UserAgent.Instance().GetWithdrawOrderDate(orderId);
            if (datetime == DateTime.MinValue)
            {
                datetime = DateTime.Now;
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("batchDate", datetime.ToString("yyyyMMdd"));
            dic.Add("batchNo", orderId);
            dic.Add("batchVersion", "00");
            dic.Add("charset", "utf-8");
            dic.Add("merchantId", this.merchantId);

            string signStr = dic.ToQueryString() + this.KEY;
            dic.Add("signType", "SHA");
            dic.Add("sign", this.Sign(signStr));

            string url = string.Format("{0}/agentPay/v1/batch/{1}-{2}?{3}", this.Gateway, this.merchantId, orderId, dic.ToQueryString());
            try
            {
                string result = NetAgent.DownloadData(url, Encoding.UTF8);

                string respCode = JsonAgent.GetValue<string>(result, "respCode");
                switch (respCode)
                {
                    case "S0001":
                        string batchContent = JsonAgent.GetValue<string>(result, "batchContent");
                        string[] content = batchContent.Split(',');
                        string resultStatus = content[content.Length - 2];
                        switch (resultStatus)
                        {
                            case "成功":
                                status = WithdrawStatus.Success;
                                break;
                            case "失败":
                                status = WithdrawStatus.Return;
                                break;
                            default:
                                status = WithdrawStatus.Paymenting;
                                break;
                        }
                        msg = content.LastOrDefault();
                        break;
                    default:
                        msg = JsonAgent.GetValue<string>(result, "respMessage");
                        break;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return WithdrawStatus.Error;
            }
            return status;

        }

        public override bool Remit(out string msg)
        {
            string bank = this.GetBankCode(BankCode);
            if (string.IsNullOrEmpty(bank))
            {
                msg = string.Format("不支持的银行");
                return false;
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("batchAmount", this.Money.ToString("0.00"));
            dic.Add("batchBiztype", "00000");
            //序号,银行账户,开户名,开户行名称,分行,支行,公/私,金额,币种,省,市,手机号,证件类型,证件号,用户协议号,商户订单号,备注
            dic.Add("batchContent", string.Format("1,{0},{1},{2},上海分行,浦东支行,0,{3},CNY,上海,上海,,,,,{4},备注",
                this.CardNo, this.Account, bank, this.Money.ToString("0.00"), this.OrderID));
            dic.Add("batchCount", "1");
            dic.Add("batchDate", DateTime.Now.ToString("yyyyMMdd"));
            dic.Add("batchNo", this.OrderID);
            dic.Add("batchVersion", "00");
            dic.Add("charset", "UTF-8");
            dic.Add("merchantId", this.merchantId);

            string signStr = dic.ToQueryString() + this.KEY;
            dic.Add("signType", "SHA");
            dic.Add("sign", this.Sign(signStr));

            string url = string.Format("{0}/agentPay/v1/batch/{1}-{2}", this.Gateway, this.merchantId, this.OrderID);
            string data = dic.ToQueryString();

            string result = NetAgent.UploadData(url, data, Encoding.UTF8);

            string code = JsonAgent.GetValue<string>(result, "respCode");
            msg = JsonAgent.GetValue<string>(result, "respMessage");
            return code == "S0001";
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }

        private string Sign(string signStr)
        {
            var sha = System.Security.Cryptography.SHA1.Create();
            var hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(signStr));
            return BitConverter.ToString(hashed).Replace("-", "");
        }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.CMB, "招商银行");
                dic.Add(BankType.ICBC, "中国工商银行");
                dic.Add(BankType.CCB, "中国建设银行");
                dic.Add(BankType.BOC, "中国银行");
                dic.Add(BankType.ABC, "中国农业银行");
                dic.Add(BankType.COMM, "交通银行");
                dic.Add(BankType.SPDB, "上海浦东发展银行");
                dic.Add(BankType.GDB, "广发银行");
                dic.Add(BankType.CITIC, "中信银行");
                dic.Add(BankType.CEB, "中国光大银行");
                dic.Add(BankType.CIB, "兴业银行");
                dic.Add(BankType.SPABANK, "平安银行");
                dic.Add(BankType.CMBC, "中国民生银行");
                dic.Add(BankType.HXBANK, "华夏银行");
                dic.Add(BankType.PSBC, "中国邮政储蓄银行");
                dic.Add(BankType.BJBANK, "北京银行");
                dic.Add(BankType.SHBANK, "上海银行");
                return dic;
            }
        }
    }
}
