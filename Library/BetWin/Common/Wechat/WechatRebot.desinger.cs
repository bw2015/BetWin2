/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Wechat
{
    /// <summary>
    ///  微信机器人
    /// </summary>
    [Table(Name = "wx_Rebot")]
    public partial class WechatRebot
    {

        /// <summary>
        /// 机器人编号
        /// </summary>
        [Column(Name = "RebotID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 机器人类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Users.ChatTalk.GroupType Type { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }


        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// 机器人参数设定
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

    }
}
