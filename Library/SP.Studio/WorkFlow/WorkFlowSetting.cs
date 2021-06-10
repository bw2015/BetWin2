using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Web;

using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.IO;

namespace SP.Studio.WorkFlow
{
    /// <summary>
    /// 工作流缓存配置
    /// </summary>
    public class WorkFlowSetting
    {
        /// <summary>
        /// 保存的定时器
        /// </summary>
        static Timer timer = new Timer(3000);

        static WorkFlowSetting()
        {
            cfgPath = HttpContext.Current.Server.MapPath("~/App_Data/WorkFlow.xml");

            timer.Elapsed += (sender, e) =>
            {
                if (!IsUpdate) return;
                Encoding encoding = Encoding.UTF8;
                FileAgent.Write(cfgPath, WorkFlowSetting.WorkFlow.ToXmlString(encoding), encoding, false);

                IsUpdate = false;
            };
        }

        /// <summary>
        /// 查询参数
        /// </summary>
        public const string KEY_Query = "QueryString";

        /// <summary>
        /// 是否有改变
        /// </summary>
        internal static bool IsUpdate = false;

        /// <summary>
        /// 配置文件路径
        /// </summary>
        internal readonly static string cfgPath;


        #region  工作流配置信息缓存 

        public static Workflow WorkFlow = new Workflow(); 

        #endregion

        /// <summary>
        /// 设置内存变量
        /// </summary>
        internal static void Install()
        {
            timer.Start();
            LoadWorkFlowCache();
        }

       

        internal static void LoadWorkFlowCache()
        {
            WorkFlow = WorkAgent.GetConfiguration();
        }
    }
}
