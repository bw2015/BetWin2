using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using SP.Studio.Core;
using SP.Studio.Web;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户资料变化日志
    /// </summary>
    partial class UserInfoLog
    {
        /// <summary>
        /// 日志类型
        /// </summary>
        public enum UserInfoLogType : byte
        {
            /// <summary>
            /// 银行账户
            /// </summary>
            [Description("银行账户")]
            BankAccount = 1,
            /// <summary>
            /// 登录密码
            /// </summary>
            [Description("登录密码")]
            Password = 2,
            /// <summary>
            /// 资金密码
            /// </summary>
            [Description("资金密码")]
            PayPassword = 3,
            /// <summary>
            /// 安全问题
            /// </summary>
            [Description("安全问题")]
            Answer = 4,
            /// <summary>
            /// 电子邮件
            /// </summary>
            [Description("电子邮件")]
            Email = 5,
            /// <summary>
            /// 手机号码
            /// </summary>
            [Description("手机号码")]
            Mobile = 6,
            /// <summary>
            /// QQ号码
            /// </summary>
            [Description("QQ号码")]
            QQ = 7,
            /// <summary>
            /// 上次登录
            /// </summary>
            [Description("上次登录")]
            LastLoginAt = 8,
            /// <summary>
            /// 安全码
            /// </summary>
            [Description("安全码")]
            SecretKey = 9
        }

        /// <summary>
        /// 转成json对象
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat("\"", this.Type, "\":{ \"Name\":\"", this.Type.GetDescription(), "\",\"UpdateAt\":\"", this.UpdateAt, "\",\"Time\":", WebAgent.GetTimeDiff(this.UpdateAt), "\"}");
        }
    }
}
