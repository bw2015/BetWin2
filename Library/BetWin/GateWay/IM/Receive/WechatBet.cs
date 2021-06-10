using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;
using BW.Common.Lottery;
using BW.Common.Users;
using SP.Studio.Core;
using SP.Studio.Web;

namespace BW.GateWay.IM.Receive
{
    /// <summary>
    /// 微信投注
    /// </summary>
    public class WechatBet : IReceive
    {
        public WechatBet(Hashtable ht) : base(ht) { }

        /// <summary>
        /// 投注内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 当前用户的session值
        /// </summary>
        public string Session { get; set; }

        /// <summary>
        /// 当前游戏
        /// </summary>
        public string Game { get; set; }

        /// <summary>
        /// 唯一的信息编号
        /// </summary>
        public long MsgID { get; set; }

        /// <summary>
        /// 是否是登录
        /// </summary>
        public string Login { get; set; }


        public override void Run()
        {
            LotteryType type = this.Game.ToEnum<LotteryType>();
            int userId = UserAgent.Instance().GetUserID(Guid.Parse(this.Session));
            int siteId = UserAgent.Instance().GetSiteID(userId);
            if (!string.IsNullOrEmpty(this.Login))
            {
                this.WechatLogin(type, userId);
            }
            else
            {
                LotteryAgent.Instance().MessageClean();
                if (!LotteryAgent.Instance().SaveOrder(userId, type, this.Content))
                {
                    this.Tip(LotteryAgent.Instance().Message(), "Delete");
                    return;
                }
                else
                {
                    this.Tip(string.Format("投注成功，当前余额：{0}元", UserAgent.Instance().GetUserMoney(userId).ToString("n")),
                        "BetSuccess");
                    this.GroupMessage(type, siteId, userId, "(投注成功)" + this.Content);
                }
            }
        }

        /// <summary>
        /// 回复发送者
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        private void Tip(string message, string type)
        {
            Config.Send(this.ID, new BW.GateWay.IM.Message.Tip()
            {
                Content = message,
                TipType = type,
                MsgId = this.MsgID
            });
        }

        /// <summary>
        /// 写入在线的微信群中
        /// </summary>
        private void WechatLogin(LotteryType type, int userId)
        {
            if (!Config.WechatUser.ContainsKey(type)) return;
            string key = string.Concat(UserAgent.IM_USER, "-", userId);
            int siteId = UserAgent.Instance().GetSiteID(userId);
            if (!Config.WechatUser[type].Contains(key))
            {
                Config.WechatUser[type].Add(key);
            }
            if (!Config.SiteUser.ContainsKey(siteId)) Config.SiteUser.Add(siteId, new List<string>());
            if (!Config.SiteUser[siteId].Contains(key))
            {
                Config.SiteUser[siteId].Add(key);
            }
            // 从其他微信游戏群中删除该用户
            foreach (KeyValuePair<LotteryType, List<string>> item in Config.WechatUser.Where(t => t.Key != type))
            {
                if (item.Value.Contains(key)) item.Value.Remove(key);
            }
        }

        /// <summary>
        /// 发送给在线的所有会员
        /// </summary>
        private void GroupMessage(LotteryType type, int siteId, int userId, string content)
        {
            if (!Config.WechatUser.ContainsKey(type)) return;
            IEnumerable<string> list = Config.WechatUser[type].Join(Config.SiteUser[siteId], t => t, t => t, (a, b) => a).ToArray();
            User user = UserAgent.Instance().GetUserInfo() ?? UserAgent.Instance().GetUserInfo(userId);
            string groupKey = string.Format("{0}-{1}", siteId, type);
            foreach (string key in list)
            {
                Config.Send(key, new BW.GateWay.IM.Message.Message()
                {
                    Content = content,
                    Avatar = user.FaceShow,
                    Name = user.UserName,
                    SendID = user.ID.ToString(),
                    Key = this.Game,
                    Time = WebAgent.GetTimeStamps(),
                    Type = "Wechat"
                });
            }

            if (Config.WechatGroupSetting.ContainsKey(groupKey) && Config.WechatGroupSetting[groupKey].Setting.BetMessage)
            {
                foreach (string key in Config.serviceList[siteId].Select(t => string.Concat(UserAgent.IM_ADMIN, "-", t)).ToArray())
                {
                    Config.Send(key, new BW.GateWay.IM.Message.Message()
                    {
                        Content = content,
                        Avatar = user.FaceShow,
                        Name = user.UserName,
                        SendID = user.ID.ToString(),
                        Key = UserAgent.Instance().GetTalkKey(key, type.ToEnum<ChatTalk.GroupType>()),
                        Time = WebAgent.GetTimeStamps(),
                        Type = "group"
                    });
                }
            }
        }
    }
}
