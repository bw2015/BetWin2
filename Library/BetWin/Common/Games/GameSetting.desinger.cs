/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Games
{
    /// <summary>
	/// 第三方游戏的接口参数设定
	/// </summary>
    [Table(Name = "game_Setting")]
    public partial class GameSetting
    {

        /// <summary>
        /// 接口类型
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public GameType Type { get; set; }


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 是否开启该接口
        /// </summary>
        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// 自动注册
        /// </summary>
        [Column(Name = "IsRegister")]
        public bool IsRegister { get; set; }

        /// <summary>
        /// 提现的流水倍数
        /// </summary>
        [Column(Name = "Turnover")]
        public Decimal Turnover { get; set; }

        /// <summary>
        /// 自定义排序，从大到小
        /// </summary>
        [Column(Name = "Sort")]
        public byte Sort { get; set; }

        /// <summary>
        /// 总共的额度（分红结算之后清零）
        /// </summary>
        [Column(Name = "TotalMoney")]
        public Decimal TotalMoney { get; set; }

        /// <summary>
        /// 当前的余额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 计算比例
        /// </summary>
        [Column(Name = "Rate")]
        public Decimal Rate { get; set; }

        /// <summary>
        /// 自定义的参数设定
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

    }
}
