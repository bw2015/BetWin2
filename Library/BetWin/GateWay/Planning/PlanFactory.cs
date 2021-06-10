using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BW.GateWay.Planning
{
    public class PlanFactory
    {
        /// <summary>
        /// 创建一个活动设置对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static IPlan CreatePlanSetting(PlanType type, string setting)
        {
            string name = string.Format("BW.GateWay.Planning.{0}", type);
            Type planType = typeof(PlanFactory).Assembly.GetType(name);
            if (planType == null) return null;
            XElement root = new XElement("root");
            if (!string.IsNullOrEmpty(setting)) root = XElement.Parse(setting);
            return (IPlan)Activator.CreateInstance(planType, root);
        }
    }
}
