using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace BW.IM.Common
{
    /// <summary>
    /// 中奖通知
    /// </summary>
    public class Notify
    {
        public Notify(DataRow dr)
        {
            this.UserID = (int)dr["UserID"];
            this.ID = (int)dr["NotifyID"];
            this.User = string.Format("{0}-{1}", UserType.USER, this.UserID);
            this.Message = (string)dr["Message"];
        }

        /// <summary>
        /// 通知编号
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 要通知的用户Key
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
    }
}
