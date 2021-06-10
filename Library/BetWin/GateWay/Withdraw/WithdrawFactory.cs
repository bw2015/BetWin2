using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Withdraw
{
    public static class WithdrawFactory
    {
        /// <summary>
        /// 创建一个出款对象（每次都是新建一个对象）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static IWithdraw CreateWithdraw(WithdrawType type, string setting)
        {
            Type objType = typeof(WithdrawFactory).Assembly.GetType("BW.GateWay.Withdraw." + type);
            if (objType == null) return null;

            return (IWithdraw)Activator.CreateInstance(objType, setting);
        }
    }
}
