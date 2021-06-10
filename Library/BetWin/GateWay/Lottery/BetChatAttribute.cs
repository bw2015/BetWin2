using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using SP.Studio.Core;
using SP.Studio.Web;

namespace BW.GateWay.Lottery
{
    /// <summary>
    /// 支持聊天下注的玩法类型
    /// </summary>
    public class BetChatAttribute : Attribute
    {
        public BetChatAttribute(string pattern)
        {
            this.Pattern = pattern;
        }

        /// <summary>
        /// 获取当前实例
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static BetChatAttribute Get(IPlayer player)
        {
            return player.GetType().GetAttribute<BetChatAttribute>();
        }

        private Regex _regex;
        public Regex Regex
        {
            get
            {
                if (_regex == null) _regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
                return _regex;
            }
        }

        /// <summary>
        /// 内容匹配的正则表达式
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// 是否匹配投注内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        internal bool IsMatch(string input)
        {
            return Regex.IsMatch(input, this.Pattern);
        }

        private GroupCollection _group;

        /// <summary>
        /// 获取值
        /// </summary>
        internal T GetValue<T>(string content, string name, T defaultValue) 
        {
            if (_group == null) _group = this.Regex.Match(content).Groups;
            string value = this._group[name].Value;
            if (string.IsNullOrEmpty(value) || !WebAgent.IsType<T>(value)) return defaultValue;
            return (T)value.GetValue(typeof(T));
        }

        /// <summary>
        /// 获取倍数
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        internal int GetTimes(string content)
        {
            int money = this.GetValue(content, "Money", 0);
            if (money == 0 || money % 2 != 0) return 0;
            return money / 2;
        }
    }
}
