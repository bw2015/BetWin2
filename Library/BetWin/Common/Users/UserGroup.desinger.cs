/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 会员分组
    /// </summary>
    [Table(Name = "usr_Group")]
    public partial class UserGroup
    {

        /// <summary>
        /// 分组编号
        /// </summary>
        [Column(Name = "GroupID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 分组名字
        /// </summary>
        [Column(Name = "GroupName")]
        public string Name { get; set; }

        /// <summary>
        /// 分组创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 分组备注
        /// </summary>
        [Column(Name = "GroupDesc")]
        public string Description { get; set; }

        /// <summary>
        /// 分组参数设定
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

        /// <summary>
        /// 是否是默认分组
        /// </summary>
        [Column(Name = "IsDefault")]
        public bool IsDefault { get; set; }

        /// <summary>
        /// 排序值，从大到小（为负数表示手动设定的分组，不受自动条件影响）
        /// </summary>
        [Column(Name = "Sort")]
        public short Sort { get; set; }

        /// <summary>
        /// 分组条件
        /// </summary>
        [Column(Name = "ConditionID")]
        public int ConditionID { get; set; }

    }
}
