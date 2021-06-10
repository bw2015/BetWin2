using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Agent;

namespace BW.Common.Users
{
    partial class UserRemark
    {
        public override string ToString()
        {
            return string.Format("管理员【{0}】于{1}备注：{2}",
                AdminAgent.Instance().GetAdminName(this.AdminID), this.CreateAt.ToString("yyyy-MM-dd HH:mm"), this.Content);
        }

        public string AdminName
        {
            get
            {
                return AdminAgent.Instance().GetAdminName(this.AdminID);
            }
        }
    }
}
