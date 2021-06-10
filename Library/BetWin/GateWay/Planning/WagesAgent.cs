using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using SP.Studio.Array;
using SP.Studio.Xml;
namespace BW.GateWay.Planning
{
    /// <summary>
    /// 代理日工资
    /// </summary>
    public class WagesAgent : IPlan
    {
        public WagesAgent(XElement root)
            : base()
        {
            XElement setting = this.Setting;
            foreach (XElement item in root.Elements())
            {
                string name = item.Name.ToString();
                if (this.Value.ContainsKey(name))
                {
                    this.Value[name] = item.GetValue(null, this.Value[name]);
                }
            }
        }
    }
}
