using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SP.Studio.PageBase
{
    /// <summary>
    /// API输出的基类（只有方法，未继承 IHttpHandler)
    /// </summary>
    public abstract class IHandlerBase : System.Web.SessionState.IRequiresSessionState
    {
        public const string MESSAGE = "MESSAGE";

        /// <summary>
        /// 当前的http对象
        /// </summary>
        protected virtual HttpContext context
        {
            get
            {
                return HttpContext.Current;
            }
        }

        /// <summary>
        /// 当前使用的定时器名字
        /// </summary>
        protected virtual string STOPWATCH { get { return "STOPWATCH"; } }

        protected virtual string StopwatchMessage()
        {
            if (!this.context.Items.Contains(this.STOPWATCH)) return "0ms";
            Stopwatch sw = (Stopwatch)this.context.Items[this.STOPWATCH];
            return string.Concat(sw.ElapsedMilliseconds, "ms");
        }

        protected virtual string QF(string name)
        {
            return WebAgent.QF(name);
        }

        protected virtual T QF<T>(string name, T defaultValue)
        {
            return WebAgent.QF<T>(name, defaultValue);
        }

        /// <summary>
        /// 通过正则表达式获取内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="regex"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected virtual IEnumerable<T> QF<T>(Regex regex, T defaultValue)
        {
            return WebAgent.QF<T>(regex, defaultValue);
        }

        protected virtual int PageIndex
        {
            get
            {
                return Math.Max(1, QF("PageIndex", 1));
            }
        }

        protected virtual int PageSize
        {
            get
            {
                return Math.Max(1, QF("PageSize", 20));
            }
        }

        /// <summary>
        /// 获取 Key[Value] 格式的内容
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual IEnumerable<T> QFS<T>(string key, T defaultValue)
        {
            Regex regex = new Regex(string.Format(@"^{0}\[(?<Value>[^\]]+)\]", key));
            foreach (string item in this.context.Request.Form.AllKeys)
            {
                if (!regex.IsMatch(item)) continue;
                yield return this.QF(item, defaultValue);
            }
        }

        protected T Show<T>(T? value) where T : struct
        {
            if (value == null) return default(T);
            return (T)value;
        }

        #region ========= ShowResult  ============

        protected virtual Result GetResult(bool result, string successMessage = "执行成功", object info = null)
        {
            if (result)
            {
                return new Result(true, successMessage, info);
            }
            else
            {
                return new Result(false, (string)context.Items[MESSAGE] ?? "发生不可描叙的错误");
            }
        }

        protected virtual void ShowResult(bool result, string successMessage = "执行成功", object info = null)
        {
            context.Response.Write(this.GetResult(result, successMessage, info));
            context.Response.End();
        }

        /// <summary>
        /// 返回列表的json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="data">扩展要显示内容。 如果要自定义文本内容使用JsonString装箱</param>
        /// <returns></returns>
        protected string ShowResult<T, TOutput>(IOrderedQueryable<T> list, Converter<T, TOutput> converter = null, Object data = null) where TOutput : class
        {
            List<T> queryList;
            return this.ShowResult(list, out queryList, converter, data);
        }

        protected string ShowResult<T, TOutput>(IOrderedQueryable<T> list, out List<T> queryList, Converter<T, TOutput> converter = null, Object data = null) where TOutput : class
        {
            if (converter == null) converter = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string json = null;
            if (this.PageIndex == 1)
            {
                queryList = list.Take(this.PageSize).ToList();
                json = queryList.ConvertAll(converter).ToJson();
            }
            else
            {
                queryList = list.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList();
                json = queryList.ConvertAll(converter).ToJson();
            }
            sb.Append("{")
                .AppendFormat("\"RecordCount\":{0},", list.Count())
                .AppendFormat("\"PageIndex\":{0},", this.PageIndex)
                .AppendFormat("\"PageSize\":{0},", this.PageSize)
                .AppendFormat("\"data\":{0}", data == null ? "null" : data.ToJson())
                .AppendFormat(",\"list\":{0}", json)
                .Append("}");

            return sb.ToString();
        }

        protected string ShowResult<T, TOutput>(IOrderedEnumerable<T> list, Converter<T, TOutput> converter = null, Object data = null) where TOutput : class
        {
            if (converter == null) converter = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string json = null;
            List<T> resultList;
            if (this.PageIndex == 1)
            {
                resultList = list.Take(this.PageSize).ToList();
                json = resultList.ConvertAll(converter).ToJson();
            }
            else
            {
                resultList = list.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList();
                json = resultList.ConvertAll(converter).ToJson();
            }
            sb.Append("{")
                .AppendFormat("\"RecordCount\":{0},", list.Count())
                .AppendFormat("\"PageIndex\":{0},", this.PageIndex)
                .AppendFormat("\"PageSize\":{0},", this.PageSize)
                .AppendFormat("\"data\":{0}", data == null ? "null" : data.ToJson())
                .AppendFormat(",\"list\":{0}", json)
                .Append("}");

            return sb.ToString();
        }

        protected string ShowResult<T, TOutput>(IEnumerable<T> list, Converter<T, TOutput> converter = null, Object data = null) where TOutput : class
        {
            if (converter == null) converter = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;

            if (typeof(TOutput) == typeof(string))
            {
                result = string.Concat("[", string.Join(",", list.Select(t => converter(t))), "]");
            }
            else
            {
                result = list.ToList().ConvertAll(converter).ToJson();
            }
            sb.Append("{")
                .AppendFormat("\"RecordCount\":{0},", list.Count())
                .AppendFormat("\"data\":{0},", data == null ? "null" : data.ToJson())
                .AppendFormat("\"list\":{0}", result)
                .Append("}");

            return sb.ToString();
        }

        #endregion

        private static Dictionary<string, MethodResult> _methodResult = new Dictionary<string, MethodResult>();
        /// <summary>
        /// 要反射的类型
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual Type GetMethod(HttpContext context, out MethodInfo method, out string agent, bool toUpper = true)
        {
            string url = context.Request.Path;
            //if (_methodResult.ContainsKey(url))
            //{
            //    MethodResult result = _methodResult[url];
            //    method = result.method;
            //    agent = result.agent;
            //    return result.type;
            //}
            Type type = null;
            method = null;
            agent = null;
            try
            {
                Regex regex = new Regex(@"(?<Method>\w+)");
                List<string> list = new List<string>();
                foreach (Match match in regex.Matches(url))
                {
                    list.Add(match.Value);
                }
                agent = list.FirstOrDefault();
                if (list.Count < 3) return null;
                if (toUpper) agent = Regex.Replace(agent, @"^\w", t => t.Value.ToUpper());
                list[0] = agent;
                string typeName = string.Concat(this.TypeName, ".", string.Join(".", list.Take(list.Count - 1)));
                type = this.GetAssembly().GetType(typeName);
                if (type == null) return null;
                string methodName = list.Last();
                method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null) return null;
                return type;
            }
            finally
            {
                //if (!_methodResult.ContainsKey(url))
                //{
                //    _methodResult.Add(url, new MethodResult()
                //    {
                //        type = type,
                //        agent = agent,
                //        method = method
                //    });
                //}
            }
        }

        /// <summary>
        /// 反射到类名的前缀
        /// </summary>
        protected abstract string TypeName { get; }

        /// <summary>
        /// 当前映射到的资源库
        /// </summary>
        /// <returns></returns>
        protected abstract Assembly GetAssembly();

    }
}
