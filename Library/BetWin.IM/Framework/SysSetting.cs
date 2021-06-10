using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml.Linq;

using SP.Studio.Core;
using BW.IM.Common;

namespace BW.IM.Framework
{
    public class SysSetting
    {
        static SysSetting()
        {
            SysSetting.GetSetting().Install();
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string DbConnection { get; private set; }

        /// <summary>
        /// 图片服务器地址
        /// </summary>
        public string imgServer { get; private set; }

        /// <summary>
        /// 投注处理服务器
        /// </summary>
        public string handlerServer { get; private set; }

        private Dictionary<GroupType, List<string>> _commandList;
        /// <summary>
        /// 投注指令列表
        /// </summary>
        public Dictionary<GroupType, List<string>> CommandList
        {
            get
            {
                if (_commandList == null)
                {
                    _commandList = this.loadCommandList();
                }
                return _commandList;
            }
        }

        public void Install()
        {
            this.DbConnection = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            this.imgServer = ConfigurationManager.AppSettings["imgserver"];
            this.handlerServer = ConfigurationManager.AppSettings["handlerserver"];
        }

        public static SysSetting GetSetting()
        {
            return Nested.intance;
        }

        /// <summary>
        /// 获取远程的命令列表
        /// </summary>
        /// <returns></returns>
        private Dictionary<GroupType, List<string>> loadCommandList()
        {
            try
            {
                string[] groupName = Enum.GetNames(typeof(GroupType));
                Dictionary<GroupType, List<string>> data = new Dictionary<GroupType, List<string>>();
                string url = string.Format("{0}/handler/game/wechat/commandlist", this.handlerServer);
                XElement root = XElement.Load(url);
                foreach (XElement item in root.Elements().Where(t => groupName.Contains(t.Name.ToString())))
                {
                    GroupType type = item.Name.ToString().ToEnum<GroupType>();
                    data.Add(type, item.Elements().Select(t => t.Value).ToList());
                }
                return data;
            }
            catch
            {
                return null;
            }
        }

        class Nested
        {
            internal readonly static SysSetting intance = new SysSetting();
        }
    }
}
