using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using SP.Studio.Model;
using SP.Studio.Web;
using SP.Studio.Net;
using SP.Studio.Xml;

namespace Web.GateWay.boqu
{
    /// <summary>
    /// 通汇卡的自动出款接口
    /// </summary>
    public class autopay : IHttpHandler
    {
        public const string Gateway = "https://pay.41.cn";
        /// <summary>
        /// 付款接口
        /// </summary>
        private string PAYMENT
        {
            get
            {
                return string.Concat(Gateway, "/remit");
            }
        }

        /// <summary>
        /// 查询接口
        /// </summary>
        private string QUERY
        {
            get
            {
                return string.Concat(Gateway, "/remit/query");
            }
        }

        Dictionary<string, string> data = new Dictionary<string, string>();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            data["12481996"] = "c2d260a3d2644b298496e35667526e8a";
            data["17118371"] = "d04e9b971d2a49f4a7ef338e6e5cc969";
            string payid = WebAgent.GetParam("payid");
            if (!data.ContainsKey(payid))
            {
                context.Response.Write(false, "出款接口错误");
            }
            string orderId = WebAgent.GetParam("orderid");
            if (string.IsNullOrEmpty(orderId))
            {
                context.Response.Write(false, "订单号错误");
            }

            if (WebAgent.GetParam("ac") == "query")
            {
                context.Response.Write(true, this.Query(orderId, payid));
                return;
            }


            string info = WebAgent.GetParam("info");
            Regex regex = new Regex(@"真实姓名：(?<Name>.+?) 银行：(?<Bank>.+?)-.*? 卡号：(?<Card>\d+)");
            if (!regex.IsMatch(info))
            {
                context.Response.Write(false, "账户信息格式错误");
            }

            string name = regex.Match(info).Groups["Name"].Value;
            string bank = this.GetBank(regex.Match(info).Groups["Bank"].Value);
            string card = regex.Match(info).Groups["Card"].Value;

            if (string.IsNullOrEmpty(bank))
            {
                context.Response.Write(false, "银行类型错误");
            }

            decimal money = WebAgent.GetParam("money", decimal.Zero);
            if (money == decimal.Zero)
            {
                context.Response.Write(false, "金额错误");
            }

            string msg;
            if (this.Remit(payid, orderId, card, name, bank, money, out msg))
            {
                context.Response.Write(true, "出款成功");
            }
            else
            {
                context.Response.Write(false, msg);
            }
        }

        /// <summary>
        /// 付款
        /// </summary>
        public bool Remit(string payId, string orderId, string card, string name, string bank, decimal money, out string msg)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("input_charset", "UTF-8");
            dic.Add("merchant_code", payId);
            dic.Add("merchant_order", orderId);
            dic.Add("bank_card_no", card);
            dic.Add("bank_account", name);
            dic.Add("bank_code", bank);
            dic.Add("amount", money.ToString("0.00"));
            string key = this.data[payId];

            string paramString = string.Join("&", dic.Select(t => t.Key + "=" + t.Value)) + "&key=" + key;
            dic.Add("sign", SP.Studio.Security.MD5.toMD5(paramString));

            string data = string.Join("&", dic.Select(t => t.Key + "=" + t.Value));

            msg = NetAgent.UploadData(PAYMENT, data, Encoding.UTF8);

            if (!msg.Contains("success"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 状态查询
        /// </summary>
        /// <param name="context"></param>
        /// <param name="orderId"></param>
        /// <param name="payId"></param>
        public string Query(string orderId, string payId)
        {
            string msg = string.Empty;
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("input_charset", "UTF-8");
            dic.Add("merchant_code", payId);
            dic.Add("merchant_order", orderId);
            string paramString = string.Join("&", dic.Select(t => t.Key + "=" + t.Value)) + "&key=" + this.data[payId];
            dic.Add("sign", SP.Studio.Security.MD5.toMD5(paramString));
            string data = string.Join("&", dic.Select(t => t.Key + "=" + t.Value));
            string result = NetAgent.UploadData(QUERY, data, Encoding.UTF8);
            string status = null;
            try
            {
                XElement response = XElement.Parse(result).Element("response");
                string queryStatus = response.GetValue("is_success");
                if (queryStatus == "FALSE")
                {
                    status = response.GetValue("error_msg");
                    return status;
                }

                msg = response.GetValue("remit_status_desc");
                switch (response.GetValue("remit_status", 0))
                {
                    case 1:
                    case 2:
                        status = string.Format("正在支付({0})", msg);
                        break;
                    case 3:
                        status = string.Format("已付款({0})", msg);
                        break;
                    case 4:
                        status = string.Format("银行退回({0})", msg);
                        break;
                }
            }
            catch (Exception ex)
            {
                status = ex.Message + "\n" + result;
            }

            return status;
        }

        private string GetBank(string value)
        {
            switch (value)
            {
                case "中国农业银行":
                    value = "ABC";
                    break;
                case "中国工商银行":
                    value = "ICBC";
                    break;
                case "中国建设银行":
                    value = "CCB";
                    break;
                case "中国招商银行":
                    value = "CMBC";
                    break;
                default:
                    value = null;
                    break;
            }
            return value;
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