using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.IM.Common;

namespace BW.IM.Factory.Command
{
    public abstract class ICommand
    {
        protected User UserInfo { get; private set; }

        protected int Value { get; private set; }

        public ICommand(User user, int value)
        {
            this.UserInfo = user;
            this.Value = value;
        }
    }
}
