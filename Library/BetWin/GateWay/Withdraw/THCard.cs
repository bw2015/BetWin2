using BW.Agent;
using BW.Common.Sites;
using Newtonsoft.Json.Linq;
using SP.Studio.Core;
using SP.Studio.Json;
using SP.Studio.Net;
using SP.Studio.Security;
using SP.Studio.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace BW.GateWay.Withdraw
{
    public sealed class THCard : IWithdraw
    {
        public THCard() : base() { }

        public THCard(string setting) : base(setting) { }


        /// <summary>
        /// 付款接口
        /// </summary>
        [Description("付款接口")]
        public string PAYMENT { get; set; }

        /// <summary>
        /// 查询接口
        /// </summary>
        [Description("查询接口")]
        public string QUERY { get; set; }

        /// <summary>
        /// 余额接口
        /// </summary>
        [Description("余额接口")]
        public string BALANCE { get; set; }

        [Description("商户号")]
        public string merchant_code { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        private string _version = "1.0";
        [Description("版本号")]
        public string Version { get { return this._version; } set { this._version = value; } }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> _banlCode = new Dictionary<BankType, string>();
                _banlCode.Add(BankType.ABC, "ABC");
                _banlCode.Add(BankType.BOC, "BOC");
                _banlCode.Add(BankType.COMM, "BOCOM");
                _banlCode.Add(BankType.CCB, "CCB");
                _banlCode.Add(BankType.CSCB, "CSCB");
                _banlCode.Add(BankType.CMB, "CMBC");
                _banlCode.Add(BankType.CMBC, "CMBCS");
                _banlCode.Add(BankType.CEB, "CEBBANK");
                _banlCode.Add(BankType.CITIC, "ECITIC");
                _banlCode.Add(BankType.CIB, "CIB");
                _banlCode.Add(BankType.GDB, "CGB");
                _banlCode.Add(BankType.ICBC, "ICBC");
                _banlCode.Add(BankType.HXBANK, "HXB");
                _banlCode.Add(BankType.HSBANK, "HSB");
                _banlCode.Add(BankType.SPDB, "SPDB");
                _banlCode.Add(BankType.SPABANK, "PINGAN");
                _banlCode.Add(BankType.PSBC, "PSBC");
                _banlCode.Add(BankType.ZJNX, "ZJRCC");
                return _banlCode;
            }
        }

        private const string LOGTYPE = "Withdraw_THK";

        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override bool Remit(out string msg)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("input_charset", "UTF-8");
            dic.Add("merchant_code", this.merchant_code);
            dic.Add("amount", this.Money.ToString("0.00"));

            switch (this.Version)
            {
                case "2.0":
                    dic.Add("transid", this.OrderID);
                    dic.Add("bitch_no", this.OrderID);
                    dic.Add("currentDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("bank_name", this.GetBankCode(this.BankCode));
                    dic.Add("account_name", this.Account);
                    dic.Add("account_number", this.CardNo);
                    break;
                case "3.0":
                    dic.Clear();
                    dic.Add("merchant_code", this.merchant_code);
                    dic.Add("order_amount", this.Money.ToString("0.00"));
                    dic.Add("trade_no", this.OrderID);
                    dic.Add("trade_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("bank_code", this.GetBankCode(this.BankCode));
                    dic.Add("account_name", this.Account);
                    dic.Add("account_number", this.CardNo);
                    dic.Add("notify_url", "http://t1.betwin.ph/handler/payment/SUCCESS");
                    break;
                default:
                    dic.Add("merchant_order", this.OrderID);
                    dic.Add("bank_code", this.GetBankCode(this.BankCode));
                    dic.Add("bank_card_no", this.CardNo);
                    dic.Add("bank_account", this.Account);
                    break;
            }
            string sign = string.Empty;
            string paramString;
            switch (this.Version)
            {
                case "2.0":
                    paramString = string.Join("&", dic.Select(t => t.Key + "=" + t.Value)) + "&key=" + this.Key;
                    sign = MD5.toMD5(MD5.toMD5(sign.ToUpper()).ToLower()).ToLower();
                    break;
                case "3.0":
                    paramString = string.Join("&", dic.OrderBy(t => t.Key).Select(t => t.Key + "=" + t.Value)) + "&key=" + this.Key;
                    sign = MD5.toMD5(paramString).ToLower();
                    break;
                default:
                    paramString = string.Join("&", dic.Select(t => t.Key + "=" + t.Value)) + "&key=" + this.Key;
                    sign = MD5.toMD5(paramString).ToLower();
                    break;
            }
            dic.Add("sign", sign);

            string data = string.Join("&", dic.Select(t => t.Key + "=" + t.Value));
            msg = NetAgent.UploadData(PAYMENT, data, Encoding.UTF8);

            bool result = false;
            bool isSuccess;
            switch (this.Version)
            {
                case "2.0":
                    isSuccess = JsonAgent.GetValue<bool>(msg, "is_success");
                    if (!isSuccess)
                    {
                        msg = JsonAgent.GetValue<string>(msg, "errror_msg");
                    }
                    else
                    {
                        result = true;
                        UserAgent.Instance().UpdateWithdrawOrderSystemID(int.Parse(this.OrderID), JsonAgent.GetValue<string>(msg, "order_id"));
                    }
                    break;
                case "3.0":
                    JObject info = JObject.Parse(msg);

                    if (!info["state"].Value<bool>() || info["return_data"] == null)
                    {
                        msg = info["msg"].Value<string>();
                    }
                    else
                    {
                        switch (((JArray)info["return_data"])[0]["bank_status"].Value<int>())
                        {
                            case 1:
                            case 8:
                            case 16:
                            case 32:
                            case 128:
                                result = true;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    result = msg.Contains("success");
                    break;
            }
            return result;
        }

        /// <summary>
        /// 异步请求
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 查询订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override WithdrawStatus Query(string orderId, out string msg)
        {
            msg = string.Empty;
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("input_charset", "UTF-8");
            dic.Add("merchant_code", this.merchant_code);
            string paramString, data;
            string result = string.Empty;
            WithdrawStatus status = WithdrawStatus.Error;
            bool isSuccess;
            switch (this.Version)
            {
                case "2.0":
                    dic.Add("currentDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("order_id", UserAgent.Instance().GetWithdrawOrderSystemID(int.Parse(orderId)));
                    paramString = string.Join("&", dic.Select(t => t.Key + "=" + t.Value)) + "&key=" + this.Key;
                    dic.Add("sign", MD5.toMD5(MD5.toMD5(MD5.toMD5(paramString)).ToLower()).ToLower());
                    data = string.Join("&", dic.Select(t => t.Key + "=" + t.Value));
                    result = NetAgent.UploadData(QUERY, data, Encoding.UTF8);
                    isSuccess = JsonAgent.GetValue<bool>(result, "is_success");
                    if (!isSuccess)
                    {
                        msg = JsonAgent.GetValue<string>(result, "errror_msg");
                    }
                    else
                    {
                        switch (JsonAgent.GetValue<int>(result, "bank_status"))
                        {
                            case 0:
                                status = WithdrawStatus.Error;
                                break;
                            case 1:
                                status = WithdrawStatus.Paymenting;
                                break;
                            case 2:
                                status = WithdrawStatus.Success;
                                break;
                            case 3:
                                status = WithdrawStatus.Return;
                                break;
                        }
                        msg = status.GetDescription();
                    }
                    break;
                case "3.0":
                    dic.Clear();
                    dic.Add("merchant_code", this.merchant_code);
                    dic.Add("now_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("order_no", orderId);
                    dic.Add("sign", MD5.toMD5(string.Join("&", dic.OrderBy(t => t.Key).Select(t => $"{t.Key}={t.Value}")) + "&key=" + this.Key).ToLower());
                    data = string.Join("&", dic.Select(t => t.Key + "=" + t.Value));
                    result = NetAgent.UploadData(QUERY, data, Encoding.UTF8);
                    JObject info = JObject.Parse(result);
                    if (!info["state"].Value<bool>())
                    {
                        msg = info["msg"].Value<string>();
                        status = WithdrawStatus.Error;
                    }
                    else
                    {
                        switch (info["data"]["trade_status"].Value<int>())
                        {
                            case 1:
                            case 8:
                            case 16:
                            case 32:
                                status = WithdrawStatus.Paymenting;
                                break;
                            case 128:
                                status = WithdrawStatus.Success;
                                break;
                            case 130:
                                status = WithdrawStatus.Return;
                                break;
                        }
                    }
                    break;
                default:
                    try
                    {

                        dic.Add("merchant_order", orderId);
                        paramString = string.Join("&", dic.Select(t => t.Key + "=" + t.Value)) + "&key=" + this.Key;
                        dic.Add("sign", SP.Studio.Security.MD5.toMD5(paramString));
                        data = string.Join("&", dic.Select(t => t.Key + "=" + t.Value));
                        result = NetAgent.UploadData(QUERY, data, Encoding.UTF8);
                        XElement response = XElement.Parse(result).Element("response");
                        string queryStatus = response.GetValue("is_success");
                        if (queryStatus == "FALSE")
                        {
                            msg = response.GetValue("error_msg");
                            return status;
                        }
                        msg = response.GetValue("remit_status_desc");
                        switch (response.GetValue("remit_status", 0))
                        {
                            case 1:
                            case 2:
                                status = WithdrawStatus.Paymenting;
                                break;
                            case 3:
                                status = WithdrawStatus.Success;
                                break;
                            case 4:
                                status = WithdrawStatus.Return;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message + "\n" + result;
                    }
                    break;
            }

            return status;
        }
    }
}
