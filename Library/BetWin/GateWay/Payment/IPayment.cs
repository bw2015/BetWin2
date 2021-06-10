using BW.Agent;
using BW.Common.Sites;
using BW.Framework;
using BW.Handler;
using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Net;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 支付的接口
    /// </summary>
    public abstract class IPayment : SettingBase
    {
        public IPayment() { }

        public IPayment(string settingString)
            : base(settingString)
        {
            foreach (var property in this.GetType().GetProperties().Where(t => t.HasAttribute(typeof(UrlPropertyAttribute))))
            {
                var url = (string)property.GetValue(this, null) ?? string.Empty;
                if (!url.ToLower().StartsWith("http"))
                {
                    property.SetValue(this,
                        string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme,
                        HttpContext.Current.Request.Url.Authority, url), null);
                }
            }
        }

        /// <summary>
        /// 传递错误信息的KEY
        /// </summary>
        public const string ERROR_KEY = "PAYMENT_ERRORKEY";

        /// <summary>
        /// 平台类型
        /// </summary>
        public const string PLATFORM = "PAYMENT_PLATFORM";

        /// <summary>
        /// 自定义的回调地址
        /// </summary>
        public const string PAYMENTNOTIFYURL = "PAYMENT_NOTIFYURL";

        /// <summary>
        /// 内部使用的网关域名
        /// </summary>
        internal const string _GATEWAY = "_gateway";

        /// <summary>
        /// 支付类型
        /// </summary>
        internal const string _GATETYPE = "_gatetype";

        protected virtual HttpContext context
        {
            get
            {
                return HttpContext.Current;
            }
        }

        /// <summary>
        /// 当前的站点
        /// </summary>
        protected Site SiteInfo
        {
            get
            {
                if (this.context == null) return null;
                return (Site)this.context.Items[BetModule.SITEINFO];
            }
        }

        /// <summary>
        /// 获取自定义的回调地址
        /// </summary>
        protected string NotifyURL(string notifyurl)
        {
            if (HttpContext.Current.Items.Contains(PAYMENTNOTIFYURL))
            {
                return (string)HttpContext.Current.Items[PAYMENTNOTIFYURL];
            }
            return notifyurl;
        }

        /// <summary>
        /// 获取账号
        /// </summary>
        /// <returns></returns>
        public virtual string GetAccount()
        {
            return string.Empty;
        }

        /// <summary>
        /// 检查密钥是否正确
        /// </summary>
        /// <param name="account"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool CheckKey(string account, string key)
        {
            return false;
        }

        /// <summary>
        /// 把相对路径改为绝对路径
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected string GetUrl(string url, string scheme = null)
        {
            if (url.StartsWith("http")) return url;
            HttpContext context = HttpContext.Current;
            if (string.IsNullOrEmpty(scheme))
            {
                scheme = context.Request.UrlReferrer == null ? context.Request.Url.Scheme : context.Request.UrlReferrer.Scheme;
            }
            string host = context.Request.UrlReferrer == null ? context.Request.Url.Authority : context.Request.UrlReferrer.Authority;
            return string.Format("{0}://{1}{2}", scheme, host, url);
        }


        /// <summary>
        /// 订单编号（唯一）
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// 订单名称（商品名称）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 订单、商品描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 可被选择的金额类型
        /// </summary>
        protected string _moneyValues;

        /// <summary>
        /// 跳转到网关
        /// </summary>
        public abstract void GoGateway();


        /// <summary>
        /// 验证回调页面是否正确
        /// </summary>
        public abstract bool Verify(VerifyCallBack callback);

        /// <summary>
        /// 是否支持微信远程读取二维码
        /// </summary>
        public virtual bool IsWechat()
        {
            return WebAgent.QF("wechat", 0) == 1;
        }

        /// <summary>
        /// 确认发货的接口
        /// </summary>
        /// <returns></returns>
        public virtual bool SendGoods()
        {
            return true;
        }

        /// <summary>
        /// 可被选择的金额列表
        /// </summary>
        /// <returns></returns>
        public virtual decimal[] GetMoneyValue()
        {
            if (string.IsNullOrEmpty(this._moneyValues)) return null;
            decimal[] values = WebAgent.GetArray<decimal>(this._moneyValues);
            if (values.Length == 0) return null;
            return values;
        }

        /// <summary>
        /// 从返回的数据中获取订单号
        /// </summary>
        /// <param name="money">实际支付的金额</param>
        /// <param name="systemId">网关的系统订单号</param>
        /// <returns>平台的订单号</returns>
        public abstract string GetTradeNo(out decimal money, out string systemId);

        /// <summary>
        /// 创建隐藏域表单
        /// </summary>
        protected string CreateInput(string name, object value)
        {
            return string.Format("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", name, value);
        }

        /// <summary>
        /// 客户端选择的银行类型（转化成为字典）
        /// </summary>
        protected virtual string BankValue
        {
            get
            {
                if (this.BankCode == null) return string.Empty;
                string _bankType = this.Bank == null ? string.Empty : WebAgent.GetParam("BankType");
                if (string.IsNullOrEmpty(_bankType)) return string.Empty;
                BankType type = _bankType.ToEnum<BankType>();
                if (this.BankCode.ContainsKey(type)) return this.BankCode[type];
                return _bankType;
            }
        }

        /// <summary>
        /// 支持的银行类型，用于在客户界面选择银行
        /// </summary>
        public virtual BankType[] Bank
        {
            get
            {
                if (this.BankCode == null) return null;
                return this.BankCode.Select(t => t.Key).ToArray();
            }
        }

        /// <summary>
        /// 把公共的银行类型转化成为接口所支持的类型
        /// </summary>
        protected virtual Dictionary<BankType, string> BankCode { get { return null; } }

        /// <summary>
        /// 充值成功服务端通知所要求显示的内容
        /// </summary>
        /// <returns>为null表示不采用</returns>
        public virtual string ShowCallback()
        {
            return null;
        }

        /// <summary>
        /// 获取跳转网关地址
        /// </summary>
        /// <param name="shop">跳转网关域名</param>
        /// <param name="gateway">真实网关</param>
        /// <returns></returns>
        protected virtual string GetGateway(string shop, string gateway)
        {
            if (string.IsNullOrEmpty(shop)) return gateway;
            return string.Format("http://{0}/handler/payment/Redirect", shop);
        }

        /// <summary>
        /// 获取远程网关返回的结果并且存入日志库
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data">如果为NULL则是GET请求</param>
        /// <returns></returns>
        protected virtual string GetGatewayResult(string url, string data = null, Encoding encoding = null)
        {
            string result = string.Empty;
            if (encoding == null) encoding = Encoding.UTF8;
            if (string.IsNullOrEmpty(data))
            {
                result = NetAgent.DownloadData(url, encoding);
            }
            else
            {
                result = NetAgent.UploadData(url, data, encoding);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Method:{0}", string.IsNullOrEmpty(data) ? "GET" : "POST")
                .AppendLine()
                .AppendFormat("URL:{0}", url)
                .AppendLine()
                .AppendFormat("DATA:{0}", data)
                .AppendLine()
                .AppendFormat("Result:{0}", result);

            SystemAgent.Instance().AddSystemLog(SiteInfo == null ? 0 : SiteInfo.ID, sb.ToString());
            return result;
        }


        #region ============ 二维码的支付页面 ===========

        /// <summary>
        /// 创建一个微信二维码的页面
        /// </summary>
        protected virtual void CreateWXCode(string code)
        {
            this.CreateQRCode(code, PaymentHandler.REDIRECT_WX);
        }

        /// <summary>
        /// 创建一个QQ支付的二维码页面
        /// </summary>
        /// <param name="code"></param>
        protected virtual void CreateQQCode(string code)
        {
            this.CreateQRCode(code, PaymentHandler.REDIRECT_QQ);
        }

        /// <summary>
        /// 创建一个支付宝二维码的页面
        /// </summary>
        protected virtual void CreateAliCode(string code)
        {
            this.CreateQRCode(code, PaymentHandler.REDIRECT_ALIPAY);
        }

        /// <summary>
        /// 创建一个二维码页面
        /// </summary>
        /// <param name="code">二维码地址</param>
        /// <param name="gateway">网关类型 wx|alipay|qq</param>
        /// <param name="data">需要被扩展的数据</param>
        protected virtual void CreateQRCode(string code, string gateway, Dictionary<string, string> data = null)
        {
            if (this.IsWechat())
            {
                this.CreateWechatPayment(!string.IsNullOrEmpty(code), code);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<form action=\"/handler/payment/Redirect\" method=\"post\" id=\"{0}\">", this.GetType().Name)
                    .Append(this.CreateInput("_orderid", this.OrderID))
                    .Append(this.CreateInput("_money", this.Money.ToString("n")))
                    .Append(this.CreateInput("_code", code))
                    .Append(this.CreateInput("_gatetype", gateway));
                if (data != null)
                {
                    foreach (KeyValuePair<string, string> item in data)
                    {
                        sb.Append(this.CreateInput(item.Key, item.Value));
                    }
                }
                sb.Append("</form>");
                sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

                this.context.Response.Write(sb);
                this.context.Response.End();
            }
        }

        /// <summary>
        /// 通用的二维码页面
        /// </summary>
        /// <param name="code"></param>
        protected virtual void CreateQRCode(string code, Dictionary<string, string> data = null)
        {
            this.CreateQRCode(code, PaymentHandler.REDIRECT_QRCODE, data);
        }


        #endregion

        #region =========== 表单提交  =============

        /// <summary>
        /// 创建一个form表单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="gateway"></param>
        /// <param name="method"></param>
        protected virtual void BuildForm(IDictionary<string, string> data, string gateway, string method = "POST")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<form action=\"{0}\" method=\"{1}\" id=\"{2}\">", gateway, method, this.GetType().Name)
                .Append(string.Join(string.Empty, data.Select(t => this.CreateInput(t.Key, t.Value))))
                .Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 创建一个跳转URL 的表单
        /// </summary>
        /// <param name="gateway"></param>
        /// <param name="method"></param>
        protected virtual void BuildForm(string gateway, string method = "GET")
        {
            StringBuilder sb = new StringBuilder();
            Uri uri = new Uri(gateway);
            if (gateway.Contains("?")) gateway.Substring(0, gateway.IndexOf('?') - 1);
            sb.AppendFormat("<form action=\"{0}\" method=\"{1}\" id=\"{2}\">", gateway, method, this.GetType().Name);
            NameValueCollection request = HttpUtility.ParseQueryString(uri.Query ?? string.Empty);
            foreach (string key in request.AllKeys)
            {
                sb.Append(this.CreateInput(key, request[key]));
            }
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }


        #endregion


        /// <summary>
        /// 获取微信投注渠道的充值备注信息
        /// </summary>
        /// <returns></returns>
        protected virtual string GetMark()
        {
            return string.Empty;
        }

        /// <summary>
        /// 创建一个用于微信投注的二维码结果返回页面
        /// </summary>
        /// <param name="success">是否加载成功</param>
        /// <param name="data">二维码路径或者错误信息</param>
        protected virtual void CreateWechatPayment(bool success, string data)
        {
            HttpContext context = HttpContext.Current;
            context.Response.Write(success, "", new
            {
                data = data,
                mark = this.GetMark()
            });
        }
    }

    /// <summary>
    /// 用于被回调的委托
    /// </summary>
    public delegate void VerifyCallBack();

    /// <summary>
    /// 写入系统日志的回调方法
    /// </summary>
    /// <param name="message">日志信息</param>
    public delegate void SystemLog(string message);

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum PaymentType : byte
    {
        [Description("线下支付")]
        None,
        [Description("扫码转账")]
        AlipayAccount,
        [Description("易宝支付")]
        YeePay,
        [Description("汇潮支付")]
        Ecpss,
        [Description("环迅支付")]
        IPS,
        [Description("环迅支付（新）")]
        IPS2,
        [Description("口袋支付")]
        KDPay,
        [Description("MO宝支付")]
        MOPay,
        [Description("汇通卡支付")]
        HTCard,
        [Description("新贝支付")]
        xBeiPay,
        [Description("久付")]
        Pay9,
        [Description("银宝商务")]
        PayV9,
        [Description("智付")]
        DinPay,
        [Description("爱扬网络")]
        Admin523,
        [Description("优宝支付")]
        YBPay,
        [Description("讯付通")]
        H8Pay,
        [Description("GBOTONG")]
        GBOTONG,
        [Description("泽圣支付")]
        ZSAGE,
        [Description("西部支付")]
        RXlicai,
        [Description("PPL支付")]
        PPL,
        [Description("金海蜇")]
        JHZ,
        [Description("易势支付")]
        IEPLM,
        [Description("威富通")]
        WFT,
        [Description("新U支付")]
        UZhiFu,
        [Description("爱益支付")]
        IYI,
        [Description("MY18")]
        MY18,
        [Description("智刷支付")]
        ZhiShuaPay,
        [Description("乐付")]
        LeFu,
        [Description("傲视支付")]
        ASO,
        [Description("香蕉支付")]
        XJPay,
        [Description("萝卜支付")]
        LuoBo,
        [Description("国付宝")]
        GoPay,
        [Description("新MSD0")]
        MSD0,
        [Description("OKPay")]
        OKPay,
        [Description("多宝")]
        DUOBAO,
        [Description("便利付")]
        BianLiPay,
        [Description("汇合支付")]
        HuiHePay,
        [Description("易百易支付")]
        YBYPay,
        [Description("A付")]
        APay,
        [Description("顺手付")]
        Pay18,
        [Description("好想付")]
        HXF,
        [Description("和支付")]
        HePay,
        [Description("ACPay")]
        ACPay,
        [Description("金阳支付")]
        JinYang,
        /// <summary>
        /// 海富盛通
        /// </summary>
        [Description("海富盛通")]
        HaiFuPay,
        /// <summary>
        /// 喜付
        /// </summary>
        [Description("喜付")]
        XIFPay,
        /// <summary>
        /// 桃宝支付 1004 红河娱乐提供
        /// </summary>
        [Description("桃宝支付")]
        TAOBAO,
        /// <summary>
        /// 金贝支付 1009 泛亚电竞
        /// </summary>
        [Description("金贝支付")]
        JBPay,
        /// <summary>
        /// 新畅汇 1004 红河娱乐
        /// </summary>
        [Description("新畅汇")]
        HuiPay,
        [Description("DD支付宝")]
        hhnnm,
        /// <summary>
        /// 100
        /// </summary>
        [Description("安易支付")]
        AnYi,
        /// <summary>
        /// 简易支付
        /// </summary>
        [Description("简易支付")]
        TTPay,
        [Description("优+支付")]
        UPlus,
        /// <summary>
        /// 1001 博卡娱乐
        /// </summary>
        [Description("TF-Pay")]
        TFPAY,
        /// <summary>
        /// 么么支付
        /// </summary>
        [Description("么么支付")]
        MMPay,
        /// <summary>
        /// 必付宝
        /// </summary>
        [Description("必付宝")]
        DPay,
        /// <summary>
        /// 阿甘支付
        /// </summary>
        [Description("阿甘支付")]
        AGPay,
        /// <summary>
        /// 中诺支付
        /// </summary>
        [Description("中诺支付")]
        ZhongNuo,
        /// <summary>
        /// 鼎付
        /// </summary>
        [Description("鼎付支付")]
        DFPay,
        /// <summary>
        /// 美易付
        /// </summary>
        [Description("美易付")]
        MeitPay,
        /// <summary>
        /// 杯子金福
        /// </summary>
        [Description("杯子金服")]
        BetZi,
        /// <summary>
        /// 真好付
        /// </summary>
        [Description("真好付")]
        ZHF,
        [Description("启航通")]
        QHT,
        [Description("新若水")]
        XRS,
        [Description("威力付")]
        WLPay,
        [Description("快捷付")]
        KJF,
        [Description("EPayGG")]
        EPayGG,
        [Description("蝶码支付")]
        DieCode,
        [Description("码控")]
        F03,
        [Description("汇天付")]
        HTPay,
        [Description("MKPay")]
        MKPay,
        /// <summary>
        /// 万能支付
        /// </summary>
        [Description("万能付")]
        AllPay = 255
    }
}
