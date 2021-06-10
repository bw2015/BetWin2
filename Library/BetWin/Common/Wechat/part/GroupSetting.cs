using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SP.Studio.Core;
using BW.Common.Users;

namespace BW.Common.Wechat
{
    partial class GroupSetting
    {
        public GroupSetting() { }

        public GroupSetting(ChatTalk.GroupType type)
        {
            this.Type = type;
            this.Setting = new WechatGroupSetting();
        }

        private WechatGroupSetting _setting;
        public WechatGroupSetting Setting
        {
            get
            {
                if (this._setting == null)
                {
                    this._setting = new WechatGroupSetting(this.SettingString);
                }
                return this._setting;
            }
            set
            {
                _setting = value;
                this.SettingString = _setting;
            }
        }

        public class WechatGroupSetting : SettingBase
        {
            public WechatGroupSetting() : base() { }

            public WechatGroupSetting(string setting) : base(setting) { }

            /// <summary>
            /// 接受群投注信息
            /// </summary>
            public bool BetMessage { get; set; }

            /// <summary>
            /// 允许聊天
            /// </summary>
            public bool Chat { get; set; }
        }
    }
}
