using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SP.Studio.Data;
using SP.Studio.Core;

using BW.Common.Systems;

namespace BW.Agent
{
    partial class SystemAgent
    {
        /// <summary>
        /// 创建一个账单支付订单
        /// </summary>
        /// <param name="billId"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public SystemBillOrder AddBillOrder(Guid billId, decimal money)
        {
            if (money < 1)
            {
                base.Message("金额错误");
                return null;
            }

            SystemBillOrder order = new SystemBillOrder()
            {
                SiteID = SiteInfo.ID,
                CreateAt = DateTime.Now,
                BillID = billId,
                Money = money
            };

            order.Add(true);
            return order;
        }

        /// <summary>
        /// 获取支付接口
        /// </summary>
        /// <param name="payId"></param>
        /// <returns></returns>
        public SystemPayment GetPaymentSetting(int payId)
        {
            return BDC.SystemPayment.Where(t => t.PayID == payId).FirstOrDefault();
        }
    }
}
