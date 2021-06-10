/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    /// 系统通知信息
    /// </summary>
    [Table(Name = "sys_News")]
    public partial class SystemNews
    {


        [Column(Name = "NewsID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [Column(Name = "Type")]
        public string Type { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Column(Name = "Title")]
        public string Title { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 通知内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 点击数量
        /// </summary>
        [Column(Name = "Click")]
        public int Click { get; set; }

    }
}
