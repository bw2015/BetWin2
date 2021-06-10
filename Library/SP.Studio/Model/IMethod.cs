using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SP.Studio.Web;

namespace SP.Studio.Model
{
    /// <summary>
    /// 用于工具类的执行接口
    /// </summary>
    public interface IMethod
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="args"></param>
        void Run(string[] args);
    }

    /// <summary>
    /// 工具类
    /// </summary>
    public static class MethodUtils
    {
        /// <summary>
        /// 多线程
        /// </summary>
        public const string TASK = "task";

        /// <summary>
        /// 指定编码
        /// </summary>
        public const string ENCODING = "encoding";

        /// <summary>
        /// 抓取网页的时候不显示内容
        /// </summary>
        public const string HIDECONTENT = "hidecontent";

        /// <summary>
        /// 自定义cookie
        /// </summary>
        public const string COOKIE = "cookie";

        /// <summary>
        /// 获取参数类型
        /// </summary>
        /// <param name="action">-action[Value] 格式</param>
        /// <returns></returns>
        public static string Get(this string[] args, string action)
        {
            string arg = args.Where(t => t.StartsWith("-" + action)).FirstOrDefault();
            if (string.IsNullOrEmpty(arg)) return null;

            return arg.Substring(action.Length + 1);
        }

        public static T Get<T>(this string[] args, string action, T defaultValue) where T : struct
        {
            string value = args.Get(action);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            if (!WebAgent.IsType<T>(value)) return defaultValue;
            return (T)SP.Studio.Core.ObjectExtensions.GetValue(value, typeof(T));
        }
    }
}
