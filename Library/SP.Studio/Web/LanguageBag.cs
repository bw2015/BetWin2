using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;
using System.Resources;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using System.Timers;

using SP.Studio.Core;
using SP.Studio.IO;
using SP.Studio.Xml;
using SP.Studio.Model;
using SP.Studio.Configuration;

namespace SP.Studio.Web
{
    /// <summary>
    /// 語言包
    /// </summary>
    public class LanguageBag
    {
        public static readonly Dictionary<string, Dictionary<LanguagePack, string>> _languageList;

        /// <summary>
        /// 是否得到更新
        /// </summary>
        private static bool isUpdate = false;

        internal static readonly string file;

        static LanguageBag()
        {
            file = HttpContext.Current.Server.MapPath("~/App_Data/Language.xml");
            if (File.Exists(file))
            {
                _languageList = new Dictionary<string, Dictionary<LanguagePack, string>>();
                Dictionary<string, bool> languageType = typeof(LanguagePack).ToList().ToDictionary(t => t.Name, t => true);
                XElement root = XElement.Parse(File.ReadAllText(file, Encoding.UTF8));
                foreach (XElement item in root.Elements())
                {
                    string key = item.GetAttributeValue("key");
                    if (_languageList.ContainsKey(key)) continue;
                    Dictionary<LanguagePack, string> obj = new Dictionary<LanguagePack, string>();
                    foreach (XElement t in item.Elements())
                    {
                        string name = t.Name.ToString();
                        if (!languageType.ContainsKey(name)) continue;
                        LanguagePack type = name.ToEnum<LanguagePack>();
                        if (obj.ContainsKey(type)) continue;
                        obj.Add(type, t.Value);
                    }
                    _languageList.Add(key, obj);
                }

                Timer timer = new Timer(60 * 1000);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Save();
        }

        /// <summary>
        /// 保存至缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="language">如果為null則表示是修改Key</param>
        /// <returns>JSON字符串</returns>
        public static bool Save(string key, string value, LanguagePack type)
        {
            if (string.IsNullOrEmpty(key)) return false;
            if (!_languageList.ContainsKey(key)) _languageList.Add(key, new Dictionary<LanguagePack, string>());
            if (!_languageList[key].ContainsKey(type))
            {
                _languageList[key].Add(type, value);
            }
            else
            {
                _languageList[key][type] = value;
            }
            return isUpdate = true;
        }

        /// <summary>
        /// 修改Key值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public static bool Save(string key, string newKey)
        {
            Dictionary<LanguagePack, string> obj = null;
            if (_languageList.ContainsKey(key))
            {
                obj = _languageList[key];
                _languageList.Remove(key);
            }
            if (!string.IsNullOrEmpty(newKey) && !_languageList.ContainsKey(newKey))
            {
                _languageList.Add(newKey, obj ?? new Dictionary<LanguagePack, string>());
            }
            return true;
        }

        /// <summary>
        /// 保存进入文件
        /// </summary>
        internal static bool Save()
        {
            if (!isUpdate) return false;
            XElement root = new XElement("root");
            foreach (KeyValuePair<string, Dictionary<LanguagePack, string>> item in _languageList)
            {
                string key = item.Key;
                XElement languageItem = new XElement("item");
                languageItem.SetAttributeValue("key", key);
                foreach (KeyValuePair<LanguagePack, string> type in item.Value)
                {
                    languageItem.Add(new XElement(type.Key.ToString(), type.Value));
                }
                root.Add(languageItem);
            }
            FileAgent.Write(file, root.ToString(), Encoding.UTF8, false);
            return true;
        }

        /// <summary>
        /// 删除一个Key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static bool Delete(string key)
        {
            if (_languageList.ContainsKey(key))
            {
                _languageList.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取语言包的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <param name="create">是否创建一个空的记录</param>
        /// <returns></returns>
        public static string Get(string key, LanguagePack type, bool create = false)
        {
            if (_languageList == null) return key;
            if (!_languageList.ContainsKey(key))
            {
                if (create) _languageList.Add(key, new Dictionary<LanguagePack, string>());
                return key;
            }
            Dictionary<LanguagePack, string> dic = _languageList[key];
            if (!dic.ContainsKey(type)) return key;
            return dic[type];
        }

            }

    /// <summary>
    /// 語言包編輯頁面
    /// </summary>
    public class LanguageHandler : IHttpHandler
    {

        #region IHttpHandler 成员

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            StringBuilder sb = new StringBuilder();
            LanguagePack language = WebAgent.QF("Language").ToEnum<LanguagePack>();
            string key = WebAgent.QF("Key");
            string value = WebAgent.QF("Value");
            ResourceManager rm = new ResourceManager(typeof(SP.Studio.Files.Language));
            context.Response.ContentType = "json/application";
            switch (WebAgent.QS("ac"))
            {
                case "save":
                    context.Response.Write(LanguageBag.Save(key, value, language), "");
                    break;
                case "delete":
                    context.Response.Write(LanguageBag.Delete(key), "删除KEY");
                    break;
                case "key":
                    context.Response.Write(LanguageBag.Save(key, value), "");
                    break;
                case "file":
                    bool fileSave = LanguageBag.Save();
                    context.Response.Write(LanguageBag.Save(), "保存入文件");
                    break;
                case "list":
                    List<string> json = new List<string>();
                    foreach (var item in LanguageBag._languageList)
                    {
                        json.Add(string.Format("\"{0}\":{1}", item.Key, item.Value.ToJson()));
                    }
                    context.Response.Write(true, "语言包", new
                    {
                        data = new JsonString(typeof(LanguagePack).ToJson()),
                        list = new JsonString(string.Format("{{{0}}}", string.Join(",", json)))
                    });
                    break;
                case "js":
                    context.Response.ContentType = "text/javascript";
                    sb.Append(rm.GetObject("js"));
                    break;
                case "css":
                    context.Response.ContentType = "text/css";
                    sb.Append(rm.GetObject("css"));
                    break;
                default:
                    context.Response.ContentType = "text/html";
                    sb.Append(rm.GetObject("page"));
                    break;
            }
            context.Response.Write(sb.ToString());
        }

        #endregion
    }
}
