using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

using Web.GateWay.App_Code;

namespace Web.GateWay
{
    /// <summary>
    /// LotteryResult 的摘要说明
    /// </summary>
    public class LotteryResult : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/xml";
            switch (context.Request.HttpMethod)
            {
                case "POST":
                    this.Save(context);
                    break;
                default:
                    this.Show(context);
                    break;
            }
        }

        private void Save(HttpContext context)
        {
            string ip = context.Request.UserHostAddress;

            string type = context.Request.QueryString["Type"];
            // 系统彩标识
            string siteId = context.Request.QueryString["Key"];

            if (string.IsNullOrEmpty(type)) return;

            int count = 0;
            using (ResultAgent resultAgent = new ResultAgent())
            {
                foreach (string key in context.Request.Form.AllKeys.OrderBy(t => t))
                {
                    if (resultAgent.Save(type, key, context.Request.Form[key], siteId))
                    {
                        count++;
                    }
                }
            }
            context.Response.Write(count);
        }

        private void Show(HttpContext context)
        {
            XElement root = new XElement("root");
            string type = context.Request.QueryString["Type"];
            if (!string.IsNullOrEmpty(type) && ResultAgent.data.ContainsKey(type))
            {
                SortedDictionary<string, int> dic = ResultAgent.data[type];
                root.SetAttributeValue("count", dic.Count);
                foreach (KeyValuePair<string, int> keyValue in dic)
                {
                    XElement item = new XElement(type.ToString());
                    item.SetAttributeValue("key", keyValue.Key);
                    item.SetAttributeValue("value", keyValue.Value);
                    root.Add(item);
                }
            }
            context.Response.Write(root);
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