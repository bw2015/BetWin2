/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Games
{
    /// <summary>
	/// 接口的游戏排序设定
	/// </summary>
    [Table(Name = "game_Player")]
    public partial class GamePlayer
    {


        [Column(Name = "PlayerID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [Column(Name = "Type")]
        public GameType Type { get; set; }

        /// <summary>
        /// 游戏代码（唯一值）
        /// </summary>
        [Column(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// 是否开放游戏
        /// </summary>
        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// 推荐游戏
        /// </summary>
        [Column(Name = "IsTop")]
        public bool IsTop { get; set; }

    }
}
