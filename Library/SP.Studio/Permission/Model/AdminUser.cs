using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using SP.Studio.Cache.Redis;
using StackExchange.Redis;

namespace SP.Studio.Permission.Model
{
    /// <summary>
	/// 通用的管理员操作表
	/// </summary>
    [Table(Name = "STUDIO_ADMIN")]
    public partial class AdminUser : IRedisBase
    {


        [Column(Name = "AdminID", IsPrimaryKey = true)]
        public int ID { get; set; }


        [Column(Name = "UserName")]
        public string UserName { get; set; }


        [Column(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 拥有的系统权限
        /// </summary>
        [Column(Name = "Permission")]
        public string Permission { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Column(Name = "Status")]
        public AdminStatus Status { get; set; }

        /// <summary>
        /// 谷歌验证码
        /// </summary>
        [Column(Name = "SecretKey")]
        public Guid SecretKey { get; set; }

        /// <summary>
        /// 最近一次登录时间
        /// </summary>
        [Column(Name = "LoginAt")]
        public DateTime LoginAt { get; set; }

        /// <summary>
        /// 登录IP
        /// </summary>
        [Column(Name = "LoginIP")]
        public string LoginIP { get; set; }

        public void FillHashEntry(IEnumerable<HashEntry> fields)
        {
            RedisUtils.Fill(this, fields);
        }

        public IEnumerable<HashEntry> ToHashEntry()
        {
            return RedisUtils.ToHashEntry(this);
        }
    }
}
