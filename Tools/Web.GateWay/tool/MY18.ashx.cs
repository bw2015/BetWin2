using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.GateWay.tool
{
    /// <summary>
    /// MY18 的摘要说明
    /// </summary>
    public class MY18 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            if (context.Request.Form["a"] == null)
            {
                context.Response.Write("false");
            }
            else
            {
                context.Response.Write("<Cad-rc-ok>");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}