using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Data.Mapping
{
    /// <summary>
    /// 数据库字段值的有效性判断
    /// </summary>
    public class DbColumnAttribute : Attribute
    {
        /// <summary>
        /// 最小长度
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; set; }


    }
}
