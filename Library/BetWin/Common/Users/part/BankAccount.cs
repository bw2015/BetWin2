using BW.Agent;
using SP.Studio.Core;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Common.Users
{
    /// <summary>
    /// 提现账户
    /// </summary>
    public partial class BankAccount : CommBase
    {
        /// <summary>
        /// 该卡是否可以提现
        /// </summary>
        public bool IsWithdraw
        {
            get
            {
                if (((TimeSpan)(DateTime.Now - this.CreateAt)).TotalHours < SiteInfo.Setting.CardTime) return false;
                if (!SiteInfo.Setting.WithdrawBankList.Contains(this.Type)) return false;
                return true;
            }
        }

        public override string ToString()
        {
            string accountName = UserAgent.Instance().GetUserAccountName(this.UserID);
            return string.Format("{0} {1} 尾号：{2}", WebAgent.HiddenName(accountName),
                this.Type == Sites.BankType.BANK ? this.Bank : this.Type.GetDescription(),
                this.Account.Substring(this.Account.Length - 4));
        }

        /// <summary>
        /// 银行卡的json数据
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"ID\":{0},", this.ID)
                .AppendFormat("\"IsWithdraw\":{0},", this.IsWithdraw ? 1 : 0)
                .AppendFormat("\"Value\":\"{0}\"", this.ToString())
                .Append("}");
            return sb.ToString();
        }
    }
}
