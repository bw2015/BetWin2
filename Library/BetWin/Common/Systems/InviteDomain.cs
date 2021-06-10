/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    /// 公用的邀请链接域名
    /// </summary>
    [Table(Name = "sys_InviteDomain")]
    public partial class InviteDomain
    {


        [Column(Name = "Domain", IsPrimaryKey = true)]
        public string Domain { get; set; }

    }
}
