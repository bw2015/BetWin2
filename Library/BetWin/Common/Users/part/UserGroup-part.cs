using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Core;
using SP.Studio.Web;

using BW.Agent;
using BW.Common.Sites;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户分组的参数设定
    /// </summary>
    partial class UserGroup
    {
        private GroupSetting _setting;
        /// <summary>
        /// 参数设定
        /// </summary>
        public GroupSetting Setting
        {
            get
            {
                if (this._setting != null) return this._setting;
                this._setting = new GroupSetting(this.SettingString);
                return this._setting;
            }
            set
            {
                this.SettingString = this._setting = value;
            }
        }

        public class GroupSetting : SettingBase
        {
            public GroupSetting() : base() { }

            public GroupSetting(string setting) : base(setting) { }

            /// <summary>
            /// 支持的充值渠道
            /// </summary>
            public string PayID { get; set; }

            public int[] Payment
            {
                get
                {
                    return WebAgent.GetArray<int>(this.PayID);
                }
            }

            public string ToJson()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{")
                    .Append("\"Payment\":[")
                    .Append(string.Join(",", SiteAgent.Instance().GetPaymentSettingList(true).Select(t =>
                    {
                        return string.Format("{{\"ID\":{0},\"Name\":\"{1}\",\"IsOpen\":{2},\"Selected\":{3}}}",
                            t.ID, t.Name, t.IsOpen ? 1 : 0, this.Payment.Contains(t.ID) ? 1 : 0);
                    })))
                    .Append("]");

                sb.Append("}");

                return sb.ToString();
            }
        }
    }
}

