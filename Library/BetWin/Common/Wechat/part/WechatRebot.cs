using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SP.Studio.Core;

namespace BW.Common.Wechat
{
    partial class WechatRebot
    {
        private RebotSetting _setting;

        /// <summary>
        /// 配置参数
        /// </summary>
        public RebotSetting Setting
        {
            get
            {
                if (_setting == null)
                {
                    _setting = new RebotSetting(this.SettingString);
                }
                return _setting;
            }
            set
            {
                this.SettingString = this._setting = value;
            }
        }

        public class RebotSetting : SettingBase
        {
            public RebotSetting(string setting) : base(setting) { }

            /// <summary>
            /// 指令
            /// </summary>
            public string Command { get; set; }

            /// <summary>
            /// 时间段开始
            /// </summary>
            public string Time1 { get; set; }

            /// <summary>
            /// 时间段结束
            /// </summary>
            public string Time2 { get; set; }

            /// <summary>
            /// 当前是否在时间区间内
            /// </summary>
            /// <returns></returns>
            public bool IsTime()
            {
                int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
                int time1 = 0;
                int time2 = 0;
                if (!string.IsNullOrEmpty(this.Time1))
                {
                    TimeSpan span1;
                    if (TimeSpan.TryParse(this.Time1, out span1)) time1 = (int)span1.TotalMinutes;
                }
                if (!string.IsNullOrEmpty(this.Time2))
                {
                    TimeSpan span2;
                    if (TimeSpan.TryParse(this.Time2, out span2)) time2 = (int)span2.TotalMinutes;
                }

                return time1 <= now && (time2 >= now || time2 == 0);
            }

            /// <summary>
            /// 投注概率
            /// </summary>
            public int Probability { get; set; }
        }
    }
}
