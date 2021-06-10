using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using SP.Studio.Core;
using SP.Studio.Model;

using BW.Agent;
using BW.Common.Systems;

namespace BW.Handler.admin
{
    public class system : IHandler
    {
        /// <summary>
        /// 待支付账单列表
        /// </summary>
        /// <param name="context"></param>
        private void billlist(HttpContext context)
        {
            IQueryable<SystemBill> list = BDC.SystemBill.Where(t => t.SiteID == SiteInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.Title,
                t.ID,
                t.Money,
                t.Paid,
                t.CreateAt,
                t.EndAt,
                Status = t.Status.GetDescription()
            }));
        }

        /// <summary>
        /// 订单信息
        /// </summary>
        /// <param name="context"></param>
        private void billinfo(HttpContext context)
        {
            SystemBill bill = BDC.SystemBill.Where(t => t.SiteID == SiteInfo.ID && t.ID == QF("BillID", Guid.Empty)).FirstOrDefault();
            if (bill == null)
            {
                context.Response.Write(false, "订单号错误");
            }
            if (bill.Status != SystemBill.BillStatus.Normal)
            {
                context.Response.Write(false, "当前账单状态为" + bill.Status.GetDescription());
            }
            SystemPayment payment = SystemAgent.Instance().GetPaymentSetting(bill.PayID);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                bill.Title,
                bill.Content,
                bill.EndAt,
                bill.CreateAt,
                BillID = bill.ID,
                Money = bill.Money - bill.Paid,
                bill.Status,
                Bank = new JsonString(payment.GetPaymentInfo().Bank == null ? "null" : string.Concat("{", string.Join(",", payment.GetPaymentInfo().Bank.Select(t => string.Format("\"{0}\":\"{1}\"", t, t.GetDescription()))), "}"))
            });
        }

        /// <summary>
        /// 提交支付
        /// </summary>
        /// <param name="context"></param>
        private void billpayment(HttpContext context)
        {
            SystemBill bill = BDC.SystemBill.Where(t => t.SiteID == SiteInfo.ID && t.ID == QF("BillID", Guid.Empty)).FirstOrDefault();
            if (bill == null)
            {
                context.Response.Write(false, "订单号错误");
            }
            if (bill.Status != SystemBill.BillStatus.Normal)
            {
                context.Response.Write(false, "当前账单状态为" + bill.Status.GetDescription());
            }
            SystemBillOrder order = SystemAgent.Instance().AddBillOrder(bill.ID, QF("Money", decimal.Zero));
            if (order == null)
            {
                context.Response.Write(false, SystemAgent.Instance().Message());
            }
            SystemPayment payment = SystemAgent.Instance().GetPaymentSetting(bill.PayID);
            lock (typeof(SystemPayment))
            {
                context.Response.ContentType = "text/html";
                payment.GoGateway(order.ID, order.Money);
            }
        }

        /// <summary>
        /// 公告列表
        /// </summary>
        /// <param name="context"></param>
        private void newslist(HttpContext context)
        {
            IQueryable<SystemNews> list = BDC.SystemNews.Where(t => t.Type == QF("Type"));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                t.Title,
                t.CreateAt,
                t.Content
            }));
        }
    }
}
