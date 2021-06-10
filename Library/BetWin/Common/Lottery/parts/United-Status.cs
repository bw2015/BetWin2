using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SP.Studio.Core;
using BW.Agent;

namespace BW.Common.Lottery
{
    partial class United
    {
        public enum UnitedStatus : byte
        {
            /// <summary>
            /// 正常接受合买
            /// </summary>
            [Description("正常")]
            Normal = 0,
            /// <summary>
            /// 已完成投注
            /// </summary>
            [Description("已投注")]
            Order = 1,
            /// <summary>
            /// 已派奖
            /// </summary>
            [Description("完成")]
            Finish = 2,
            /// <summary>
            /// 已撤单
            /// </summary>
            [Description("撤单")]
            Revoke = 3
        }

        /// <summary>
        /// 公开选项
        /// </summary>
        public enum PublicType : byte
        {
            [Description("完全公开")]
            Public = 0,
            [Description("购买后公开")]
            Protected = 1,
            [Description("完全保密")]
            Private = 2
        }


        /// <summary>
        /// 每份价格（如果为0则表示价格错误)
        /// </summary>
        public decimal UnitMoney
        {
            get
            {
                decimal price = this.Money / (decimal)this.Total;
                if (Math.Round(price, 2) != price) return decimal.Zero;
                return price;
            }
        }

        /// <summary>
        /// 当前进度
        /// </summary>
        public decimal Progress
        {
            get
            {
                if (this.Total == 0) return decimal.Zero;
                return (decimal)this.Buyed / (decimal)this.Total;
            }
        }

        /// <summary>
        /// 状态名称
        /// </summary>
        public string StatusName
        {
            get
            {
                string status = this.Status.GetDescription();
                switch (this.Status)
                {
                    case UnitedStatus.Normal:
                        if (this.CloseAt < DateTime.Now)
                        {
                            status = "已封单";
                        }
                        else if (this.Buyed == this.Total)
                        {
                            status = "已投满";
                        }
                        break;
                    case UnitedStatus.Finish:
                        status = this.Reward == decimal.Zero ? "未中奖" : "已中奖";
                        break;
                }
                return status;
            }
        }

        /// <summary>
        /// 当前剩余份额
        /// </summary>
        public int Remaining
        {
            get
            {
                return this.Total - this.Buyed;
            }
        }

        /// <summary>
        /// 获取用户是否可以查看号码
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetNumber(int userId)
        {
            if (userId == this.UserID || this.Public == PublicType.Public)
            {
                return this.Number;
            }
            if (this.Public == PublicType.Private) return null;

            return LotteryAgent.Instance().IsUnitedJoin(this.ID, userId) ? this.Number : null;

        }
    }
}
