using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BW.GateWay.Planning
{
    /// <summary>
    /// 第三方游戏工资
    /// </summary>
    public class GameWages : IPlan
    {
        public GameWages(XElement root) : base(root) { }
    }
}
