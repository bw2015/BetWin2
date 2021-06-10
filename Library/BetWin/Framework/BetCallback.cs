using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.Agent;

namespace BW.Framework
{
    /// <summary>
    /// 外部设定的回调方法
    /// </summary>
    public class BetCallback
    {
        /// <summary>
        /// 充值回调
        /// </summary>
        private static Dictionary<string, MethodCallback> _callback = new Dictionary<string, MethodCallback>();


        /// <summary>
        /// 可供外部事件调用的委托
        /// </summary>
        /// <param name="args"></param>
        public delegate void MethodCallback(params object[] args);

        /// <summary>
        /// 添加一个回调方法
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="method"></param>
        public static void AddCallback(int siteId, CallbackType type, MethodCallback method)
        {
            string key = string.Concat(siteId, "-", type);
            if (!_callback.ContainsKey(key))
            {
                _callback.Add(key, method);
            }
            else
            {
                _callback[key] = method;
            }
        }

        /// <summary>
        /// 获取一个回调方法
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static void GetCallback(int siteId, CallbackType type, params object[] args)
        {
            string key = string.Concat(siteId, "-", type);
            if (!_callback.ContainsKey(key)) return;

            try
            {
                _callback[key].Invoke(args);
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(siteId, ex, string.Format("回调出错 SiteID={0} Type={1} {2}", siteId, type, string.Join(",", args)));
            }
        }

        public enum CallbackType
        {
            /// <summary>
            /// 充值
            /// </summary>
            Recharge
        }

    }
}
