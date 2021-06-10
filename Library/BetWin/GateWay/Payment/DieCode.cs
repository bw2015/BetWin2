using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Payment
{
    public class DieCode : IPayment
    {
        public DieCode()
        {
        }

        public DieCode(string settingString) : base(settingString)
        {
        }

        [Description("商户ID")]
        public string account_id { get; set; }

        [Description("网页类型")]
        public string content_type { get; set; } = "text";

        /// <summary>
        /// 目前通道：alipayRed_auto（支付宝红包）、wechat_auto（商户版微信）、alipay_auto（商户版支付宝）、bank_auto（商户版支付宝转账）、service_auto（服务版微信/支付宝）
        /// </summary>
        [Description("通道")]
        public string thoroughfare { get; set; }

        /// <summary>
        /// 支付类型，该参数在服务版下有效（service_auto），其他可为空参数，微信：1，支付宝：2 ，支付宝转银行卡：3
        /// </summary>
        [Description("支付类型")]
        public string type { get; set; }

        /// <summary>
        /// 2：开启轮训，1：进入单通道模式
        /// </summary>
        [Description("轮训")]
        public string robin { get; set; }

        /// <summary>
        /// 设备KEY，在商户版列表里面Important参数下的DEVICE Key一项，如果该请求为轮训模式，则本参数无效，本参数为单通道模式
        /// </summary>
        [Description("设备KEY")]
        public string keyId { get; set; }

        [Description("异步通知地址")]
        public string callback_url { get; set; } = "/handler/payment/DieCode";


        [Description("同步通知地址")]
        public string success_url { get; set; } = "/handler/payment/DieCode";

        [Description("错误通知地址")]
        public string error_url { get; set; } = "/handler/payment/DieCode";

        [Description("密钥")]
        public string key { get; set; }

        [Description("网关")]
        public string Gateway { get; set; } = "http://www.diecode.cn/gateway/index/checkpoint.do";

        public override string ShowCallback()
        {
            return "success";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.QF("amount", decimal.Zero);
            systemId = WebAgent.QF("trade_no");
            return WebAgent.QF("out_trade_no");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
           {
               {"account_id",this.account_id },
               {"content_type",this.content_type },
               {"thoroughfare",this.thoroughfare },
               {"out_trade_no",this.OrderID },
               {"robin",this.robin },
               {"callback_url",this.GetUrl(this.callback_url) },
               {"success_url",this.GetUrl(this.success_url) },
               {"error_url",this.GetUrl(this.error_url) },
               {"amount",this.Money.ToString("0.00") },
               {"type",this.type },
               {"keyId",this.keyId }
           };
            dic.Add("sign", this.sign(dic["amount"], dic["out_trade_no"]));
            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.QF("status") != "success") return false;

            string account_key = WebAgent.QF("account_key");
            string sign = WebAgent.QF("sign");

            //account_name : haoyuan      
            //pay_time : 1550594518      
            //status : success      
            //amount : 1.00      
            //out_trade_no : 20190220004126918      
            //trade_no : 93782019022055995450      
            //fees : 0.003      
            //sign : e1dd169107a2b504674cd43aa119a605      
            //callback_time : 1550594518      
            //type : 1      
            //account_key : 97A0B44F724F27

            //第一步，检测商户KEY是否一致
            if (!string.IsNullOrEmpty(account_key) && account_key != this.key) return false;
            //第二步，验证签名是否一致
            //if (this.sign(WebAgent.QF("amount"), WebAgent.QF("out_trade_no")) != sign) return false;
            callback.Invoke();
            return true;
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="money"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private string sign(string money, string orderId)
        {
            string data = MD5.toMD5(money + orderId).ToLower();

            byte[] key = new byte[256];
            byte[] box = new byte[256];
            int pwd_length = this.key.Length;
            int data_length = data.Length;


            for (int i = 0; i < 256; i++)
            {
                key[i] = (byte)this.key[i % pwd_length];
                box[i] = (byte)i;
            }

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + box[i] + key[i]) % 256;
                byte tmp = box[i];
                box[i] = box[j];
                box[j] = tmp;
            }

            int a = 0;
            j = 0;
            // string cipher = string.Empty;
            List<byte> datatmp = new List<byte>();
            for (int i = 0; i < data_length; i++)
            {
                a = (a + 1) % 256;
                j = (j + box[a]) % 256;

                byte tmp = box[a];
                box[a] = box[j];
                box[j] = tmp;

                byte k = box[((box[a] + box[j]) % 256)];
                //cipher += Convert.ToChar((byte)(((byte)(data[i])) ^ k));
                datatmp.Add((byte)((byte)data[i] ^ k));
            }

            return MD5.toMD5(datatmp.ToArray()).ToLower();
        }

    }
}
