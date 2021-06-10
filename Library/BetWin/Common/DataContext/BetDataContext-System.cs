using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Common.Systems;

namespace BW.Common
{
    partial class BetDataContext
    {
        /// <summary>
        /// 系统通用的邀请链接
        /// </summary>
        public Table<InviteDomain> InviteDomain
        {
            get
            {
                return this.GetTable<InviteDomain>();
            }
        }

        /// <summary>
        /// 整个平台的游戏接口
        /// </summary>
        public Table<GameInterface> GameInterface
        {
            get
            {
                return this.GetTable<GameInterface>();
            }
        }

        /// <summary>
        /// 系统账单
        /// </summary>
        public Table<SystemBill> SystemBill
        {
            get
            {
                return this.GetTable<SystemBill>();
            }
        }

        /// <summary>
        /// 支付接口
        /// </summary>
        public Table<SystemPayment> SystemPayment
        {
            get
            {
                return this.GetTable<SystemPayment>();
            }
        }

        /// <summary>
        /// 系统公告
        /// </summary>
        public Table<SystemNews> SystemNews
        {
            get
            {
                return this.GetTable<SystemNews>();
            }
        }

        /// <summary>
        /// 系统历史记录转换的标记
        /// </summary>
        public Table<SystemMark> SystemMark
        {
            get
            {
                return this.GetTable<SystemMark>();
            }
        }
    }
}
