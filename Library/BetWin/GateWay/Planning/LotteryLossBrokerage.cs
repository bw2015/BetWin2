using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using SP.Studio.Xml;
using BW.Agent;

namespace BW.GateWay.Planning
{
    /// <summary>
    /// 代理的单日亏损奖励 对应资金类型 LossAgent = 31
    /// </summary>
    public class LotteryLossBrokerage : IPlan
    {
        public LotteryLossBrokerage(XElement root) : base(root) { }
    }
}
