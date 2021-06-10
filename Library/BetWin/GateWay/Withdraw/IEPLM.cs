using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;

using SP.Studio.Web;
using SP.Studio.Core;
using BW.Common.Sites;
using SP.Studio.Net;
using SP.Studio.Security;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Withdraw
{
    public class IEPLM : IWithdraw
    {
        public IEPLM() : base() { }

        public IEPLM(string setting) : base(setting) { }

        /// <summary>
        /// 接口版本号
        /// </summary>
        private const string version = "v1";

        /// <summary>
        /// 渠道编号
        /// </summary>
        private const string channelNo = "04";

        /// <summary>
        /// 交易码
        /// </summary>
        private const string tranCode = "1001";

        /// <summary>
        /// 交易币种
        /// </summary>
        private const string currency = "RMB";

        [Description("商户号")]
        public string merchantNo { get; set; }

        private string _gateway = "https://cashier.ielpm.com/paygate/v1/dfpay";
        [Description("网关地址")]
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

        /// <summary>
        /// pfx 文件
        /// </summary>
        [Description("私钥")]
        public string privateCert { get; set; }

        [Description("私钥密码")]
        public string certPass { get; set; }

        /// <summary>
        /// cer文件
        /// </summary>
        [Description("公钥")]
        public string publicCert { get; set; }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> code = new Dictionary<BankType, string>();
                code.Add(BankType.ICBC, "102100099996");
                code.Add(BankType.ABC, "103100000026");
                code.Add(BankType.BOC, "104100000004");
                code.Add(BankType.CCB, "105100000017");
                code.Add(BankType.COMM, "301290000007");
                code.Add(BankType.CEB, "303100000006");
                code.Add(BankType.HXBANK, "304100040000");
                code.Add(BankType.CMBC, "305100000013");
                code.Add(BankType.GDB, "306581000003");
                code.Add(BankType.SPABANK, "307584007998");
                code.Add(BankType.CMB, "308584000013");
                code.Add(BankType.CIB, "309391000011");
                code.Add(BankType.BJBANK, "313100000013");
                code.Add(BankType.GCB, "313581003284");
                code.Add(BankType.PSBC, "403100000004");
                code.Add(BankType.SPDB, "310290000013");
                code.Add(BankType.SHBANK, "325290000012");
                return code;
            }
        }

        public override bool Remit(out string msg)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>(new IELPMComparer());
            dic.Add("merchantNo", this.merchantNo);
            dic.Add("version", version);
            dic.Add("channelNo", channelNo);
            dic.Add("tranCode", tranCode);
            dic.Add("tranFlow", this.OrderID);
            dic.Add("tranDate", DateTime.Now.ToString("yyyyMMdd"));
            dic.Add("tranTime", DateTime.Now.ToString("HHmmss"));
            dic.Add("accNo", this.EncryptData(this.CardNo));
            dic.Add("accName", this.EncryptData(this.Account));
            dic.Add("bankAgentId", this.GetBankCode(this.BankCode));
            dic.Add("currency", "RMB");
            dic.Add("bankName", this.BankCode.GetDescription());
            dic.Add("amount", ((int)(this.Money * 100)).ToString());
            dic.Add("remark", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("YUL1", Guid.NewGuid().ToString("N").Substring(0, 8));
            dic.Add("YUL2", Guid.NewGuid().ToString("N").Substring(0, 8));
            dic.Add("YUL3", Guid.NewGuid().ToString("N").Substring(0, 8));
            dic.Add("ext1", Guid.NewGuid().ToString("N").Substring(0, 8));
            dic.Add("ext2", Guid.NewGuid().ToString("N").Substring(0, 8));
            dic.Add("sign", this.Sign(dic));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));

            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);

            NameValueCollection request = HttpUtility.ParseQueryString(result);

            msg = request["rtnMsg"];
            if (request["rtnCode"] == "0000")
            {
                return true;
            }

            return false;
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>(new IELPMComparer());
            dic.Add("merchantNo", this.merchantNo);
            dic.Add("version", version);
            dic.Add("channelNo", channelNo);
            dic.Add("tranCode", "1004");
            dic.Add("tranFlow", orderId);
            dic.Add("tranDate", DateTime.Now.ToString("yyyyMMdd"));
            dic.Add("tranTime", DateTime.Now.ToString("HHmmss"));
            dic.Add("oriTranFlow", orderId);
            dic.Add("oriTranDate", DateTime.Now.ToString("yyyyMMdd"));
            dic.Add("sign", this.Sign(dic));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));

            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
            NameValueCollection request = HttpUtility.ParseQueryString(result);

            if (request["oriRtnCode"] != "0000")
            {
                msg = request["oriRtnMsg"];
                return WithdrawStatus.Error;
            }
            msg = request["rtnMsg"];
            WithdrawStatus status = WithdrawStatus.Error;

            //oriRtnCode=0006&oriRtnMsg=原交易不存在
            switch (request["rtnCode"])
            {
                case "0000":
                    status = WithdrawStatus.Success;
                    break;
                case "0001":
                    status = WithdrawStatus.Return;
                    break;
                case "0002":
                    status = WithdrawStatus.Paymenting;
                    break;
            }
            return status;
        }

        #region ============ 工具方法  ===============

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private string Sign(SortedDictionary<string, string> dic)
        {
            string stringData = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            //stringData = "YUL1=" + dic["YUL1"] + "&" + stringData;

            byte[] byteSign = this.SignBySoft(GetSignProviderFromPfx(), Encoding.UTF8.GetBytes(stringData));
            return Convert.ToBase64String(byteSign);
        }

        public bool Validate(SortedDictionary<string, string> data, Encoding encoding)
        {
            string signValue = data["sign"];
            byte[] signByte = Convert.FromBase64String(signValue);
            data.Remove("sign");
            string stringData = string.Join("&", data.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            RSACryptoServiceProvider provider = this.GetValidateProviderFromPath();
            return null != provider && this.ValidateBySoft(provider, signByte, encoding.GetBytes(stringData));
        }

        /// <summary>
        /// 私钥
        /// </summary>
        /// <returns></returns>
        public RSACryptoServiceProvider GetSignProviderFromPfx()
        {
            X509Certificate2 pc = new X509Certificate2(Convert.FromBase64String(this.privateCert), this.certPass);
            return (RSACryptoServiceProvider)pc.PrivateKey;
        }

        /// <summary>
        /// 公钥
        /// </summary>
        /// <returns></returns>
        public RSACryptoServiceProvider GetValidateProviderFromPath()
        {
            X509Certificate2 pc = new X509Certificate2(Convert.FromBase64String(this.publicCert));
            return (RSACryptoServiceProvider)pc.PublicKey.Key;
        }

        public bool ValidateBySoft(RSACryptoServiceProvider provider, byte[] base64DecodingSignStr, byte[] srcByte)
        {
            HashAlgorithm hashalg = new SHA1CryptoServiceProvider();
            return provider.VerifyData(srcByte, hashalg, base64DecodingSignStr);
        }

        public byte[] SignBySoft(RSACryptoServiceProvider provider, byte[] data)
        {
            byte[] res = null;
            try
            {
                HashAlgorithm hashalg = new SHA1CryptoServiceProvider();
                res = provider.SignData(data, hashalg);
            }
            catch (Exception e)
            {
                throw e;
            }
            byte[] result;
            if (null == res)
            {
                result = null;
            }
            else
            {
                result = res;
            }
            return result;
        }

        /// <summary>
        /// 加密敏感数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string EncryptData(string data)
        {
            Encoding encoding = Encoding.UTF8;
            string result;
            if (string.IsNullOrEmpty(data))
            {
                result = "";
            }
            else
            {
                result = Convert.ToBase64String(encryptedData(encoding.GetBytes(data)));
            }
            return result;
        }

        public byte[] encryptedData(byte[] encData)
        {
            byte[] result;
            try
            {
                RSACryptoServiceProvider p = this.GetValidateProviderFromPath();
                byte[] enBytes = p.Encrypt(encData, false);
                result = enBytes;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        private class IELPMComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                char[] v = x.ToCharArray();
                char[] v2 = y.ToCharArray();
                int len = v.Length;
                int len2 = v2.Length;
                int lim = (len > len2) ? len2 : len;
                int result;
                for (int i = 0; i < lim; i++)
                {
                    char c = v[i];
                    char c2 = v2[i];
                    if (c != c2)
                    {
                        result = (int)(c - c2);
                        return result;
                    }
                }
                result = len - len2;
                return result;
            }
        }
    }
}
