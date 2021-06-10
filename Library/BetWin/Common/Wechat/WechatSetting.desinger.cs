/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Wechat
{
    /// <summary>
    ///  微信设置
    /// </summary>
    [Table(Name = "wx_Setting")]
    public partial class WechatSetting
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "Name")]
        public string Name { get; set; }


        [Column(Name = "Face")]
        public string Face { get; set; }

        /// <summary>
        /// 微信公共号设定
        /// </summary>
        [Column(Name = "OpenSetting")]
        public string SettingString { get; set; }

    }
}
