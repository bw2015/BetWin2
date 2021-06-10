using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using SP.Studio.Xml;

namespace Web.GateWay.App_Code
{
    /// <summary>
    /// 彩种的配置
    /// </summary>
    public struct LotteryConfig
    {
        public LotteryConfig(XElement item)
        {
            this.Value = item.GetAttributeValue("Value", (byte)255);
            this.Number = item.GetAttributeValue("Number");
            this.Length = item.GetAttributeValue("Length", 0);
        }

        /// <summary>
        /// 彩种值
        /// </summary>
        public byte Value;

        /// <summary>
        /// 彩种的号码范围
        /// </summary>
        public string Number;

        /// <summary>
        /// 彩种的长度
        /// </summary>
        public int Length;

        /// <summary>
        /// 获取开奖号码
        /// </summary>
        /// <param name="number"></param>
        /// <returns>返回null表示不符合规则</returns>
        public string GetNumber(string number)
        {
            Regex regex = new Regex(string.Format("({0})", string.Join("|", this.Number.Split(',').Select(t => string.Format("{0}", t)))));
            if (!regex.IsMatch(number)) return null;
            List<string> list = new List<string>();
            foreach (Match match in regex.Matches(number))
            {
                list.Add(match.Value);
            }
            if (list.Count != this.Length) return number;
            return string.Join(",", list);
        }
    }
}