using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    /// 系统配置
    /// </summary>
    [Table(Name = "sys_Mark")]
    public class SystemMark
    {
        /// <summary>
        /// 资金记录更新到的记录编号
        /// </summary>
        [Column(Name = "MoneyID")]
        public int MoneyID { get; set; }

        /// <summary>
        /// 资金记录的上次更新时间
        /// </summary>
        [Column(Name = "MoneyUpdateAt")]
        public DateTime MoneyUpdateAt { get; set; }
    }
}
