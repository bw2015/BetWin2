using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SP.Studio.Web
{
    public static class ContextExtendsions
    {
        /// <summary>
        /// 显示错误页面
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statusCode"></param>
        /// <param name="error"></param>
        public static void ShowError(this HttpContext context, HttpStatusCode statusCode, string error = null)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "text/html";
            string status = Regex.Replace(statusCode.ToString(), "[A-Z]", t =>
            {
                return " " + t.Value;
            });

            if (string.IsNullOrEmpty(error))
            {
                error = status;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head><title>{0} {1}</title><meta name=\"description\" content=\"{2}\" /><body><h1><center>{0} {1}</center></h1>", (int)statusCode, status, error);
            sb.Append("<hr />");
            sb.AppendFormat("<center>{0}/{1}</center>", "BetWin", typeof(ContextExtendsions).Assembly.GetName().Version);
            sb.Append("</body></html>");
            context.Response.Write(sb);
            context.Response.End();
        }

        /// <summary>
        /// 显示自定义的错误信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        public static void ShowError(this HttpContext context, ErrorType type, string msg = null)
        {
            if (string.IsNullOrEmpty(msg)) msg = type.GetDescription();
            context.Response.Write(false, msg, new
            {
                ErrorType = type
            });
        }
    }
}
