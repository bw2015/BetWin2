using System;
using System.Web;
using System.Data;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Resources;

using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Web;
using SP.Studio.WorkFlow;

namespace SP.Studio.WorkFlow
{
    /// <summary>
    /// <add type="SP.Studio.WorkFlow.WorkFlowHandler,SP.Studio" verb="*" path="/workflow.axd"/>
    /// </summary>
    public class WorkFlowHandler : IHttpHandler
    {

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.HttpMethod == "GET")
            {
                switch (WebAgent.QS("type"))
                {
                    case "test":
                        context.Response.Write(WorkFlowSetting.WorkFlow.ToXmlString());
                        break;
                    case "style":
                        this.Style(context);
                        break;
                    case "image":
                        this.Image(context);
                        break;
                    case "script":
                        this.JScript(context);
                        break;
                    case "html":
                        this.Html(context);
                        break;
                    case "create":
                        this.Create();
                        break;
                    case "cache":
                        this.Cache();
                        break;
                    case "methods":
                        this.Methods(context);
                        break;
                    default:
                        this.Main(context);
                        break;
                }
            }
            else
            {
                this.Handler(context);
            }
        }

        /// <summary>
        /// 获取当前类下面可供工作流使用的方法列表
        /// </summary>
        private void Methods(HttpContext context)
        {
            WorkGroup group = WorkFlowSetting.WorkFlow.GroupList.Find(t => t.ID == WebAgent.QS("GroupID", 0));
            if (group == null) { context.Response.Write("找不到组ID：" + WebAgent.QS("GroupID", 0)); return; }
            Assembly assembly = Assembly.Load(group.Assembly);
            if (assembly == null) { context.Response.Write("组的资源设置错误"); return; }
            Type type = assembly.GetType(WebAgent.QS("Class"));
            if (type == null) { context.Response.Write("找不到类:" + WebAgent.QS("Class")); return; }
            context.Response.Write((from o in type.GetMethods(BindingFlags.Static | BindingFlags.Public) select new { o.Name, Description = o.GetDescription() }).ToList().ToJson(t => t.Name, t => t.Description));
        }

        /// <summary>
        /// 工作流管理首页
        /// </summary>
        private void Main(HttpContext context)
        {
            WorkFlowSetting.Install();

            context.Response.ContentType = "text/html";
            ResourceManager rm = new ResourceManager(typeof(SP.Studio.Files.WorkFlow));
            context.Response.Write((string)rm.GetObject("workflow"));
        }

        /// <summary>
        /// 样式文件
        /// </summary>
        /// <param name="context"></param>
        private void Style(HttpContext context)
        {
            context.Response.ContentType = "text/css";
            ResourceManager rm = new ResourceManager(typeof(SP.Studio.Files.WorkFlow));
            context.Response.Write((string)rm.GetObject("StyleSheet"));
        }

        /// <summary>
        /// 显示图片
        /// </summary>
        /// <param name="context"></param>
        private void Image(HttpContext context)
        {
            ResourceManager rm = new ResourceManager(typeof(SP.Studio.Files.WorkFlow));
            string src = WebAgent.QS("src");
            string image = src.Substring(0, src.LastIndexOf('.'));
            string type = src.Substring(src.LastIndexOf('.') + 1);
            System.Drawing.Bitmap obj = (System.Drawing.Bitmap)rm.GetObject(image);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                context.Response.ContentType = "image/" + type;
                obj.Save(ms, obj.RawFormat);
                context.Response.BinaryWrite(ms.ToArray());
            }
        }

        /// <summary>
        /// 窗体文件
        /// </summary>
        private void Html(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            ResourceManager rm = new ResourceManager(typeof(SP.Studio.Files.WorkFlow));
            string src = WebAgent.QS("src").ToLower();
            src = src.Substring(0, 1).ToUpper() + src.Substring(1);
            context.Response.Write((string)rm.GetObject(src));
        }

        /// <summary>
        /// 脚本文件
        /// </summary>
        private void JScript(HttpContext context)
        {
            context.Response.ContentType = "text/script";
            ResourceManager rm = new ResourceManager(typeof(SP.Studio.Files.WorkFlow));
            context.Response.Write((string)rm.GetObject("JScript"));
        }

        /// <summary>
        /// 创建xml文件并且刷新缓存
        /// </summary>
        private void Create()
        {
            WorkAgent.CreateConfigurationFile(); 
            WorkFlowSetting.LoadWorkFlowCache();
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        private void Cache()
        {
            WorkFlowSetting.LoadWorkFlowCache();
        }

        private void Handler(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            StringBuilder sb = new StringBuilder();
            Position position;

            switch (WebAgent.QS("ac"))
            {
                case "group":
                    context.Response.Write(WorkFlowSetting.WorkFlow.GroupList.ToJsonString());
                    break;
                case "workflow":
                    int groupID = WebAgent.QS("GroupID", 0);
                    sb.AppendLine(WorkFlowSetting.WorkFlow.PageList.FindAll(t=>t.GroupID == groupID).ToJsonString());
                    sb.AppendLine(WorkFlowSetting.WorkFlow.EventList.FindAll(t=>t.GroupID == groupID).ToJsonString());
                    sb.AppendLine(WorkFlowSetting.WorkFlow.ResultList.FindAll(t => t.GroupID == groupID).ToJsonString());
                    sb.AppendLine(WorkFlowSetting.WorkFlow.LineList.FindAll(t => t.GroupID == groupID).ToJsonString());
                    context.Response.Write(sb.ToString());
                    break;
                case "create":  //创建一个元素
                    position = new Position() { x = WebAgent.QF("Position[x]", 0), y = WebAgent.QF("Position[y]", 0) };
                    switch (WebAgent.QF("Genre"))
                    {
                        case "page":
                            WorkPage workPage = new WorkPage() { GroupID = WebAgent.QF("GroupID", 0), Position = position };
                            workPage.Add();
                            context.Response.Write(workPage.ToJsonString());
                            break;
                        case "event":
                            WorkEvent workEvent = new WorkEvent() { GroupID = WebAgent.QF("GroupID", 0), Position = position };
                            workEvent.Add();
                            context.Response.Write(workEvent.ToJsonString());
                            break;
                        case "result":
                            WorkResult workResult = new WorkResult() { GroupID = WebAgent.QF("GroupID", 0), Position = position };
                            workResult.Add();
                            context.Response.Write(workResult.ToJsonString());
                            break;
                        case "line":
                            position = new Position() { x1 = WebAgent.QF("Position[x1]", 0), y1 = WebAgent.QF("Position[y1]", 0), x2 = WebAgent.QF("Position[x2]", 0), y2 = WebAgent.QF("Position[y2]", 0) };
                            WorkLine workLine = new WorkLine() { GroupID = WebAgent.QF("GroupID", 0), Position = position };
                            workLine.Add();
                            context.Response.Write(workLine.ToJsonString());
                            break;
                    }
                    break;
                case "update":  //修改元素的坐标点
                    position = new Position() { x = WebAgent.QF("Position[x]", 0), y = WebAgent.QF("Position[y]", 0) };
                    switch (WebAgent.QF("Genre"))
                    {

                        case "page":
                            WorkPage workPage = new WorkPage() { ID = WebAgent.QF("ID", 0), Position = position };
                            workPage.Update<WorkPage>(t => t.Position);
                            break;
                        case "event":
                            WorkEvent workEvent = new WorkEvent() { ID = WebAgent.QF("ID", 0), Position = position };
                            workEvent.Update<WorkEvent>(t => t.Position);
                            break;
                        case "result":
                            WorkResult workResult = new WorkResult() { ID = WebAgent.QF("ID", 0), Position = position };
                            workResult.Update<WorkResult>(t => t.Position);
                            break;
                        case "line":
                            position = new Position() { x1 = WebAgent.QF("Position[x1]", 0), y1 = WebAgent.QF("Position[y1]", 0), x2 = WebAgent.QF("Position[x2]", 0), y2 = WebAgent.QF("Position[y2]", 0) };
                            WorkLine workLine = new WorkLine() { ID = WebAgent.QF("ID", 0), PageID = WebAgent.QF("PageID", 0), EventID = WebAgent.QF("EventID", 0), ResultID = WebAgent.QF("ResultID", 0), Position = position };
                            workLine.Update<WorkLine>(t => t.Position, t => t.PageID, t => t.EventID, t => t.ResultID);
                            break;
                    }
                    break;
                case "save":
                    switch (WebAgent.QF("Genre"))
                    {
                        case "page":
                            WorkPage workPage = new WorkPage() { ID = WebAgent.QF("ID", 0), Name = WebAgent.QF("Name"), Description = WebAgent.QF("Description"), Url = WebAgent.QF("Url") };
                            workPage.Update<WorkPage>(t => t.Name, t => t.Description, t => t.Url);
                            context.Response.Write(workPage.ToJsonString());
                            break;
                        case "event":
                            WorkEvent workEvent = new WorkEvent()
                            {
                                ID = WebAgent.QF("ID", 0),
                                Name = WebAgent.QF("Name"),
                                Description = WebAgent.QF("Description"),
                                Type = WebAgent.QF("Type"),
                                Method = WebAgent.QF("Method"),
                                Params = string.IsNullOrEmpty(WebAgent.QF("Params")) ? null : WebAgent.QF("Params").Split(',')
                            };
                            workEvent.Update<WorkEvent>(t => t.Name, t => t.Description, t => t.Type, t => t.Method, t => t.Params);
                            break;
                        case "result":
                            WorkResult workResult = new WorkResult()
                            {
                                ID = WebAgent.QF("ID", 0),
                                Name = WebAgent.QF("Name"),
                                Description = WebAgent.QF("Description"),
                                Type = WebAgent.QF("Type"),
                                Method = WebAgent.QF("Method"),
                                Params = WebAgent.QF("Params").Split(','),
                                Next = WebAgent.QF("Next", 0)
                            };
                            workResult.Update<WorkResult>(t => t.Name, t => t.Description, t => t.Type, t => t.Method, t => t.Params, t => t.Next);
                            break;
                    }
                    break;
                case "delete":
                    switch (WebAgent.QF("Genre"))
                    {
                        case "page":
                            new WorkPage() { ID = WebAgent.QF("ID", 0) }.Delete();
                            break;
                        case "event":
                            new WorkEvent() { ID = WebAgent.QF("ID", 0) }.Delete();
                            break;
                        case "result":
                            new WorkResult() { ID = WebAgent.QF("ID", 0) }.Delete();
                            break;
                        case "line":
                            new WorkLine() { ID = WebAgent.QF("ID", 0) }.Delete();
                            break;
                    }
                    break;
            }
        }
    }
}
