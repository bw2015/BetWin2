/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    ///  公告的阅读日期
    /// </summary>
    [Table(Name = "site_NewsRead")]
    public partial class NewsRead
    {


        [Column(Name = "NewsID", IsPrimaryKey = true)]
        public int NewsID { get; set; }


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }


        [Column(Name = "ReadAt")]
        public DateTime ReadAt { get; set; }

    }
}
