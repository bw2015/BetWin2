/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{

    [Table(Name = "Site")]
    public partial class Site
    {

        /// <summary>
        /// 站点编号
        /// </summary>
        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int ID { get; set; }

        /// <summary>
        /// 站点名
        /// </summary>
        [Column(Name = "SiteName")]
        public string Name { get; set; }

        /// <summary>
        /// 站点的备注信息（超级管理员可见）
        /// </summary>
        [Column(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///  站点状态
        /// </summary>
        [Column(Name = "Status")]
        public SiteStatus Status { get; set; }

        /// <summary>
        /// 停站说明
        /// </summary>
        [Column(Name = "StopDesc")]
        public string StopDesc { get; set; }

        /// <summary>
        /// 站点参数设定
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

        /// <summary>
        ///  系统级别的设置，管理员无法操作此参数
        /// </summary>
        [Column(Name = "SysConfig")]
        public String ConfigString { get; set; }

        /// <summary>
        /// 机器人的设定
        /// </summary>
        [Column(Name = "Rebot")]
        public string RebotString { get; set; }

    }
}
