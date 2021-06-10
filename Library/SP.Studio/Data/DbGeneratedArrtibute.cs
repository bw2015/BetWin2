using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Data
{
    /// <summary>
    /// 用于描述字段在更新的时候自增
    /// 拥有该属性而且字段值为0的时候启用
    /// </summary>
    public class DbGeneratedAttribute : Attribute
    {
        public DbGeneratedAttribute()
        {
            this.Step = 1;
        }

        /// <summary>
        /// 自增量
        /// </summary>
        public int Step { get; set; }
    }
}
