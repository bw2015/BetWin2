using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using BW.Framework;

using BW.Agent;

namespace BW.Common.Users
{
    /// <summary>
    /// 会员对象的枚举
    /// </summary>
    partial class User
    {
        public User() { }

        /// <summary>
        /// 会员状态
        /// </summary>
        public enum UserType : byte
        {
            [Description("会员")]
            Member = 0,
            [Description("代理")]
            Agent = 1
        }

        /// <summary>
        /// 用户的状态
        /// </summary>
        public enum UserStatus : byte
        {
            [Description("正常")]
            Normal = 0,
            [Description("锁定")]
            Lock = 1
        }

        /// <summary>
        /// 找回密码的类型
        /// </summary>
        public enum ForgetType : byte
        {
            [Description("资金密码")]
            PayPassword = 0,
            [Description("问题答案")]
            Answer = 1
        }

        /// <summary>
        /// 密码问题
        /// </summary>
        public enum QuestionType : byte
        {
            [Description("请选择密保问题")]
            None,
            [Description("您的母亲姓名是？")]
            Q1,
            [Description("您的父亲姓名是？")]
            Q2,
            [Description("您的配偶姓名是？")]
            Q3,
            [Description("您的配偶生日是？")]
            Q4,
            [Description("您的学号（或工号）是？")]
            Q5,
            [Description("您的小学班主任姓名是？")]
            Q6,
            [Description("您的初中班主任姓名是？")]
            Q7,
            [Description("您的高中班主任姓名是？")]
            Q8,
            [Description("您最熟悉的童年好友姓名是？")]
            Q9
        }

        /// <summary>
        /// 锁定类型
        /// </summary>
        [Flags]
        public enum LockStatus : byte
        {
            [Description("登录")]
            Login = 1,
            [Description("充值")]
            Recharge = 2,
            [Description("投注")]
            Bet = 4,
            [Description("提现")]
            Withdraw = 8,
            [Description("转账")]
            Transfer = 16,
            /// <summary>
            /// 契约锁定。 被锁定情况下不能投注、转账、提现
            /// </summary>
            [Description("契约")]
            Contract = 32
        }

        /// <summary>
        /// 功能开放
        /// </summary>
        [Flags]
        public enum FunctionType
        {
            /// <summary>
            /// 对上级转账
            /// </summary>
            [Description("上级转账")]
            TransferUp = 1,
            /// <summary>
            /// 对下级转账
            /// </summary>
            [Description("下级转账")]
            TransferDown = 2,
            /// <summary>
            /// 系统直接发放分红
            /// </summary>
            [Description("总代分红")]
            Dividends = 4
        }

        /// <summary>
        /// 对外显示的名字
        /// </summary>
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(this.NickName)) return this.NickName;
                return this.UserName;
            }
        }

        /// <summary>
        /// 头像路径
        /// </summary>
        public string FaceShow
        {
            get
            {
                if (string.IsNullOrEmpty(this.Face)) return SysSetting.GetSetting().imgServer + "/images/user.png";
                return SysSetting.GetSetting().imgServer + this.Face;
            }
        }

        /// <summary>
        /// 总金额（可用余额，锁定金额，第三方钱包）
        /// </summary>
        public decimal TotalMoney
        {
            get
            {
                return this.Money + this.LockMoney + this.Wallet;
            }
        }


        /// <summary>
        /// IM需要的用户资料，json格式
        /// </summary>
        /// <returns></returns>
        public string ToIMString()
        {
            return string.Concat("{\"username\":\"", this.Name,
                "\",\"id\":\"", this.ID,
                "\",\"avatar\":\"", this.FaceShow,
                "\",\"online\":\"", this.IsOnline ? 1 : 0,
                "\",\"sign\":\"", (this.Name == this.UserName ? "" : this.UserName),
                "\"}");
        }

        public void Log(string content, params object[] args)
        {
            UserAgent.Instance().SaveLog(this.ID, content, args);
        }

    }
}
