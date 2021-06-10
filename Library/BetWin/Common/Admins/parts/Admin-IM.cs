using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Agent;

namespace BW.Common.Admins
{
    partial class Admin
    {
        /// <summary>
        /// 管理员的会话ID
        /// </summary>
        public string IMID
        {
            get
            {
                return string.Concat(UserAgent.IM_ADMIN, "-", this.ID);
            }
        }
    }
}
