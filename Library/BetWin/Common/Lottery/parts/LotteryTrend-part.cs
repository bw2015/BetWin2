using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using SP.Studio.Xml;

namespace BW.Common.Lottery
{
    partial class LotteryTrend : SP.Studio.Model.ModelBase<LotteryTrend>
    {
        protected override string _extendXML
        {
            get
            {
                return this.Result;
            }
            set
            {
                this.Result = value;
            }
        }

        /// <summary>
        ///  把xml数据转化成为JSON数据
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat("{",
                string.Join(",",
                this.XMLObj.Elements().Select(t => string.Format("\"{0}\":\"{1}\"", t.GetAttributeValue("name"), t.GetAttributeValue("value")))),
                "}");
        }
    }
}
