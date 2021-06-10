using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BW.GateWay.Payment
{
    public class BeiZi : IPayment
    {
        public BeiZi() : base() { }

        public BeiZi(string setting) : base(setting) { }

        [Description("商户号")]
        public string merIdmerId { get; set; }

        [Description("应用ID")]
        public string appId { get; set; }

        [Description("通道")]
        public string channeltype { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            throw new NotImplementedException();
        }

        public override void GoGateway()
        {
            throw new NotImplementedException();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            throw new NotImplementedException();
        }
    }
}
