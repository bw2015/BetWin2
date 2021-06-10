using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Xml.Linq;

using BW.Common.Sites;

using SP.Studio.Net;
using SP.Studio.Core;
using SP.Studio.Security;
using SP.Studio.Text;
using SP.Studio.Xml;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 国付宝代付接口
    /// </summary>
    public class GoPay : IWithdraw
    {
        public GoPay() : base() { }

        public GoPay(string setting) : base(setting) { }

        private string _gateway = "https://gateway.gopay.com.cn/Trans/WebClientAction.do";

        [Description("网关")]
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

        [Description("企业ID")]
        public string customerId { get; set; }

        [Description("国付宝账号")]
        public string payAccId { get; set; }

        private string _merURL = "/handler/payment/SUCCESS";
        [Description("通知地址")]
        public string merURL
        {
            get
            {
                return this._merURL;
            }
            set
            {
                this._merURL = value;
            }
        }

        [Description("密钥")]
        public string VerficationCode { get; set; }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                foreach (BankType type in Enum.GetValues(typeof(BankType)))
                {
                    if (!dic.ContainsKey(type))
                        dic.Add(type, type.GetDescription());
                }
                return dic;
            }
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            // 版本号
            dic.Add("Version", "1.1");
            // 交易代码
            dic.Add("TranCode", "BQ01");
            // 报文字符集 1:GBK,2:UTF-8
            dic.Add("Charset", "2");
            // 签名方式 1:MD5,2:SHA 3:RSA
            dic.Add("SignType", "1");
            // 商户ID
            dic.Add("MerId", this.customerId);
            // 商户账户ID
            dic.Add("MerAcctId", this.payAccId);
            // 查询的商户订单号
            dic.Add("QryMerOrderId", orderId);
            // 查询的国付宝订单号
            dic.Add("QryGopayOrderId", string.Empty);
            // 批次号
            dic.Add("batchNum", string.Empty);
            // 查询的交易代码
            dic.Add("QryTranCode", "22");
            // 订单国付宝交易时间开始
            dic.Add("GopayTxnTmStart", DateTime.Now.AddDays(-3).ToString("yyyyMMddHHmmss"));
            // 订单国付宝交易时间截至
            dic.Add("GopayTxnTmEnd", DateTime.Now.ToString("yyyyMMddHHmmss"));
            // 查询交易状态 A-全部，S-成功，P-进行中，F-失败
            dic.Add("TxnStat", "A");
            // 页码
            dic.Add("PageNum", "1");
            // 密钥
            dic.Add("VerficationCode", this.VerficationCode);

            string signStr = string.Join(string.Empty, new string[] { "Version", "TranCode", "MerId", "MerAcctId", "QryGopayOrderId", "QryTranCode", "GopayTxnTmStart", "GopayTxnTmEnd", "PageNum", "VerficationCode" }.Select(t => string.Format("{0}=[{1}]", t, dic.ContainsKey(t) ? dic[t] : string.Empty)));
            dic.Add("SignValue", MD5.toMD5(signStr).ToLower());
            dic.Remove("VerficationCode");
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
            WithdrawStatus status = WithdrawStatus.Error;
            msg = HttpUtility.HtmlEncode(result);
            try
            {
                XElement root = XElement.Parse(result);
                string bizstatus = root.GetValue("BizInf[0]/TxnSet[0]/TxnInf[0]/BizStsCd[0]", string.Empty);
                msg = root.GetValue("BizInf[0]/TxnSet[0]/TxnInf[0]/BizStsDesc[0]", string.Empty);
                switch (bizstatus)
                {
                    case "S":
                        status = WithdrawStatus.Success;
                        break;
                    case "P":
                        status = WithdrawStatus.Paymenting;
                        break;
                    case "F":
                        status = WithdrawStatus.Return;
                        break;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message + "\n" + msg;
            }

            return status;
        }

        public override bool Remit(out string msg)
        {
            string gopayServerTime = NetAgent.DownloadData("https://gateway.gopay.com.cn/time.do", Encoding.UTF8);
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic.Add("version", "1.2");
            // 交易代码
            dic.Add("tranCode", "4025");
            // 报文字符集 1:GBK,2:UTF-8
            dic.Add("charset", "1");
            // 签名方式 1:MD5,2:SHA 3:RSA
            dic.Add("signType", "1");
            // 商户ID
            dic.Add("customerId", this.customerId);
            // 商户账户ID
            dic.Add("payAcctId", this.payAccId);
            // 查询的商户订单号
            dic.Add("merOrderNum", this.OrderID);
            dic.Add("merURL", this.merURL);
            dic.Add("tranAmt", this.Money.ToString("0.00"));
            dic.Add("recvBankAcctName", this.Account);
            dic.Add("recvBankName", this.BankCode.GetDescription());
            dic.Add("recvBankProvince", "北京");
            dic.Add("recvBankCity", "北京");
            dic.Add("recvBankBranchName", this.BankCode.GetDescription());
            dic.Add("recvBankAcctNum", this.CardNo);
            dic.Add("tranDateTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("description", "网关单笔付款");
            dic.Add("approve", "2");
            dic.Add("settlementToday", "1");
            dic.Add("VerficationCode", this.VerficationCode);
            dic.Add("gopayServerTime", gopayServerTime);
            dic.Add("corpPersonFlag", "2");

            String signStr = string.Join(string.Empty, new string[]{
            "version","tranCode","customerId","merOrderNum","tranAmt","feeAmt","totalAmount","merURL","recvBankAcctNum",
            "tranDateTime","orderId","respCode","payAcctId","approve","VerficationCode","gopayServerTime" }
            .Select(t => string.Format("{0}=[{1}]", t, dic.ContainsKey(t) ? dic[t] : string.Empty)));
            dic.Add("signValue", MD5.toMD5(signStr).ToLower());
            dic.Remove("VerficationCode");

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));

            string result = NetAgent.UploadData(this.Gateway, data, Encoding.GetEncoding("GBK"));
            bool success = false;
            msg = HttpUtility.HtmlEncode(result);
            try
            {
                XElement root = XElement.Parse(result);
                int resCode = root.GetValue("respCode", 0);
                string msgExt = root.GetValue("msgExt", string.Empty);
                if (!string.IsNullOrEmpty(msgExt)) msg = msgExt;
                switch (resCode)
                {
                    case 0:
                        XElement errCode = root.Element("errCode");
                        XElement errMessage = root.Element("errMessage");
                        if (errCode != null && errMessage != null)
                        {
                            msg = string.Format("{0}:{1}", errCode, errMessage);
                        }
                        break;
                    case 2:
                    case 7:
                        success = true;
                        break;
                }


            }
            catch (Exception ex)
            {
                msg = ex.Message + "\n" + msg;
                success = false;
            }
            return success;
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }


    }
}
