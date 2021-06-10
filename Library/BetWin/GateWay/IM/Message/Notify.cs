using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Core;
namespace BW.GateWay.IM.Message
{
    /// <summary>
    /// 通知类型
    /// </summary>
    public class Notify : IMessage
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 可被转换成为json的内容
        /// </summary>
        public JsonString Content { get; set; }


    }
}
