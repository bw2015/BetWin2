using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BW.Common.Sites;

using SP.Studio.Web;
using SP.Studio.Security;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class TTPay : IPayment
    {
        public TTPay() : base() { }

        public TTPay(string setting) : base(setting) { }

        [Description("商户号")]
        public string mch_id { get; set; }

        private string _callback_url = "/handler/payment/TTPay";
        [Description("前台通知")]
        public string callback_url
        {
            get
            {
                return this._callback_url;
            }
            set
            {
                this._callback_url = value;
            }
        }


        private string _notify_url = "/handler/payment/TTPay";
        [Description("后台通知")]
        public string notify_url
        {
            get
            {
                return this._notify_url;
            }
            set
            {
                this._notify_url = value;
            }
        }

        /// <summary>
        /// wx:微信        al:支付宝        qq:qq钱包        jd:京东        wy:网银支付        kj:快捷支付        yl:银联二维码
        /// </summary>
        [Description("接口类型")]
        public string service { get; set; }

        /// <summary>
        /// h5：公众号支付或者其他js支付        pay：扫码或者网关支付        micropay：被扫支付        wap：wap支付        app:app支付
        /// </summary>
        [Description("支付方式")]
        public string way { get; set; }

        private string _format = "xml";
        [Description("数据格式")]
        public string format
        {
            get
            {
                return this._format;
            }
            set
            {
                this._format = value;
            }
        }

        [Description("密钥")]
        public string KEY { get; set; }

        private string _gateway = "http://ipay.ttpay.net.cn/cashier/Home";
        [Description("网关地址")]
        public string GateWay
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

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("mch_id", this.mch_id);
            dic.Add("out_trade_no", this.OrderID);
            dic.Add("body", this.Name);
            dic.Add("callback_url", this.GetUrl(this.callback_url));
            dic.Add("notify_url", this.GetUrl(this.notify_url));
            dic.Add("total_fee", this.Money.ToString("0.00"));
            dic.Add("service", this.service);
            dic.Add("way", this.way);
            dic.Add("format", this.format);
            dic.Add("mch_create_ip", IPAgent.IP);
            dic.Add("goods_tag", this.BankValue);
            string signStr = string.Join(string.Empty, "mch_id+out_trade_no+callback_url+notify_url+total_fee+service+way+format".Split('+').Select(t => dic[t])) + this.KEY;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());
            if (this.format == "xml")
            {
                this.BuildForm(dic, this.GateWay, "GET");
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            //?device_info=&attach=&mch_id=51016&time_end=00010101000000&transtypeid=516417&out_trade_no=20180606094506503&transaction_id=7777201806060912903598&ordernumber=18060609450657424877461&way=pay&total_fee=10.00&service=qq&result_code=0&sign=549EB0BF449F39FC5256427F0290077B
            string signStr = string.Join(string.Empty,
                "mch_id+device_info+attach+time_end+out_trade_no+ordernumber+transtypeid+transaction_id+total_fee+service+way+result_code".Split('+').Select(t => WebAgent.GetParam(t))) + this.KEY;

            string sign = WebAgent.GetParam("sign");
            if (MD5.toMD5(signStr).ToLower() == sign.ToLower())
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("ordernumber");
            money = WebAgent.GetParam("total_fee", decimal.Zero);
            return WebAgent.GetParam("out_trade_no");
        }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.service != "wy") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.CMB, "CMB");
                dic.Add(BankType.COMM, "COMM");
                dic.Add(BankType.CMBC, "CMBC");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.HZCB, "HCCB");
                dic.Add(BankType.PSBC, "PSBC");
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.HXBANK, "HXB");
                dic.Add(BankType.GCB, "GZCB");
                dic.Add(BankType.NBBANK, "NBCB");
                dic.Add(BankType.NJCB, "NJCB");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.GDB, "GDB");
                dic.Add(BankType.CEB, "CEB");
                dic.Add(BankType.CITIC, "CITIC");
                return dic;
            }
        }
    }
}
