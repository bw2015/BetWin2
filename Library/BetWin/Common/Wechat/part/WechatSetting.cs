using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SP.Studio.Core;
using SP.Studio.Web;
using BW.Framework;
using SP.Studio.GateWay.WeChat;

namespace BW.Common.Wechat
{
    partial class WechatSetting
    {
        /// <summary>
        /// 微信公共号对外显示的头像
        /// </summary>
        public string FaceShow
        {
            get
            {
                if (string.IsNullOrEmpty(this.Face))
                {
                    return SysSetting.GetSetting().GetImage("/images/space.gif");
                }
                return SysSetting.GetSetting().GetImage(this.Face);
            }
        }

        /// <summary>
        /// 当前是否微信登录而且是否开启了公众号对接
        /// </summary>
        /// <returns></returns>
        public bool IsWechat()
        {
            return WebAgent.IsWechat() && !string.IsNullOrEmpty(this.Setting.AppId);
        }

        private OpenSetting _setting;
        public OpenSetting Setting
        {
            get
            {
                if (_setting == null)
                {
                    _setting = new OpenSetting(this.SettingString);
                }
                return _setting;
            }
            set
            {
                this.SettingString = _setting = value;
            }
        }

        /// <summary>
        /// 微信公众号的设定
        /// </summary>
        public class OpenSetting : SettingBase
        {
            public OpenSetting() : base() { }

            public OpenSetting(string setting) : base(setting) { }

            /// <summary>
            /// 开发者ID
            /// </summary>
            public string AppId { get; set; }

            /// <summary>
            /// 密钥
            /// </summary>
            public string AppSecret { get; set; }

            /// <summary>
            /// 业务域名
            /// </summary>
            public string Domain { get; set; }

            /// <summary>
            /// 获取要跳转到的授权url
            /// </summary>
            /// <returns></returns>
            public string GetAuthorizeUrl(string redirect)
            {
                return WX.GetAuthorizeUrl(this.AppId, redirect, "wx");
            }
        }
    }
}
