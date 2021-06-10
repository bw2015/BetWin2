using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;

using SP.Studio.Core;

namespace BW.IM.Common
{
    /// <summary>
    /// 
    /// </summary>
    public enum CommandMessage
    {
        /// <summary>
        /// 非命令
        /// </summary>
        None,
        /// <summary>
        /// 禁言
        /// </summary>
        [Description(@"^@BLOCK:(?<Value>\d+)$")]
        BLOCK
    }

    /// <summary>
    /// 命令的缓存结构体
    /// </summary>
    public struct Command
    {
        public Command(CommandMessage type, int value)
        {
            this.Type = type;
            this.Value = value;
        }

        public CommandMessage Type;

        public int Value;


    }
}
