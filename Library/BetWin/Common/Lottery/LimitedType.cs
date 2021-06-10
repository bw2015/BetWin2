using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BW.Common.Lottery
{
    public enum LimitedType
    {
        /// <summary>
        /// 未设定
        /// </summary>
        None,
        [Description("五星直选")]
        X5_Start5 = 101,
        [Description("四星直选")]
        X5_Start4 = 102,
        [Description("三星直选")]
        X5_Start3 = 103,
        [Description("二星直选")]
        X5_Start2 = 104,
        [Description("五星组选")]
        X5_Start5_Group = 105
    }
}
