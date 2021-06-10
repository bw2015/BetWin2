using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Configuration;


using SP.Studio.Net;
using SP.Studio.Web;
using System.IO;
using System.Windows.Forms;

using System.Diagnostics;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 客户端采集类
    /// </summary>
    public abstract class IGateway : IDisposable
    {
        #region ======= 付费接口的通用采集方法 ============

        /// <summary>
        /// 彩期的长度
        /// </summary>
        protected virtual int IndexLength
        {
            get
            {
                return 3;
            }
        }

        /// <summary>
        /// 通用的彩期转换方法
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetIndex(string index)
        {
            Regex[] regexs = new Regex[]
            {
                new Regex(@"^(?<Date>\d{8})(?<Index>\d{2})$"),
                new Regex(@"^(?<Date>\d{8})(?<Index>\d{3})$"),
                new Regex(@"^(?<Date>\d{6})(?<Index>\d{2})$"),
                new Regex(@"^(?<Date>\d{6})(?<Index>\d{3})$")
            };
            foreach (Regex regex in regexs)
            {
                if (regex.IsMatch(index))
                {
                    string date = regex.Match(index).Groups["Date"].Value;
                    if (date.Length == 6) date = "20" + date;
                    string value = regex.Match(index).Groups["Index"].Value;
                    if (value.Length < this.IndexLength)
                    {
                        value = value.PadLeft(this.IndexLength, '0');
                    }
                    else
                    {
                        value = value.Substring(value.Length - this.IndexLength);
                    }
                    return string.Format("{0}-{1}", date, value);
                }
            }
            return index;
        }

        /// <summary>
        /// 开奖结果采集的转换方法
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> GetResult(Dictionary<string, string> dic)
        {
            string type = this.GetType().Name;
            if (type.EndsWith("11x5"))
            {
                return dic.ToDictionary(t => this.GetIndex(t.Key), t => string.Join(",", t.Value.Split(',').Select(p => p.PadLeft(2, '0'))));
            }
            if (type.EndsWith("K3"))
            {
                return dic.ToDictionary(t => this.GetIndex(t.Key), t => t.Value);
            }
            return dic;
        }

        /// <summary>
        /// 开采网获取
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getResultByAPIPlus()
        {
            return this.GetResult(Utils.GetAPI(API.opencai, this.GetType()));
        }

        private Dictionary<string, string> getResultByCPK()
        {
            return this.GetResult(Utils.GetAPI(API.cpk, this.GetType()));
        }

        private Dictionary<string, string> getResultByMCai()
        {
            return this.GetResult(Utils.GetAPI(API.mcai, this.GetType()));
        }


        #endregion

        /// <summary>
        /// 并行执行方法
        /// </summary>
        /// <param name="method"></param>
        protected virtual void Run(params Func<Dictionary<string, string>>[] method)
        {
            List<Func<Dictionary<string, string>>> list = method.ToList();

            foreach (API api in Enum.GetValues(typeof(API)))
            {
                if (Utils.IsAPI(api, this.GetType()))
                {
                    switch (api)
                    {
                        case API.opencai:
                            list.Add(getResultByAPIPlus);
                            break;
                        case API.cpk:
                            list.Add(getResultByCPK);
                            break;
                        case API.mcai:
                            list.Add(getResultByMCai);
                            break;
                    }
                }
            }

            System.Threading.Tasks.Parallel.ForEach(list, t =>
            {
                string name = t.Method.Name;
                string key = string.Concat(this.GetType().Name, ".", name);
                lock (typeof(IGateway))
                {
                    if (!Utils.Run.ContainsKey(key)) Utils.Run.Add(key, false);
                    if (Utils.Run[key]) return;
                }

                try
                {
                    Utils.Run[key] = true;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("执行方法：{0}\n", name);
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    Dictionary<string, string> result = (Dictionary<string, string>)t.Invoke();
                    sb.AppendFormat("获取耗时：{0}ms\n", sw.ElapsedMilliseconds);
                    sb.AppendLine(this.Save(result, name));
                    sw.Stop();

                    Utils.WriteLine(sb.ToString());

                    //this.SaveLog("{0}.{1} 耗时：{2}ms", this.GetType().Name, name, sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    this.SaveLog("{0}.{1} {2}", this.GetType().Name, name, ex.Message);
                    Utils.SaveErrorLog("{0}.{1} {2}", this.GetType().Name, name, WebAgent.GetError(ex));
                }
                finally
                {
                    Utils.Run[key] = false;
                }
            });
        }


        protected internal string Save(Dictionary<string, string> data, string name)
        {
            if (data == null || data.Count == 0) return "无采集结果";

            KeyValuePair<string, string> last = data.OrderByDescending(t => t.Key).FirstOrDefault();
            string url = Utils.Configuration.Gateway + "?Type=" + this.GetType().Name;
            string post = string.Join("&", data.OrderBy(t => t.Key).Select(t => t.Key + "=" + t.Value));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("URL:" + url);
            sb.AppendLine("POST:" + post);
            string result = NetAgent.UploadData(url, post, Encoding.UTF8);
            sb.AppendLine("Result:" + result);
            this.SaveLog("{0} - {1}:{2}({3}:{4})", this.GetType().Name, name, result, last.Key, last.Value);
            return sb.ToString();
        }


        private void SaveLog(string content, params object[] args)
        {
            lock (typeof(IGateway))
            {
                content = string.Format(content, args);
                string logFile = Application.StartupPath + @"\Log\";
                if (!Directory.Exists(logFile)) Directory.CreateDirectory(logFile);
                logFile += DateTime.Now.ToString("yyyyMMdd") + ".log";
                //string content = string.Format("{0}\n{1} : {2}\n\r", DateTime.Now, this.GetType().Name, result);
                File.AppendAllText(logFile, DateTime.Now + "\n\r" + content + "\n\r\n\r", Encoding.UTF8);
            }
        }

        public void Dispose()
        {

        }
    }
}
