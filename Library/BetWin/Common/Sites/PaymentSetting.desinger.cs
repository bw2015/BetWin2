/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
	/// 支付接口的参数设定
	/// </summary>
    [Table(Name = "site_PaymentSetting")]
    public partial class PaymentSetting
    {

        /// <summary>
        /// 支付接口编号
        /// </summary>
        [Column(Name = "PayID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 手续费，用于平台财务计算，不向会员收取
        /// </summary>
        [Column(Name = "Fee")]
        public Decimal Fee { get; set; }

        /// <summary>
        /// 单笔充值最低
        /// </summary>
        [Column(Name = "MinMoney")]
        public Decimal MinMoney { get; set; }

        /// <summary>
        /// 单笔充值最高
        /// </summary>
        [Column(Name = "MaxMoney")]
        public Decimal MaxMoney { get; set; }

        /// <summary>
        /// 是否开启
        /// </summary>
        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        ///  支付接口类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.GateWay.Payment.PaymentType Type { get; set; }

        /// <summary>
        /// 支付接口的参数设定
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

        /// <summary>
        /// 排序值，从小到大
        /// </summary>
        [Column(Name = "Sort")]
        public short Sort { get; set; }

        /// <summary>
        /// 适用平台
        /// </summary>
        [Column(Name = "Platform")]
        public PlatformType Platform { get; set; }

        /// <summary>
        /// 充值奖励（比例，可为负数）
        /// </summary>
        [Column(Name = "Reward")]
        public Decimal Reward { get; set; }

        /// <summary>
        /// 图标类型
        /// </summary>
        [Column(Name = "Icon")]
        public IconType Icon { get; set; }

    }
}
