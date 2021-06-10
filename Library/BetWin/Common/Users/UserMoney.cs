using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BW.Common.Users
{
    /// <summary>
    /// 资金
    /// </summary>
    public struct UserMoney
    {
        public UserMoney(DataSet ds)
        {
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                this.Money = this.LockMoney = this.Wallet = this.Withdraw = 0.00M;
                return;
            }
            DataRow dr = ds.Tables[0].Rows[0];
            this.Money = dr.Table.Columns.Contains("Money") ? (decimal)dr["Money"] : 0.00M;
            this.LockMoney = dr.Table.Columns.Contains("LockMoney") ? (decimal)dr["LockMoney"] : 0.00M;
            this.Wallet = dr.Table.Columns.Contains("Wallet") ? (decimal)dr["Wallet"] : 0.00M;
            this.Withdraw = dr.Table.Columns.Contains("Withdraw") ? (decimal)dr["Withdraw"] : 0.00M;
        }

        /// <summary>
        /// 可用余额
        /// </summary>
        public decimal Money;

        /// <summary>
        /// 锁定资金
        /// </summary>
        public decimal LockMoney;

        /// <summary>
        /// 第三方账户钱包
        /// </summary>
        public decimal Wallet;

        /// <summary>
        /// 当前的可提现额度
        /// </summary>
        public decimal Withdraw;

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalMoney
        {
            get { return this.Money + this.LockMoney + this.Wallet; }
        }

        /// <summary>
        /// 当前的余额
        /// </summary>
        public decimal Balance
        {
            get
            {
                return this.Money + this.LockMoney;
            }
        }

        /// <summary>
        /// 更新一个用户对象的资金缓存
        /// </summary>
        /// <param name="user"></param>
        public void Update(User user)
        {
            user.Money = this.Money;
            user.LockMoney = this.LockMoney;
            user.Wallet = this.Wallet;
            user.Withdraw = this.Withdraw;
        }
    }
}
