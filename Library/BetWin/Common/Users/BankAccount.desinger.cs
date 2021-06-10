/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    ///  用户的银行帐号
    /// </summary>
    [Table(Name = "usr_BankAccount")]
    public partial class BankAccount
    {

        /// <summary>
        /// 账户编号
        /// </summary>
        [Column(Name = "AccountID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 银行类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Sites.BankType Type { get; set; }

        /// <summary>
        /// 分行
        /// </summary>
        [Column(Name = "Bank")]
        public string Bank { get; set; }

        /// <summary>
        /// 银行帐号
        /// </summary>
        [Column(Name = "Account")]
        public string Account { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

    }
}
