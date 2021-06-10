using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IMService.Framework;
using IMService.Common;

using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Json;
using SP.Studio.Array;

namespace IMService.Rebot
{
    /// <summary>
    /// 图灵机器人
    /// </summary>
    public class tuling
    {
        private const string API = "http://www.tuling123.com/openapi/api";

        private const string KEY = "4284a75cef0a4d06819d24a7db5e534b";

        /// <summary>
        /// 获取机器人返回值
        /// </summary>
        /// <param name="info">发送过来的信息</param>
        /// <param name="userid">客户端标识</param>
        /// <returns></returns>
        public static string GetResult(string info, string userid)
        {
            // 关键词检测
            if (SysSetting.GetSetting().Key.Length != 0)
            {
                foreach (Keyword keyword in SysSetting.GetSetting().Key)
                {
                    foreach (string key in keyword.Key)
                    {
                        if (info.Contains(key))
                        {
                            return keyword.Content;
                        }
                    }
                }
            }

            string data = string.Format("key={0}&info={1}&userid={2}", KEY, info, userid);
            string result = NetAgent.UploadData(API, data, Encoding.UTF8);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null) return result;
            result = ht.GetValue("text", result);
            if (result.Contains("REBOTNAME")) result = result.Replace("REBOTNAME", SysSetting.GetSetting().SiteInfo.Rebot.Name);
            if (result.Contains("北京光年无限科技有限公司")) result = string.Format("我们平台叫{0}", SysSetting.GetSetting().SiteInfo.Name);
            return result;
        }
    }
}
