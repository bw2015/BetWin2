/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户的层级
    /// </summary>
    [Table(Name = "usr_Depth")]
    public partial class UserDepth
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }

        /// <summary>
        /// 下级用户
        /// </summary>
        [Column(Name = "ChildID", IsPrimaryKey = true)]
        public int ChildID { get; set; }

        /// <summary>
        /// 下级用户与当前用户相差的层级（不可能为0）
        /// </summary>
        [Column(Name = "Depth")]
        public int Depth { get; set; }

    }
}
