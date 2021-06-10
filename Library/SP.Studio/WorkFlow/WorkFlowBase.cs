using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;

using SP.Studio.Core;
using SP.Studio.PageBase;


namespace SP.Studio.WorkFlow
{
    /// <summary>
    /// 工作流页面的基类
    /// </summary>
    public abstract class WorkFlowBase : Pagebase
    {
        /// <summary>
        /// 保存动作名称的字段
        /// </summary>
        protected const string WorkFlowEvent = "WorkFlowEvent";

        protected WorkPage PageInfo
        {
            get
            {
                return (WorkPage)Context.Items["PAGEINFO"];
            }
        }

        /// <summary>
        /// 当前页面的所有事件
        /// </summary>
        protected List<WorkEvent> EventList
        {
            get
            {
                List<WorkLine> lineList = WorkFlowSetting.WorkFlow.LineList.FindAll(t => t.PageID == PageInfo.ID && t.EventID > 0);
                return WorkFlowSetting.WorkFlow.EventList.FindAll(t => lineList.Exists(t1 => t1.EventID == t.ID));
            }
        }

        protected WorkGroup GroupInfo
        {
            get
            {
                if (PageInfo == null) return null;
                return WorkFlowSetting.WorkFlow.GroupList.Find(t => t.ID == PageInfo.GroupID);
            }
        }

        private Assembly assemble;

        protected override void OnInit(EventArgs e)
        {
            if (GroupInfo != null)
            {
                assemble = Assembly.Load(GroupInfo.Assembly);
            }
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (GroupInfo != null)
            {
                if (Request.HttpMethod == "POST")
                {
                    this.FireEvent(QF(WorkFlowEvent));
                }
                this.FireEvent("Load");
            }
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (GroupInfo != null)
            {
                this.FireEvent("PreRender");
            }
            base.OnPreRender(e);
        }

        #region =========  私有方法  ===========

        /// <summary>
        /// 触发事件中的方法
        /// </summary>
        private void FireEvent(string eventName)
        {
            int eventID;
            WorkEvent loadEvent = this.EventList.Find(t => t.Name.Equals(eventName, StringComparison.CurrentCultureIgnoreCase));
            if (loadEvent == null) return;
            eventID = loadEvent.ID;
            Type type;
            try
            {
                type = assemble.GetType(loadEvent.Type, true);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}\n没有对事件:{1}指定类型或者指定的类型不存在。\n{2}", ex.Message, loadEvent.Name, loadEvent.Type));
            }
            MethodInfo method = type.GetMethod(loadEvent.Method);
            if (method == null) throw new Exception(string.Format("事件:{0}找不到方法:{1}({2})", loadEvent.Name, type.FullName, loadEvent.Method));
            string resultValue = method.Invoke(method.IsStatic ? null : Activator.CreateInstance(type), loadEvent.Params  == null ? null : new object[]{ loadEvent.Params }).ToString();
            List<WorkLine> lineList = WorkFlowSetting.WorkFlow.LineList.FindAll(t => t.EventID == eventID && t.ResultID > 0);
            List<WorkResult> resultList = WorkFlowSetting.WorkFlow.ResultList.FindAll(t => lineList.Exists(t1 => t1.ResultID == t.ID));
            WorkResult result = resultList.Find(t => t.Name.Equals(resultValue, StringComparison.CurrentCultureIgnoreCase));
            if (result == null)
            {
                if (resultValue == ResultType.None.ToString()) return;
                throw new Exception(string.Format("没有对事件“{0}”的返回值“{1}”做出配置", loadEvent.Name, resultValue));
            }
            if (!string.IsNullOrEmpty(result.Type))
            {
                type = assemble.GetType(result.Type, true);
                method = type.GetMethod(result.Method);
                if (method == null) throw new Exception(string.Format("返回值:{0}的处理方法不存在\nnType:{1}\nnMethod:{2}", result.Name, type.FullName, result.Method));
                try
                {
                    method.Invoke(method.IsStatic ? null : Activator.CreateInstance(type), new object[] { result.Params });
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{0}\nResult:{1}\nType:{2}\nMethod:{3}\nParams:{4}", ex.Message, resultValue, type.FullName, method.Name, result.Params.ToJsonString()));
                }
            }
            int next = result.Next;
            if (next == 0)
            {
                lineList = WorkFlowSetting.WorkFlow.LineList.FindAll(t => t.ResultID == result.ID && t.PageID > 0);
                WorkPage page = WorkFlowSetting.WorkFlow.PageList.Find(t => lineList.Exists(t1 => t1.PageID == t.ID));
                next = page == null ? 0 : page.ID;
            }
            if (next != 0)
            {
                string query = HttpContext.Current.Request.QueryString.ToString();
                if (HttpContext.Current.Items.Contains(WorkFlowSetting.KEY_Query)) { query = (string)HttpContext.Current.Items[WorkFlowSetting.KEY_Query]; }
                if (!string.IsNullOrEmpty(query)) query = "?" + query;
                Response.Write(string.Format("<script language=\"javascript\" type=\"text/javascript\"> location.href = \"{0}.aspx{1}\"; </script>", next, query));
                Response.End();
            }

        }

        #endregion
    }
}
