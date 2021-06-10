using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.IM.Common
{
    /// <summary>
    /// 群类型
    /// </summary>
    public enum GroupType : byte
    {
        None,
        /// <summary>
        /// 北京赛车
        /// </summary>
        PK10 = 8,
        /// <summary>
        /// 香港赛马
        /// </summary>
        HKSM = 32,
        /// <summary>
        /// 重庆时时彩
        /// </summary>
        ChungKing = 1,
        /// <summary>
        /// 幸运飞艇
        /// </summary>
        Boat = 40,
        /// <summary>
        /// 新疆时时彩
        /// </summary>
        Sinkiang = 9
    }
}
