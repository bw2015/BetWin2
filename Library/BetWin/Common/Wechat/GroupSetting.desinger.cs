/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Wechat
{
    /// <summary>
    /// 微信群设置
    /// </summary>
    [Table(Name = "wx_Group")]
    public partial class GroupSetting
    {

        /// <summary>
        ///  群类型
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public BW.Common.Users.ChatTalk.GroupType Type { get; set; }


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 参数设定
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

    }
}
