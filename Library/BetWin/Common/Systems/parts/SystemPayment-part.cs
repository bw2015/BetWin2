using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.GateWay.Payment;

namespace BW.Common.Systems
{
    /// <summary>
    /// 支付接口
    /// </summary>
    partial class SystemPayment
    {
        public IPayment GetPaymentInfo()
        {
            return PaymentFactory.CreatePayment(this.Type, this.SettingString);
        }

        public void GoGateway(int orderId, decimal money)
        {
            IPayment payment = this.GetPaymentInfo();
            payment.Money = money;
            payment.OrderID = orderId.ToString();
            payment.Name = "BILL";
            payment.GoGateway();
        }
    }
}
