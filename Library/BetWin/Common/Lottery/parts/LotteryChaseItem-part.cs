using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SP.Studio.Array;

namespace BW.Common.Lottery
{
    partial class LotteryChaseItem
    {
        public LotteryChaseItem() { }

        public LotteryChaseItem(Hashtable ht)
        {
            this.Index = ht.GetValue("index", string.Empty);
            this.Times = ht.GetValue("times", 0);
        }

        /// <summary>
        /// 追号内容
        /// </summary>
        public string Content { get; set; }
    }
}
