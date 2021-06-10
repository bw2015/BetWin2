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
    /// 电子游戏会员反水
    /// </summary>
    public class SlotGame : IPlan
    {
        public SlotGame(XElement root) : base(root) { }
    }
}
