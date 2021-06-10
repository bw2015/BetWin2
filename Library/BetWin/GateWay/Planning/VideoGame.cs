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
    /// 真人游戏本级返点
    /// </summary>
    public class VideoGame : IPlan
    {
        public VideoGame(XElement root) : base(root) { }
    }
}
