using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Data;

namespace BW.IM.Common
{
    /// <summary>
    /// 群参数设定
    /// </summary>
    public class GroupSetting
    {
        public GroupSetting(DataRow dr)
        {
            this.Key = Utils.GetGroupID((int)dr["SiteID"], (GroupType)dr["Type"]);
            string setting = (string)dr["Setting"];
            NameValueCollection request = HttpUtility.ParseQueryString(setting);

            this.Chat = request["Chat"] == "True";
            this.BetMessage = request["BetMessage"] == "True";
        }

        /// <summary>
        /// 群参数设定
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 允许聊天
        /// </summary>
        public bool Chat { get; set; }

        /// <summary>
        /// 是否接收群信息
        /// </summary>
        public bool BetMessage { get; set; }

    }
}
