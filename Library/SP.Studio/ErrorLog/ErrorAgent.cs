using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Web.Configuration;
using System.Configuration;
using System.IO;

using SP.Studio.Data;
using SP.Studio.Configuration;
using SP.Studio.IO;
using SP.Studio.Web;

namespace SP.Studio.ErrorLog
{
    public class ErrorAgent : DbAgent
    {
        private static DatabaseType databaseType = DatabaseType.SQLite;

        private const string TABLE = "Exception";

        /// <summary>
        /// 开启调试
        /// </summary>
        private static bool isDebug;

        /// <summary>
        /// 系统的计时起点时间
        /// </summary>
        private readonly static DateTime StartTime = DateTime.Parse("2011-1-1");

        static ErrorAgent()
        {
            databaseType = ConfigurationManager.AppSettings.AllKeys.Contains(Config.HOST_KEY) && ConfigurationManager.AppSettings[Config.HOST_KEY].Equals(Config.MONO, StringComparison.CurrentCultureIgnoreCase) ?
                DatabaseType.SQLiteMono : DatabaseType.SQLite;

            System.Configuration.Configuration configuration = WebConfigurationManager.OpenWebConfiguration(null);
            SystemWebSectionGroup ws = (SystemWebSectionGroup)configuration.GetSectionGroup("system.web");
            CompilationSection cp = ws.Compilation;
            isDebug = cp.Debug;


        }


        public ErrorAgent(string db = "|DataDirectory|ErrorLog.db")
            : base(string.Format(@"Data Source ={0}", db), databaseType, DataConnectionMode.Instance)
        {
            this.CreateTable();
        }

        /// <summary>
        /// 创建当日的日志表
        /// </summary>
        private void CreateTable()
        {
            using (DbExecutor db = NewExecutor())
            {
                object obj = db.ExecuteScalar(string.Format("SELECT COUNT(*) FROM sqlite_master where type='table' and name= '{0}'", TABLE));
                if ((long)obj == 0)
                {
                    db.ExecuteNonQuery(string.Format("Create TABLE MAIN.[{0}]([ErrorID] char(32) PRIMARY KEY,Time integer,[Domain] varchar(50),[Title] nvarchar(100),[Detail] text,[HttpCode] smallint);",
                        TABLE));
                }
            }
        }

        /// <summary>
        /// 写入一条信息
        /// </summary>
        public void AddErrorLog(string errorID, string domain, string title, string detail, int httpCode)
        {
            if (httpCode == 0) httpCode = 500;
            this.NewOperation().Insert(TABLE,
                    NewParam("@ErrorID", errorID),
                    NewParam("@Time", GetTime(DateTime.Now)),
                    NewParam("@Domain", domain),
                    NewParam("@Title", title),
                    NewParam("@Detail", detail),
                    NewParam("@HttpCode", httpCode));
        }

        /// <summary>
        /// 获取错误日志列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页的分页大小</param>
        /// <param name="condition">查询条件</param>
        /// <param name="recordCount">总记录数</param>
        internal DataSet GetErrorList(int pageIndex, int pageSize, string condition, out int recordCount)
        {
            return this.NewOperation().GetList(pageIndex, pageSize, TABLE, null, condition, "Time DESC", out recordCount);
        }

        internal DataRow GetErrorInfo(string errorID)
        {
            string condition = "ErrorID = '" + errorID + "'";
            return this.NewOperation().GetInfo(TABLE, null, condition, null);
        }

        #region ======== 静态方法  ============

        internal static DateTime GetTime(long time)
        {
            return StartTime.AddSeconds((double)time);
        }

        internal static long GetTime(DateTime time)
        {
            return (long)((TimeSpan)(time - StartTime)).TotalSeconds;
        }

        public static string CreateDetail()
        {
            StringBuilder sb = new StringBuilder();
            if (HttpContext.Current != null)
            {
                HttpRequest request = HttpContext.Current.Request;

                sb.AppendLine("RawUrl : " + request.Url.Authority + request.RawUrl);
                if (request.UrlReferrer != null) sb.AppendLine("UrlReferrer : " + request.UrlReferrer);
                sb.AppendLine("UserAgent : " + request.UserAgent);
                sb.AppendLine("IP : " + request.UserHostAddress);
                sb.AppendLine("Method : " + request.HttpMethod);
                if (request.Cookies.Count > 0)
                {
                    sb.AppendLine("Cookie:");
                    for (int i = 0; i < request.Cookies.Count; i++)
                        sb.AppendLine("\t" + request.Cookies[i].Name + "\t:\t" + request.Cookies[i].Value);
                }

                sb.AppendLine();
                sb.AppendLine("Request.ServerVariables");
                foreach (var key in HttpContext.Current.Request.ServerVariables.AllKeys)
                {
                    sb.AppendFormat("\t{0}\t:\t{1}", key, HttpContext.Current.Request.ServerVariables[key]);
                    sb.AppendLine();
                }
                sb.AppendLine();

                if (request.HttpMethod == "POST")
                {
                    sb.Append("PostData : ");
                    for (int i = 0; i < request.Form.Count; i++)
                    {
                        if (i > 0) sb.Append("&");
                        sb.Append(request.Form.GetKey(i));
                        sb.Append("=");
                        sb.Append(request.Form[i].ToString());
                    }
                    sb.AppendLine("");
                }
                sb.AppendLine("堆栈信息");
            }
            return sb.ToString();

        }
        public static string CreateDetail(Exception ex)
        {
            string errorID;
            int httpCode;
            return CreateDetail(ex, out errorID, out httpCode);
        }

