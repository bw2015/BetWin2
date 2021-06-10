using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.Agent;
using BW.Common.Users;

using SP.Studio.Model;

namespace BW.Handler
{
    partial class ResultInterface
    {
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="context"></param>
        private void User_Register(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().Register(QF("UserName"), QF("Password"), QF("Invite")), "注册成功");
        }
    }
}
