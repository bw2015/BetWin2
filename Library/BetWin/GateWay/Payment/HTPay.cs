using BW.Common.Sites;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class HTPay : IPayment
    {
        public HTPay()
        {
        }

        public HTPay(string settingString) : base(settingString)
        {
        }

        /// <summary>
        /// https://gateway.huitianpay.com/Pay/KDBank.aspx
        /// </summary>
        [Description("网关")]
        public string Gateway { get; set; } = "https://gateway.huitianpay.com/Pay/KDBank.aspx";

        /// <summary>
        /// 商户编号
        /// </summary>
        [Description("商户编号")]
        public string P_UserId { get; set; }

        [Description("充值渠道")]
        public string P_ChannelId { get; set; }

        [Description("异步通知")]
        public string P_Result_URL { get; set; } = "/handler/payment/HTPay";

        [Description("同步通知")]
        public string P_Notify_URL { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        [Description("密钥")]
        public string KEY { get; set; }

        public override string ShowCallback()
        {
            return "ErrCode=0";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("P_FaceValue", decimal.Zero);
            systemId = WebAgent.GetParam("P_OrderId");
            return systemId;
        }

        public override void GoGateway()
        {
            string P_PostKey = SP.Studio.Security.MD5.toMD5(this.P_UserId + "|" + this.OrderID + "|" + "" + "|" + "" + "|" + this.Money.ToString("0.00") + "|" + this.P_ChannelId + "|" + this.KEY).ToLower();            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"P_UserId",this.P_UserId },
                {"P_OrderId",this.OrderID },
                {"P_FaceValue",this.Money.ToString("0.00") },
                {"P_ChannelId",this.P_ChannelId },
                {"P_Price",this.Money.ToString("0.00") },
                {"P_Result_URL",this.GetUrl(P_Result_URL) },
                {"P_Notify_URL",this.GetUrl(P_Notify_URL) },
                {"P_PostKey",P_PostKey }
            };
            if (this.BankCode != null) dic.Add("P_Description", this.BankValue);            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("P_ErrCode") != "0") return false;
            callback.Invoke();
            return true;
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.P_ChannelId != "1") return null;
                return new Dictionary<BankType, string>()
                {
                    { BankType.ICBC,"10001" },
                    { BankType.ABC,"10002" },
                    { BankType.CMB,"10003" },
                    { BankType.BOC,"10004" },
                    { BankType.CCB,"10005" },
                    { BankType.CMBC,"10006" },
                    { BankType.CITIC,"10007" },
                    { BankType.COMM,"10008" },
                    { BankType.CIB,"10009" },
                    { BankType.CEB,"10010" },
                    { BankType.SPABANK,"10014" },
                    { BankType.PSBC,"10012" },
                    { BankType.BJBANK,"10013" },
                    { BankType.SPDB,"10015" },
                    { BankType.GDB,"10016" }
                };
            }
        }
    }
}
