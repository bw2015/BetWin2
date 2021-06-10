using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SP.Studio.Permission.Model
{
    public enum AdminStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("禁止登录")]
        Stop = 1
    }
}
