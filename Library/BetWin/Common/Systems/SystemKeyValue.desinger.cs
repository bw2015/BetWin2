using System;
using System.Data;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    [Table(Name = "sys_KeyValue")]
    public class SystemKeyValue
    {
        public SystemKeyValue() { }

        public SystemKeyValue(DataRow dr)
        {
            this.Key = (string)dr["Key"];
            this.Value = (string)dr["Value"];
            this.CreateAt = (DateTime)dr["CreateAt"];
        }

        [Column(Name = "Key", IsPrimaryKey = true)]
        public string Key { get; set; }

        [Column(Name = "Value")]
        public string Value { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }
    }
}
