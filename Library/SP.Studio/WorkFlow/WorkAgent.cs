using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Web;
using System.IO;

using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.IO;

namespace SP.Studio.WorkFlow
{
    /// <summary>
    /// 工作流与配置数据库操作的代理类
    /// 修改成为不需要与数据库关联，保存结果直接存入xml
    /// </summary>
    public class WorkAgent
    {
        /// <summary>
        /// 创建/保存工作流配置信息至xml配置文件
        /// </summary>
        internal static void CreateConfigurationFile()
        {
            WorkFlowSetting.IsUpdate = true;
        }

        /// <summary>
        /// 从xml配置文件中获取工作配置信息
        /// </summary>
        internal static Workflow GetConfiguration()
        {
            Workflow workFlow = null;
            if (File.Exists(WorkFlowSetting.cfgPath))
                workFlow = (Workflow)(FileAgent.ReadText(WorkFlowSetting.cfgPath, Encoding.UTF8).ToObject(typeof(Workflow), Encoding.UTF8));

            if (WorkFlowSetting.WorkFlow == null)
            {
                workFlow = new Workflow()
                {
                    GroupList = new List<WorkGroup>(){ new WorkGroup(){ ID=1,
                            Name="WorkFlow",
                            Assembly="",
                            Description ="系统自动生成",
                            Setting = new WorkGroup.WorkGroupSetting(){ Height = 600},
                            Sort = 0 
                    }
                  }
                };
                CreateConfigurationFile();
            }
            return workFlow;
        }

        /// <summary>
        /// 从缓存中获取
        /// </summary>
        /// <param name="pageID"></param>
        /// <returns></returns>
        internal static WorkPage GetWorkPage(int pageID)
        {
            WorkPage workPage = WorkFlowSetting.WorkFlow.PageList.Find(t => t.ID == pageID);
            if (workPage == null) throw new Exception(string.Format("未找到ID为{0}的相关页面配置。\n\n可能的原因：1、未增加对应的页面\n2、配置文件与数据库配置不同步。\n3、未刷新缓存", pageID));
            return workPage;
        }

        /// <summary>
        /// 根据名字获取工作流路径
        /// </summary>
        public static string GetUrl(string groupName, string pageName)
        {
            WorkGroup group = WorkFlowSetting.WorkFlow.GroupList.Find(t => t.Name == groupName);
            if (group == null) return null;
            var page = WorkFlowSetting.WorkFlow.PageList.Find(t => t.GroupID == group.ID && t.Name == pageName);
            return page == null ? null : "/WorkFlow/" + page.ID + ".aspx";
        }

        public static string GetUrl(string pageName)
        {
            var page = WorkFlowSetting.WorkFlow.PageList.Find(t => t.Name == pageName);
            return page == null ? null : "/WorkFlow/" + page.ID + ".aspx";
        }
    }
}
