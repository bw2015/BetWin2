using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using BW.Common.Lottery;
using BW.Framework;
using BW.Agent;
using SP.Studio.Model;

namespace BW.Handler.game
{
    /// <summary>
    /// 老虎机小游戏
    /// </summary>
    public class slot : IHandler
    {
        /// <summary>
        /// 保存结果
        /// </summary>
        /// <param name="context"></param>
        private void save(HttpContext context)
        {
            StreamReader stream = new StreamReader(context.Request.InputStream);
            string data = stream.ReadToEnd();
            DateTime? now = (DateTime?)context.Items[BetModule.DATETIME];

            List<LotteryOrder> orders = LotteryAgent.Instance().GetLotteryOrderList(data, true, now);
            if (orders == null)
            {
                context.Response.Write(false, LotteryAgent.Instance().Message());
            }

            LotteryOrder order = orders.FirstOrDefault();
            string resultNumber;
            this.ShowResult(context, LotteryAgent.Instance().SaveOrder(UserInfo.ID, order, out resultNumber),
                string.Format("{0}第{1}期投注成功", LotteryAgent.Instance().GetLotteryName(order.Type), order.Index), new
                {
                    Index = order.Index,
                    ResultNumber = resultNumber
                });
        }
    }
}
