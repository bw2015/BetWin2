/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Logs
{
    /// <summary>
    /// 与第三方游戏接口之间通信的日志记录
    /// </summary>
    [Table(Name = "log_GameGateway")]
    public partial class GameGatewayLog
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 涉及到的用户，如果不存在用户就是0
        /// </summary>
        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Games.GameType Type { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 要保存的内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 详情
        /// </summary>
        [Column(Name = "LogData")]
        public String LogData { get; set; }

    }
}
