/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
	/// 平台会员
	/// </summary>
    [Table(Name = "Users")]
    public partial class User
    {

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column(Name = "UserID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserName")]
        public string UserName { get; set; }


        [Column(Name = "NickName")]
        public string NickName { get; set; }

        /// <summary>
        /// 用户密码 MD5加密
        /// </summary>
        [Column(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// 支付密码，SHA1+MD5 双重加密
        /// </summary>
        [Column(Name = "PayPassword")]
        public string PayPassword { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }


        [Column(Name = "RegIP")]
        public string RegIP { get; set; }

        /// <summary>
        /// 当前可用余额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 冻结资金（冻结的时候需要减去可用余额，用户总余额等于Money+LockMoney）
        /// </summary>
        [Column(Name = "LockMoney")]
        public Decimal LockMoney { get; set; }

        /// <summary>
        /// 第三方游戏总额度
        /// </summary>
        [Column(Name = "Wallet")]
        public Decimal Wallet { get; set; }

        /// <summary>
        /// 当前可用的提现额度
        /// </summary>
        [Column(Name = "Withdraw")]
        public Decimal Withdraw { get; set; }

        /// <summary>
        ///  用户类型（会员/代理）
        /// </summary>
        [Column(Name = "Type")]
        public UserType Type { get; set; }

        /// <summary>
        /// 上级代理
        /// </summary>
        [Column(Name = "AgentID")]
        public int AgentID { get; set; }

        /// <summary>
        /// 上次登录时间
        /// </summary>
        [Column(Name = "LoginAt")]
        public DateTime LoginAt { get; set; }

        /// <summary>
        /// 上次登录IP
        /// </summary>
        [Column(Name = "LoginIP")]
        public string LoginIP { get; set; }

        /// <summary>
        /// 所属奖金组
        /// </summary>
        [Column(Name = "Rebate")]
        public int Rebate { get; set; }

        /// <summary>
        /// QQ号码
        /// </summary>
        [Column(Name = "QQ")]
        public string QQ { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        [Column(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [Column(Name = "Mobile")]
        public string Mobile { get; set; }

        /// <summary>
        /// 所属的用户分组
        /// </summary>
        [Column(Name = "GroupID")]
        public int GroupID { get; set; }

        /// <summary>
        /// 用户的状态
        /// </summary>
        [Column(Name = "Status")]
        public UserStatus Status { get; set; }

        /// <summary>
        ///  锁定的类型，位枚举（登录、充值、投注、提现、转账）
        /// </summary>
        [Column(Name = "Lock")]
        public LockStatus Lock { get; set; }

        /// <summary>
        ///  位枚举，用户单独开放的功能
        /// </summary>
        [Column(Name = "Function")]
        public FunctionType Function { get; set; }

        /// <summary>
        /// 密码提示问题
        /// </summary>
        [Column(Name = "Question")]
        public QuestionType Question { get; set; }

        /// <summary>
        /// 问题答案，MD5加密
        /// </summary>
        [Column(Name = "Answer")]
        public string Answer { get; set; }

        /// <summary>
        /// 真实姓名，与银行开户号绑定
        /// </summary>
        [Column(Name = "AccountName")]
        public string AccountName { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        [Column(Name = "Face")]
        public string Face { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        [Column(Name = "IsOnline")]
        public bool IsOnline { get; set; }

        /// <summary>
        /// 活动时间
        /// </summary>
        [Column(Name = "ActiveAt")]
        public DateTime ActiveAt { get; set; }

        /// <summary>
        /// 测试帐号
        /// </summary>
        [Column(Name = "IsTest")]
        public bool IsTest { get; set; }

        /// <summary>
        /// 个性签名
        /// </summary>
        [Column(Name = "Sign")]
        public string Sign { get; set; }

        /// <summary>
        /// 谷歌验证码的key值
        /// </summary>
        [Column(Name = "SecretKey")]
        public Guid SecretKey { get; set; }

        /// <summary>
        /// 所属的层级
        /// </summary>
        [Column(Name = "UserLevel")]
        public int UserLevel { get; set; }

    }
}
