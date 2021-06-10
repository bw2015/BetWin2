using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using SP.Studio.Core;
using SP.Studio.IO;

namespace SP.Studio.Web
{
    /// <summary>
    /// 与脚本交互的方法
    /// </summary>
    public class JSAgent
    {
        /// <summary>
        /// 获取资源内的枚举解释
        /// </summary>
        /// <param name="jsFile">保存到的js文件</param>
        public static string BuildEnumScript(Assembly assembly,string jsFile,Encoding encoding = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("if (!window[\"Enum\"]) window[\"Enum\"] = new Object();");
            assembly.GetTypes().Where(t => t.IsEnum).ToList().ForEach(t =>
            {
                foreach (var name in Enum.GetNames(t))
                {
                    Enum em = (Enum)Enum.Parse(t, name);
                    sb.AppendFormat("Enum[\"{0}_{1}\"] = \"{2}\";", t.Name, name, em.GetDescription());
                }
            });
            if (!string.IsNullOrEmpty(jsFile))
            {
                sb.AppendLine();
                FileAgent.Write(jsFile, sb.ToString(), encoding, false);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将一个列队作为js输出
        /// </summary>
        public static string BuildListScript<T>(List<T> list, string name, Func<T, object> key, string jsFile = null, Encoding encoding = null, bool isAppend = true) where T : class
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(name)) name = typeof(T).Name;
            sb.AppendFormat("if (!window[\"{0}\"]) window[\"{0}\"] = new Object();", name);
            foreach (T t in list)
            {
                sb.AppendFormat("{0}[\"{1}\"] = {2};", name, key.Invoke(t), t.ToJson());
            }
            if (!string.IsNullOrEmpty(jsFile))
            {
                sb.AppendLine();
                FileAgent.Write(jsFile, sb.ToString(), encoding, isAppend);
            }
            return sb.ToString();
        }
    }
}
