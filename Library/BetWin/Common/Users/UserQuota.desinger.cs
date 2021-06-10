/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户的配额设定
    /// </summary>
    [Table(Name = "usr_Quota")]
    public partial class UserQuota
    {

        [Column(Name = "QuotaID", IsPrimaryKey = true)]
        public int ID { get; set; }

        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Column(Name = "Number")]
        public int Number { get; set; }

    }
}
