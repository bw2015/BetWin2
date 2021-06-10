using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SP.Studio.Core;
using BW.Common.Lottery;
using BW.Agent;
namespace BW.GateWay.Lottery
{
    /// <summary>
    /// 玩法工厂类
    /// </summary>
    public class PlayerFactory
    {
        /// <summary>
        /// 根据站点定义的玩法编号获取玩法对象
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public static IPlayer GetPlayer(int playerId, out LotteryType type)
        {
            LotteryPlayer player = LotteryAgent.Instance().GetPlayerInfo(playerId);
            if (player == null) { type = (LotteryType)0; return null; }
            return GetPlayer(player.Code, out type);
        }

        /// <summary>
        /// 根据代码获取玩法类
        /// </summary>
        /// <param name="code">彩种_玩法类库</param>
        /// <returns></returns>
        public static IPlayer GetPlayer(string code, out LotteryType type)
        {
            type = (LotteryType)0;
            Regex regex = new Regex(@"^(?<Game>\w+)_(?<Player>\w+)$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(code)) return null;

            type = regex.Match(code).Groups["Game"].Value.ToEnum<LotteryType>();
            if (!Enum.IsDefined(typeof(LotteryType), (byte)type)) return null;
            string player = regex.Match(code).Groups["Player"].Value;

            return GetPlayer(type.GetCategory().Cate, player);
        }

        /// <summary>
        /// 根据分类和类库名字获取类库
        /// </summary>
        /// <param name="category"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static IPlayer GetPlayer(LotteryCategory category, string player)
        {
            string typeName = string.Format("BW.GateWay.Lottery.{0}.{1}", category, player);
            Type type = typeof(PlayerFactory).Assembly.GetType(typeName);
            if (type == null) return null;

            return (IPlayer)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 从微信群聊中获取投注内容
        /// </summary>
        /// <param name="type">彩种类型</param>
        /// <param name="content">投注内容</param>
        /// <param name="number">翻译之后的投注内容</param>
        /// <param name="times">投注倍数</param>
        /// <param name="siteId">当前站点 非web程序的时候必须要赋值</param>
        /// <returns>JSON格式的投注内容（模仿前台的投注格式）</returns>
        public static string GetPlayer(BW.Common.Users.ChatTalk.GroupType type, string content, out string number, out int times, int siteId)
        {
            number = string.Empty;
            times = 0;
            LotteryType lottery = type.ToString().ToEnum<LotteryType>();
            string typeName = string.Format("BW.GateWay.Lottery.{0}.", lottery.GetCategory().Cate);
            IEnumerable<Type> typeList = typeof(PlayerFactory).Assembly.GetTypes().Where(t => t.FullName.StartsWith(typeName) && t.HasAttribute<BetChatAttribute>());
            if (typeList.Count() == 0) return null;
            foreach (Type t in typeList)
            {
                BetChatAttribute betChat = t.GetAttribute<BetChatAttribute>();
                if (betChat.IsMatch(content))
                {
                    IPlayer player = (IPlayer)Activator.CreateInstance(t);
                    number = player.GetBetChat(content, out times);
                    //[{"id":"2323","number":"01||||","mode":"元","times":1,"rebate":0,"reward":19.6,"bet":1,"money":2}]:
                    string code = string.Concat(lottery, "_", t.Name);
                    int playerId = 0;
                    if (siteId == 0)
                    {
                        playerId = LotteryAgent.Instance().GetPlayerID(code);
                    }
                    else
                    {
                        playerId = LotteryAgent.Instance().GetPlayerID(code, siteId);
                    }
                    return string.Format("[{0}]", new
                    {
                        id = playerId,
                        number = number,
                        mode = "元",
                        times = times
                    }.ToJson());
                }
            }
            return null;
        }

        public static string GetPlayer(LotteryType type, string content, out string number, out int times, int siteId)
        {
            number = string.Empty;
            times = 0;
            string typeName = string.Format("BW.GateWay.Lottery.{0}.", type.GetCategory().Cate);
            IEnumerable<Type> typeList = typeof(PlayerFactory).Assembly.GetTypes().Where(t => t.FullName.StartsWith(typeName) && t.HasAttribute<BetChatAttribute>());
            if (typeList.Count() == 0) return null;
            foreach (Type t in typeList)
            {
                BetChatAttribute betChat = t.GetAttribute<BetChatAttribute>();
                if (betChat.IsMatch(content))
                {
                    IPlayer player = (IPlayer)Activator.CreateInstance(t);
                    number = player.GetBetChat(content, out times);
                    //[{"id":"2323","number":"01||||","mode":"元","times":1,"rebate":0,"reward":19.6,"bet":1,"money":2}]:
                    string code = string.Concat(type, "_", t.Name);
                    int playerId = 0;
                    if (siteId == 0)
                    {
                        playerId = LotteryAgent.Instance().GetPlayerID(code);
                    }
                    else
                    {
                        playerId = LotteryAgent.Instance().GetPlayerID(code, siteId);
                    }
                    return string.Format("[{0}]", new
                    {
                        id = playerId,
                        number = number,
                        mode = "元",
                        times = times
                    }.ToJson());
                }
            }
            return null;
        }
    }
}
