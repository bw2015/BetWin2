using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Web.SessionState;

using System.IO;

namespace Web.GateWay
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            string url = app.Request.RawUrl;
            Regex regex = new Regex(@"^/(?<ID>[a-zA-Z0-9]{2,12})$|^/wx/(?<ID>[a-zA-Z0-9]{4,6})$");
            if (regex.IsMatch(url))
            {
                string inviteId = regex.Match(url).Groups["ID"].Value;
                app.Context.RewritePath("~/Handler/Invite.ashx?ID=" + inviteId);
            }


            if (Regex.IsMatch(url, @"^/(wx|mobile|mobile3|pc)/\d{4}\w{3}$|^/([0-9A-F]{32})$", RegexOptions.IgnoreCase))
            {
                app.Context.RewritePath("~/Handler/Redirect.ashx");
            }

            if (Regex.IsMatch(url, @"^/wxapi/\d{4}"))
            {
                app.Context.RewritePath("~/Handler/Wechat.ashx");
            }

            if (Regex.IsMatch(url, @"^/payment/\w+"))
            {
                app.Context.RewritePath("~/Payment.ashx");
            }

            if (Regex.IsMatch(url, @"^/scripts/ghost"))
            {
                app.Context.RewritePath("~/GHost.ashx");
            }

            if (Regex.IsMatch(url, @"^/app/\w+$"))
            {
                app.Context.RewritePath("~/Handler/App.ashx");
            }

            if (Regex.IsMatch(url, @"^/plist/\w+$"))
            {
                app.Context.RewritePath("~/Handler/PList.ashx");
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}