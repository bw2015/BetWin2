using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Json
{
    public static class JsonAgent
    {
        /// <summary>
        /// 把json格式的单一对象转换成为哈希表
        /// </summary>
        public static Hashtable GetJObject(string json)
        {
            Hashtable ht = new Hashtable();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                foreach (KeyValuePair<string, JToken> keyValue in obj)
                {
                    ht.Add(keyValue.Key, keyValue.Value);
                }
            }
            catch
            {
                ht = null;
            }

            return ht;
        }

        /// <summary>
        /// json对象转化成为表
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static IDictionary<T1, T2> GetDictionary<T1, T2>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<T1, T2>>(json);
        }

        /// <summary>
        /// 把一个json数组对象转化成为hashlist
        /// </summary>
        public static Hashtable[] GetJList(string json)
        {
            Hashtable[] list;
            try
            {
                JArray obj = (JArray)JsonConvert.DeserializeObject(json);
                list = new Hashtable[obj.Count];
                for (int i = 0; i < list.Length; i++)
                {
                    list[i] = new Hashtable();
                    foreach (KeyValuePair<string, JToken> keyValue in (JObject)obj[i])
                    {
                        list[i].Add(keyValue.Key, keyValue.Value);
                    }
                }

            }
            catch
            {
                list = null;
            }
            return list;
        }

        /// <summary>
        /// 获取json的某个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T GetValue<T>(string json, params string[] path)
        {
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            return obj.GetValue<T>(path);
        }

        public static T GetValue<T>(this JObject json, params string[] path)
        {
            for (int i = 0; i < path.Length; i++)
            {
                string name = path[i];
                if (json[name] == null) return default(T);
                if (json[name].GetType() == typeof(JValue))
                {
                    if (string.IsNullOrEmpty(json.Value<string>(name))) return default(T);
                    return json.Value<T>(name);
                }
                else
                {
                    json = (JObject)json[name];
                }
            }
            return default(T);
        }

        /// <summary>
        /// 获取json的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetValue<T>(this JObject item, string key, T defaultValue)
        {
            if (item[key] == null) return defaultValue;
            if (item[key] == JValue.CreateNull()) return default(T);
            return item[key].Value<T>();
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 把Dic转化成为JSON字符串
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string GetJson<T1, T2>(IDictionary<T1, T2> dic)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(dic);
        }

        /// <summary>
        /// 把JObject(key-value)类型，转化成为Dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this JObject obj)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (KeyValuePair<string, JToken> item in obj)
            {
                Console.WriteLine(item.Value.Type);
                switch (item.Value.Type)
                {
                    case JTokenType.Array:
                        break;
                    case JTokenType.Null:
                        data.Add(item.Key, null);
                        break;
                    default:
                        data.Add(item.Key, item.Value.ToString());
                        break;
                }
            }
            return data;
        }
    }
}
