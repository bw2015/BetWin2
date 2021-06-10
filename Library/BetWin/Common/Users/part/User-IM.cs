using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Agent;

namespace BW.Common.Users
{
    partial class User
    {
        /// <summary>
        /// 用户的会话ID
        /// </summary>
        public string IMID
        {
            get
            {
                return string.Concat(UserAgent.IM_USER, "-", this.ID);
            }
        }
    }
}
