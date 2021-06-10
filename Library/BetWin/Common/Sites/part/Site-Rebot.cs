using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SP.Studio.Core;

using BW.Framework;

namespace BW.Common.Sites
{
    /// <summary>
    /// 机器人的设定
    /// </summary>
    partial class Site
    {
        private RebotSetting _rebot;

        public RebotSetting Rebot
        {
            get
            {
                if (_rebot == null)
                {
                    this._rebot = new RebotSetting(this.RebotString);
                }
                return this._rebot;
            }
            set
            {
                this.RebotString = _rebot = value;
            }
        }

        /// <summary>
        /// 机器人的参数设定
        /// </summary>
        public class RebotSetting : SettingBase
        {
            public RebotSetting() { }

            public RebotSetting(string setting) : base(setting) { }

            /// <summary>
            /// 是否开启机器人
            /// </summary>
            public bool IsOpen { get; set; }

            private string _name = "客服";
            /// <summary>
            /// 机器人的名字
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }

            /// <summary>
            /// 签名
            /// </summary>
            public string Sign { get; set; }

            private string _face = "/images/rebot.jpg";
            /// <summary>
            /// 存储的头像
            /// </summary>
            public string Face
            {
                get
                {
                    return _face;
                }
                set
                {
                    _face = value;
                }
            }

            /// <summary>
            /// 对外显示的头像
            /// </summary>
            public string FaceShow
            {
                get
                {
                    return SysSetting.GetSetting().GetImage(this.Face);
                }
            }
        }
    }
}
