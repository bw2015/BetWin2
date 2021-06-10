/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
	/// 用户的登录设备
	/// </summary>
    [Table(Name = "usr_Device")]
    public partial class UserDevice
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }

        /// <summary>
        /// 设备的唯一标识
        /// </summary>
        [Column(Name = "UUID", IsPrimaryKey = true)]
        public Guid UUID { get; set; }

        /// <summary>
        /// 设备型号
        /// </summary>
        [Column(Name = "Model")]
        public string Model { get; set; }

        /// <summary>
        /// 最近使用这台设备的时间
        /// </summary>
        [Column(Name = "UpdateAt")]
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [Column(Name = "Phone")]
        public string Phone { get; set; }

        /// <summary>
        /// 用户自己设定的密钥（MD5加密）
        /// </summary>
        [Column(Name = "Key")]
        public string Key { get; set; }

    }
}
