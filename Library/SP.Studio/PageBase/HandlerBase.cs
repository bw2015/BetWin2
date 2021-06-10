using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;

using SP.Studio.Web;
using SP.Studio.Core;
using SP.Studio.Model;

namespace SP.Studio.PageBase
{
    public abstract class HandlerBase : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public virtual void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 通过反射执行方法
        /// </summary>
        /// <param name="actionNameKey">登录传递过来的参数或者自身。 如果是自身则需要区分大小写</param>
        protected virtual void Invoke(HttpContext context, string actionNameKey = "ac")
        {
            string ac = WebAgent.GetParam(actionNameKey);
            MethodInfo method = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(t => t.Name.Equals(ac, StringComparison.InvariantCultureIgnoreCase) || t.Name.Equals(actionNameKey, StringComparison.InvariantCultureIgnoreCase));
            if (method == null)
            {
                this.ShowNotFound(context);
                return;
            }
            method.Invoke(this, new object[] { context });
        }

        /// <summary>
        /// 获取分页数据（来自于linq数据源）
        /// </summary>
        protected virtual string getListResult<T>(IOrderedQueryable<T> list, System.Converter<T, object> convert = null, int pageSize = 10)
        {
            if (convert == null) convert = t => t;
            int pageIndex = WebAgent.GetParam("PageIndex", 1);
            pageSize = WebAgent.GetParam("PageSize", pageSize);
            int recordCount = list.Count();
            return new
            {
                pageIndex = pageIndex,
                pageSize = pageSize,
                recordCount = recordCount,
                list = new JsonString(list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().ConvertAll(t => convert(t)).ToJson())
            }.ToJson();
        }

        /// <summary>
        /// 显示错误信息并且终止页面执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        protected virtual void ShowError(HttpContext context, string msg, object obj = null)
        {
            context.Response.Write(new Result(false, msg, obj));
            context.Response.End();
        }

        protected virtual void ShowNotFound(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Write("404 Not Found");
            context.Response.End();
        }

          public virtual bool IsReusable
        {
            get { return false; }
        }
    }
}
