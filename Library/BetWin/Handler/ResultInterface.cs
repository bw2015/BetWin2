using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Reflection;
using System.Net;
using System.Diagnostics;
using System.IO;

using BW.Agent;
using BW.PageBase;
using BW.Framework;
using SP.Studio.Web;

using SP.Studio.Core;
using SP.Studio.Model;

namespace BW.Handler
{
    /// <summary>
    /// 前台的调用基类
    /// </summary>
    public partial class ResultInterface : HandlerBase
    {
        /// <summary>
        /// 扩展的类库
        /// </summary>
        private static Dictionary<int, Assembly> extendAssembly = new Dictionary<int, Assembly>();

        public virtual void Invoke(HttpContext context, string agent, string methodName)
        {
            string typeName = "BW.Handler." + agent;
            Type type = this.GetType().Assembly.GetType(typeName);
            if (type == null)
            {
                type = this.GetExtendType(context, agent);
                if (type == null) this.ShowNotFound(context);
            }
            MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                Utils.ShowError(context, HttpStatusCode.MethodNotAllowed, string.Format("{0} {1}", type.FullName, methodName));
            }
            if (agent.StartsWith("admin") || agent.StartsWith("admin2"))
            {
                AdminAttribute adminPermission = method.GetAttribute<AdminAttribute>();
                if (adminPermission != null)
                {
                    if (AdminInfo == null) { context.Response.Write(false, "您没有登录"); }
                    if (!AdminInfo.HasPermission(adminPermission.Permission))
                    {
                        context.Response.Write(false, string.Format("您所在的角色{0}没有该操作权限{1}", AdminInfo.GroupInfo.Name, string.Join(",", adminPermission.Permission)));
                    }
                }
            }
            else
            {
                GuestAttribute guest = method.GetAttribute<GuestAttribute>();
                if (guest == null && UserInfo == null)
                {
                    context.Response.Write(false, "请先登录", new
                    {
                        Type = IHandler.ErrorType.Login
                    });
                }
            }

            method.Invoke(Activator.CreateInstance(type), new object[] { context });
        }

        /// <summary>
        /// 获取扩展的类型
        /// </summary>
        /// <param name="context"></param>
        /// <param name="agentName">admin.manage</param>
        /// <returns></returns>
        private Type GetExtendType(HttpContext context, string agent)
        {
            string typeName = string.Format("BW.S{0}.Handler.{1}", SiteInfo.ID, agent);

            if (extendAssembly.ContainsKey(SiteInfo.ID))
            {
                if (extendAssembly[SiteInfo.ID] == null) return null;
                return extendAssembly[SiteInfo.ID].GetType(typeName);
            }
            string path = context.Server.MapPath("~/bin/BetWin." + SiteInfo.ID + ".dll");
            if (!File.Exists(path))
            {
                extendAssembly.Add(SiteInfo.ID, null);
                return null;
            }
            byte[] data = File.ReadAllBytes(path);
            extendAssembly.Add(SiteInfo.ID, Assembly.Load(data));
            return extendAssembly[SiteInfo.ID].GetType(typeName);
        }

        /// <summary>
        /// 执行调用基类方法
        /// URL形式：/类库名字(简写)/方法名字.do
        /// </summary>
        /// <param name="context">只能被POST调用</param>
        public override void ProcessRequest(HttpContext context)
        {
            Regex regex = new Regex(@"/(?<Agent>[a-z]+)/(?<Type>[a-z0-9]+)/(?<Method>[a-z0-9]+)$");
            string url = context.Request.RawUrl;
            if (!regex.IsMatch(url))
            {
                this.ShowNotFound(context);
            }

            string agent = regex.Match(url).Groups["Agent"].Value;
            string type = regex.Match(url).Groups["Type"].Value;
            string method = regex.Match(url).Groups["Method"].Value;

            this.Invoke(context, string.Format("{0}.{1}", agent, type), method);
        }

        #region ==========  私有方法  ==============

        /// <summary>
        /// 显示找不到页面
        /// </summary>
        /// <param name="context"></param>
        protected override void ShowNotFound(HttpContext context)
        {
            Utils.ShowError(context, HttpStatusCode.NotFound);
        }

        #endregion
    }
}
