using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Reflection;
using System.Net;
using System.Diagnostics;
using System.Web.Caching;

using BW.Common.Users;

using BW.Agent;
using BW.Common.Sites;
using BW.PageBase;
using BW.Framework;
using SP.Studio.Web;
using SP.Studio.Core;

using BW.GateWay.Payment;

using SP.Studio.Array;
using SP.Studio.Model;
using SP.Studio.ErrorLog;

namespace BW.Handler
{
    /// <summary>
    /// 充值回调
    /// </summary>
    public partial class PaymentHandler : HandlerBase
    {
        /// <summary>
        /// 跳转链接
        /// </summary>
        private const string REDIRECT = "Redirect";

        /// <summary>
        /// 返回页面的成功提示
        /// </summary>
        private const string SUCCESS = "SUCCESS";

        public override void ProcessRequest(HttpContext context)
        {
            Regex regex = new Regex(@"/payment/(?<Type>\w+)", RegexOptions.IgnoreCase);
            string url = context.Request.Path;
            if (!regex.IsMatch(url))
            {
                Utils.ShowError(context, HttpStatusCode.NotFound, url);
            }
            string typeName = regex.Match(url).Groups["Type"].Value;

            if (!Enum.IsDefined(typeof(PaymentType), typeName))
            {
                switch (typeName)
                {
                    case REDIRECT:
                        this.ShowRedirect(context);
                        break;
                    case "SUCCESS":
                        this.Success(context);
                        break;
                    default:
                        Utils.ShowError(context, HttpStatusCode.NotFound, string.Format("Unsupported payment type {0}", typeName));
                        break;
                }

                return;
            }

            SystemAgent.Instance().AddSystemLog(SiteInfo.ID, string.Format("充值到账信息\n\r{0}", string.Join("\n", ErrorAgent.GetLog(context))));
            PaymentType type = typeName.ToEnum<PaymentType>();
            this.CheckKey(context, type);

            IPayment payment = PaymentFactory.CreatePayment(type, "");
            decimal money = 0.00M;
            string systemId = string.Empty;
            string tradeNo = payment.GetTradeNo(out money, out systemId);
            if (string.IsNullOrEmpty(tradeNo))
            {
                context.Response.Write(false, "充值订单号错误");
            }
            if (string.IsNullOrEmpty(systemId))
            {
                context.Response.Write(false, "网关订单号错误");
            }
            if (money <= decimal.Zero)
            {
                context.Response.Write(false, "金额错误");
            }

            lock (typeof(PaymentHandler))
            {
                RechargeOrder order;
                switch (type)
                {
                    // 主动创建订单号到账通知类型
                    case PaymentType.AlipayAccount:
                        #region =========== 支付宝类型 ============

                        int payId = SiteAgent.Instance().GetPaymentID(type, WebAgent.GetParam("account"));
                        if (payId == 0)
                        {
                            context.Response.Write(false, string.Format("收款帐号{0}不存在", WebAgent.GetParam("account")));
                        }
                        payment = SiteAgent.Instance().GetPaymentSettingInfo(payId).PaymentObject;

                        order = UserAgent.Instance().GetRechargeOrderInfo(systemId);
                        if (order != null)
                        {
                            context.Response.Write(true, "该订单已成功充值");
                        }
                        string userName = tradeNo.Contains('|') ? tradeNo.Split('|')[0] : null;
                        if (string.IsNullOrEmpty(userName))
                        {
                            context.Response.Write(false, "用户名不存在");
                        }
                        int userId = UserAgent.Instance().GetUserID(userName);
                        if (userId == 0)
                        {
                            context.Response.Write(false, string.Format("用户名：{0}不存在", userName));
                        }

                        string remark = WebAgent.GetParam("remark");
                        if (string.IsNullOrEmpty(remark)) remark = type.GetDescription();

                        long orderId = UserAgent.Instance().CreateRechargeOrder(userId, payId, money, remark);
                        if (!payment.Verify(() =>
                        {
                            if (UserAgent.Instance().ConfirmRechargeOrderInfo(orderId, money, systemId))
                            {
                                context.Response.Write(true, string.Format("SUCCESS 充值成功。订单ID：{0}，到帐金额：{1}元", systemId, money.ToString("c")));
                            }
                            else
                            {
                                context.Response.Write(false, UserAgent.Instance().Message());
                            }
                        }))
                        {
                            context.Response.StatusCode = 403;
                            context.Response.Write(false, "密钥验证错误");
                        }

                        #endregion
                        break;
                    // 第三方网关充值到帐
                    default:
                        if (!WebAgent.IsType<long>(tradeNo))
                        {
                            context.Response.Write(false, "订单号格式错误");
                        }
                        order = UserAgent.Instance().GetRechargeOrderInfo(long.Parse(tradeNo));
                        if (order == null)
                        {
                            context.Response.Write(false, "订单号错误", new { OrderID = tradeNo });
                        }

                        PaymentSetting setting = SiteAgent.Instance().GetPaymentSettingInfo(order.PayID);

                        if (order.IsPayment)
                        {
                            string showCallback;
                            if (!string.IsNullOrEmpty(showCallback = setting.PaymentObject.ShowCallback()))
                            {
                                context.Response.Write(showCallback);
                                return;
                            }
                            context.Response.Write(true, "该订单已成功充值");
                        }

                        if (!setting.PaymentObject.Verify(() =>
                        {
                            if (UserAgent.Instance().ConfirmRechargeOrderInfo(order.ID, money, systemId))
                            {
                                if (!string.IsNullOrEmpty(setting.PaymentObject.ShowCallback()))
                                {
                                    context.Response.Write(setting.PaymentObject.ShowCallback());
                                }
                                else
                                {
                                    context.Response.Write(string.Format("SUCCESS 充值成功。订单ID：{0}，到帐金额：{1}元", order.ID, money.ToString("c")));
                                }
                            }
                            else
                            {
                                context.Response.Write(false, UserAgent.Instance().Message());
                            }
                        }))
                        {
                            context.Response.StatusCode = 403;
                            context.Response.Write("密钥验证错误");
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// 成功的提示页面
        /// </summary>
        /// <param name="context"></param>
        private void Success(HttpContext context)
        {
            string html = BW.Resources.Res.Payment_Success;
            context.Response.ContentType = "text/html";
            context.Response.Write(html);
        }

        /// <summary>
        /// 提交支付页面
        /// </summary>
        /// <param name="context"></param>
        private void Pay(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.Write(this.ReplaceContent(context, BW.Resources.Res.Payment_Submit));

        }

        /// <summary>
        /// 自动到帐类型 验证密钥是否正确
        /// </summary>
        private void CheckKey(HttpContext context, PaymentType type)
        {
            switch (type)
            {
                case PaymentType.AlipayAccount:
                    if (string.IsNullOrEmpty(WebAgent.GetParam("key"))) return;

                    int payId = SiteAgent.Instance().GetPaymentID(type, WebAgent.GetParam("Alipay"));
                    if (payId == 0)
                    {
                        context.Response.Write(false, string.Format("收款帐号{0}不存在", WebAgent.GetParam("Alipay")));
                    }
                    if (SiteAgent.Instance().GetPaymentSettingInfo(payId).PaymentObject.CheckKey(WebAgent.GetParam("Alipay"), WebAgent.GetParam("key")))
                    {
                        context.Response.Write(true, "验证成功");
                    }
                    else
                    {
                        context.Response.Write(false, "密钥错误");
                    }
                    break;
            }
        }


        /// <summary>
        /// 替换POST内容
        /// </summary>
        private string ReplaceContent(HttpContext context, string content, params string[] args)
        {
            if (args.Length % 2 == 0)
            {
                for (int i = 0; i < args.Length; i += 2)
                {
                    content = content.Replace("${" + args[i] + "}", args[i + 1]);
                }
            }
            foreach (string key in context.Request.Form.AllKeys)
            {
                content = content.Replace("${" + key + "}", context.Request.Form[key]);
            }
            return content.Replace("${QUERY}", BW.Resources.Res.Payment_Query);
        }
    }
}
