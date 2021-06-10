using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Common.Users;


namespace BW.Common
{
    /// <summary>
    /// 用户对象
    /// </summary>
    partial class BetDataContext : DataContext, IDisposable
    {
        public Table<User> User
        {
            get { return this.GetTable<User>(); }
        }

        /// <summary>
        /// 会员分组
        /// </summary>
        public Table<UserGroup> UserGroup
        {
            get
            {
                return this.GetTable<UserGroup>();
            }
        }

        /// <summary>
        /// 分组条件
        /// </summary>
        public Table<GroupCondition> GroupCondition
        {
            get
            {
                return this.GetTable<GroupCondition>();
            }
        }

        public Table<UserInfoLog> UserInfoLog
        {
            get
            {
                return this.GetTable<UserInfoLog>();
            }
        }

        /// <summary>
        /// 注册链接
        /// </summary>
        public Table<UserInvite> UserInvite
        {
            get
            {
                return this.GetTable<UserInvite>();
            }
        }

        /// <summary>
        /// 会员的层级
        /// </summary>
        public Table<UserDepth> UserDepth
        {
            get
            {
                return this.GetTable<UserDepth>();
            }
        }

        /// <summary>
        /// 备注信息
        /// </summary>
        public Table<UserRemark> UserRemark
        {
            get
            {
                return this.GetTable<UserRemark>();
            }
        }

        public Table<UserSession> UserSession
        {
            get
            {
                return this.GetTable<UserSession>();
            }
        }

        /// <summary>
        /// 获取用户对于彩种的参数设定
        /// </summary>
        public Table<UserLotterySetting> UserLotterySetting
        {
            get
            {
                return this.GetTable<UserLotterySetting>();
            }
        }

        /// <summary>
        /// 提现银行绑定
        /// </summary>
        public Table<BankAccount> BankAccount
        {
            get
            {
                return this.GetTable<BankAccount>();
            }
        }

        /// <summary>
        /// 提现订单
        /// </summary>
        public Table<WithdrawOrder> WithdrawOrder
        {
            get
            {
                return this.GetTable<WithdrawOrder>();
            }
        }

        /// <summary>
        /// 提现订单的处理日志
        /// </summary>
        public Table<WithdrawOrderLog> WithdrawOrderLog
        {
            get
            {
                return this.GetTable<WithdrawOrderLog>();
            }
        }

        /// <summary>
        /// 用户的操作日志
        /// </summary>
        public Table<UserLog> UserLog
        {
            get
            {
                return this.GetTable<UserLog>();
            }
        }

        /// <summary>
        /// 帐变流水（视图）
        /// </summary>
        public Table<MoneyLog> MoneyLog
        {
            get
            {
                return this.GetTable<MoneyLog>();
            }
        }

        public Table<MoneyLog1> MoneyLog1
        {
            get
            {
                return this.GetTable<MoneyLog1>();
            }
        }

        /// <summary>
        /// 资金流水历史记录表
        /// </summary>
        public Table<MoneyHistory> MoneyHistory
        {
            get
            {
                return this.GetTable<MoneyHistory>();
            }
        }

        /// <summary>
        /// 聊天历史记录
        /// </summary>
        public Table<ChatLog> ChatLog
        {
            get
            {
                return this.GetTable<ChatLog>();
            }
        }

        /// <summary>
        /// 用户的配额数量
        /// </summary>
        public Table<UserQuota> UserQuota
        {
            get
            {
                return this.GetTable<UserQuota>();
            }
        }

        /// <summary>
        /// 充值订单
        /// </summary>
        public Table<RechargeOrder> RechargeOrder
        {
            get
            {
                return this.GetTable<RechargeOrder>();
            }
        }

        /// <summary>
        /// 锁定资金
        /// </summary>
        public Table<MoneyLock> MoneyLock
        {
            get
            {
                return this.GetTable<MoneyLock>();
            }
        }

        /// <summary>
        /// 用户在第三方游戏接口中的账户信息
        /// </summary>
        public Table<GameAccount> UserGame
        {
            get
            {
                return this.GetTable<GameAccount>();
            }
        }

        /// <summary>
        /// 站内信
        /// </summary>
        public Table<UserMessage> UserMessage
        {
            get
            {
                return this.GetTable<UserMessage>();
            }
        }

        /// <summary>
        /// 用户的通知
        /// </summary>
        public Table<UserNotify> UserNotify
        {
            get
            {
                return this.GetTable<UserNotify>();
            }
        }

        /// <summary>
        /// 转账订单
        /// </summary>
        public Table<TransferOrder> TransferOrder
        {
            get
            {
                return this.GetTable<TransferOrder>();
            }
        }

        /// <summary>
        /// 契约列表
        /// </summary>
        public Table<Contract> Contract
        {
            get
            {
                return this.GetTable<Contract>();
            }
        }

        /// <summary>
        /// 契约转账日志
        /// </summary>
        public Table<ContractLog> ContractLog
        {
            get
            {
                return this.GetTable<ContractLog>();
            }
        }

        /// <summary>
        /// 用户的授权设备
        /// </summary>
        public Table<UserHost> UserHost
        {
            get
            {
                return this.GetTable<UserHost>();
            }
        }

        /// <summary>
        /// 用户的登录设备
        /// </summary>
        public Table<UserDevice> UserDevice
        {
            get
            {
                return this.GetTable<UserDevice>();
            }
        }

        /// <summary>
        /// 用户绑定的微信帐号信息
        /// </summary>
        public Table<UserWechat> UserWechat
        {
            get
            {
                return this.GetTable<UserWechat>();
            }
        }

        /// <summary>
        /// 微信的随机密钥串
        /// </summary>
        public Table<UserWechatKey> UserWechatKey
        {
            get
            {
                return this.GetTable<UserWechatKey>();
            }
        }
    }
}
