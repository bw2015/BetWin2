using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using BW.Common.Wechat;

namespace BW.Common
{
    /// <summary>
    /// 微信
    /// </summary>
    partial class BetDataContext
    {
        /// <summary>
        /// 微信公共号的配置参数
        /// </summary>
        public Table<WechatSetting> WechatSetting
        {
            get
            {
                return this.GetTable<WechatSetting>();
            }
        }

        /// <summary>
        /// 微信机器人
        /// </summary>
        public Table<WechatRebot> WechatRebot
        {
            get
            {
                return this.GetTable<WechatRebot>();
            }
        }

        /// <summary>
        /// 微信群设置
        /// </summary>
        public Table<GroupSetting> GroupSetting
        {
            get
            {
                return this.GetTable<GroupSetting>();
            }
        }
    }
}
