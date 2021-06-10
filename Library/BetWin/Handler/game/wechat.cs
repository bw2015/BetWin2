using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Web;

using BW.Agent;
using BW.Common.Lottery;
using BW.GateWay.Lottery;

using SP.Studio.Model;
using SP.Studio.Core;

namespace BW.Handler.game
{
    public class wechat : IHandler
    {
        /// <summary>
        /// 微信投注
        /// </summary>
        /// <param name="context"></param>
        private void bet(HttpContext context)
        {
            string content = QF("Content");
            LotteryType type = QF("Type").ToEnum<LotteryType>();

            string betIndex;
            if (LotteryAgent.Instance().SaveOrder(UserInfo.ID, type, content, out betIndex))
            {
                context.Response.Write(true, string.Format("{0}第{1}期投注成功，当前余额：{2}元",
                    LotteryAgent.Instance().GetLotteryName(type), betIndex,
                    UserAgent.Instance().GetUserMoney(UserInfo.ID).ToString("n")));
            }
            else
            {
                context.Response.Write(false, LotteryAgent.Instance().Message());
            }
        }

        /// <summary>
        /// 是否是投注时间
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void isbettime(HttpContext context)
        {
            LotteryType type = QF("Type").ToEnum<LotteryType>();
            int siteId = QF("SiteID", SiteInfo.ID);
            string betIndex;
            if (!Utils.IsBet(type, out betIndex, siteId))
            {
                context.Response.Write(false, "当前期已封单");
            }
            else
            {
                context.Response.Write(true, betIndex);
            }
        }

        /// <summary>
        /// 系统支持的指令列表
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void commandlist(HttpContext context)
        {
            context.Response.ContentType = "text/xml";
            XElement root = new XElement("root");

            foreach (LotteryType type in Enum.GetValues(typeof(LotteryType)))
            {
                if (!type.GetCategory().Wechat) continue;

                XElement item = new XElement(type.ToString());

                string typeName = string.Format("BW.GateWay.Lottery.{0}.", type.GetCategory().Cate);
                IEnumerable<Type> typeList = this.GetType().Assembly.GetTypes().Where(t => t.FullName.StartsWith(typeName) && t.HasAttribute<BetChatAttribute>());
                foreach (Type player in typeList)
                {
                    XElement play = new XElement(player.Name);
                    play.SetValue(player.GetAttribute<BetChatAttribute>().Pattern);
                    item.Add(play);
                }
                root.Add(item);
            }

            context.Response.Write(root);
        }
    }
}
