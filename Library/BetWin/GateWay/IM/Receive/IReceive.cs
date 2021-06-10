using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BW.Agent;

namespace BW.GateWay.IM.Receive
{
    /// <summary>
    /// 收到的信息
    /// </summary>
    public abstract class IReceive
    {
        /// <summary>
        /// 发送信息的用户KEY值
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 发送信息的用户类型
        /// </summary>
        protected string UserType { get; private set; }

        /// <summary>
        /// 发送信息的用户ID
        /// </summary>
        protected int UserID { get; private set; }

        /// <summary>
        /// 收到的信息构造信息结构
        /// </summary>
        /// <param name="ht">客户端传递过来的数据</param>
        public IReceive(Hashtable ht)
        {
            if (ht.ContainsKey("ID"))
            {
                this.ID = ht["ID"].ToString();
                string userType;
                this.UserID = UserAgent.Instance().GetChatUserID(this.ID, out userType);
                this.UserType = userType;
            }

            foreach (DictionaryEntry item in ht)
            {
                PropertyInfo property = this.GetType().GetProperty(item.Key.ToString(), BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite) continue;

                switch (property.PropertyType.Name)
                {
                    case "String":
                        property.SetValue(this, item.Value.ToString());
                        break;
                    case "Int32":
                        property.SetValue(this, int.Parse(item.Value.ToString()));
                        break;
                    case "Int64":
                        property.SetValue(this, long.Parse(item.Value.ToString()));
                        break;
                }
            }
        }

        /// <summary>
        /// 收到信息后要执行的方法
        /// </summary>
        public virtual void Run()
        {

        }
    }
}
