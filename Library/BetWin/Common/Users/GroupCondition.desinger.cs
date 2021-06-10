/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{

    [Table(Name = "usr_GroupCondition")]
    public partial class GroupCondition
    {

        /// <summary>
        /// 编号
        /// </summary>
        [Column(Name = "ConditionID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 条件名称
        /// </summary>
        [Column(Name = "ConditionName")]
        public string Name { get; set; }

        /// <summary>
        /// 拼合的sql条件语句
        /// </summary>
        [Column(Name = "SQL")]
        public string SQL { get; set; }

    }
}
