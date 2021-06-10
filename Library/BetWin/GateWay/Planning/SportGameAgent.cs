using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BW.GateWay.Planning
{
    /// <summary>
    /// 体育游戏代理返水
    /// </summary>
    public class SportGameAgent : IPlan
    {
        public SportGameAgent(XElement root) : base(root) { }
    }
}
