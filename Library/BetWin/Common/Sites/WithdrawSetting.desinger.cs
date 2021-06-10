/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    ///  提现接口
    /// </summary>
    [Table(Name = "site_WithdrawSetting")]
    public partial class WithdrawSetting
    {

        /// <summary>
        /// 出款接口配置
        /// </summary>
        [Column(Name = "SettingID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 接口名字
        /// </summary>
        [Column(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        ///  提现接口类型 0：手动出款、1：通汇卡出款
        /// </summary>
        [Column(Name = "Type")]
        public BW.GateWay.Withdraw.WithdrawType Type { get; set; }


        [Column(Name = "Sort")]
        public short Sort { get; set; }

        /// <summary>
        /// 是否开放当前接口
        /// </summary>
        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// 支付接口的参数设定
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

    }
}
