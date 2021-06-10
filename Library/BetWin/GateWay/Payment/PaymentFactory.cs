using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 支付工厂
    /// </summary>
    public class PaymentFactory
    {

        /// <summary>
        /// 创建支付对象
        /// </summary>
        /// <param name="typeName">支付类型。 使用反射找到 IPayment 的实现类</param>
        /// <param name="settingString">支付参数设定</param>
        /// <returns></returns>
        public static IPayment CreatePayment(string typeName, string settingString)
        {
            Type type = typeof(IPayment).Assembly.GetType("BW.GateWay.Payment." + typeName, false);
            if (type == null) return null;
            return (IPayment)Activator.CreateInstance(type, settingString);
        }

        public static IPayment CreatePayment(PaymentType type, string settingString)
        {
            return CreatePayment(type.ToString(), settingString);
        }

    }
}
