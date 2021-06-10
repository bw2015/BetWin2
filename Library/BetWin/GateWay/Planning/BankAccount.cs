using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BW.GateWay.Planning
{
    /// <summary>
    /// 绑定银行卡赠送
    /// </summary>
    public class BankAccount : IPlan
    {
        public BankAccount(XElement root)
            : base(root)
        {

        }
    }
}
