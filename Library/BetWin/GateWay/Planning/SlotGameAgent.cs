using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using SP.Studio.Xml;
using BW.Agent;

namespace BW.GateWay.Planning
{
    public class SlotGameAgent : IPlan
    {
        public SlotGameAgent(XElement root)
            : base(root)
        {
        }
    }
}