        /// <summary>
        /// 生成错误的详细日志
        /// </summary>
        public static string CreateDetail(Exception ex, out string errorID, out int httpCode)
        {
            httpCode = 0;
            if (ex == null) ex = new Exception();
            if (ex.InnerException != null) ex = ex.InnerException;
            StringBuilder sb = new StringBuilder();
            errorID = Guid.NewGuid().ToString("N").ToUpper();
            sb.AppendLine("ErrorID : " + errorID);
            sb.AppendLine("Message : " + ex.Message);

            if (ex is HttpException)
            {
                httpCode = ((HttpException)ex).GetHttpCode();
                sb.AppendLine("HttpCode:" + httpCode);
            }
            sb.AppendLine(CreateDetail());
            if (ex != null)
            {
                sb.AppendLine("Type\t:\t" + ex.GetType());
                sb.AppendLine("Source\t:\t" + ex.Source);
                sb.AppendLine("StackTrace\t:\t");
                sb.AppendLine(ex.StackTrace);
                if (ex.TargetSite != null)
                {
                    sb.AppendLine("Method\t:\t" + ex.TargetSite.Name);
                    sb.AppendLine("Class\t:\t" + ex.TargetSite.DeclaringType.FullName);
                }
            }

            foreach (DictionaryEntry obj in ex.Data)
            {
                sb.AppendLine(obj.Key + "\t:\t" + obj.Value);
            }

            sb.AppendLine("Time : " + DateTime.Now);
            return sb.ToString();
        }



        /// <summary>
        /// 写入错误的文本文件
        /// </summary>
        public static void WriteLog(string logName = null, string msg = null)
        {
            if (!isDebug) return;

            if (string.IsNullOrEmpty(logName)) logName = "studio";
            string path = HttpContext.Current.Server.MapPath(string.Format("/App_Data/{1}-{0}.log", DateTime.Now.ToString("yyyyMMdd"), logName));
            int httpCode;
            string errorID;
            if (msg == null) msg = CreateDetail(null, out errorID, out httpCode);

            FileAgent.Write(path, msg + "\n\r", Encoding.UTF8, true);
        }


        /// <summary>
        /// 获取HTTP访问的信息
        /// </summary>
        /// <param name="context"></param>
        public static IEnumerable<string> GetLog(HttpContext context)
        {
            yield return (DateTime.Now.ToString() + " " + context.Response.StatusCode);
            yield return (string.Format("{0} {1} {2}", context.Request.RawUrl, context.Request.UserHostAddress, context.Request.HttpMethod));
            yield return "=================  Header Begin  =================";
            foreach (string header in context.Request.Headers.AllKeys)
            {
                yield return string.Format("{0}：{1}", header, context.Request.Headers[header]);
            }
            yield return "=================  Header End  =================";
            int postCount = -1;
            if (context.Request.HttpMethod == "POST")
            {
                yield return "=================  POST Begin  =================";
                postCount++;
                List<string> post = new List<string>();
                foreach (string key in context.Request.Form.AllKeys)
                {
                    postCount++;
                    post.Add(string.Format("    {0} : {1}", key, context.Request.Form[key]));
                }
                yield return (string.Format("POST \n\r{0}", string.Join("\n\r", post)));
                yield return "=================  POST End  =================";
            }

            if (postCount == 0)
            {
                yield return Encoding.UTF8.GetString(WebAgent.GetInputSteam(context));
            }

            //yield return (string.Format("UserAgent : {0}", context.Request.UserAgent));
            //yield return (string.Format("Cookies : {0}", string.Join("&", context.Request.Cookies.AllKeys.ToList().ConvertAll(t => string.Format("{0}={1}", t, context.Request.Cookies[t].Value)))));
            yield return (string.Empty);
        }

        /// <summary>
        /// 写入当前访问的信息
        /// </summary>
        /// <param name="fileName"></param>
        public static void WriteLog(HttpContext context, string fileName)
        {
            fileName = fileName.Replace('/', '\\');
            if (Directory.Exists(fileName)) fileName += '\\' + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";

            List<string> log = new List<string>();
            try
            {
                log = GetLog(context).ToList();
            }
            catch (Exception ex)
            {
                File.WriteAllText(fileName, ex.Message);
            }
            finally
            {
                File.WriteAllText(fileName, string.Join("\n\r", log));
            }
        }

        #endregion
    }
}
