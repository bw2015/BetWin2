using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Common.Sites
{
    partial class QuotaSetting
    {
        /// <summary>
        /// 已使用的数量
        /// </summary>
        public int Count { get; internal set; }

        /// <summary>
        /// 剩余数量
        /// </summary>
        public int Overage
        {
            get
            {
                return this.Number - this.Count;
            }
        }
    }
}
