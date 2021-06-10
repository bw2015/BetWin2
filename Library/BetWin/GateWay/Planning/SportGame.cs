using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BW.GateWay.Planning
{
    /// <summary>
    /// 体育游戏会员返水
    /// </summary>
    public class SportGame : IPlan
    {
        public SportGame(XElement root) : base(root) { }
    }
}
