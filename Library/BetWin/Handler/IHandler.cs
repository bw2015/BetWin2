using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.Data.Linq;


using BW.Common.Admins;
using BW.Common.Sites;
using BW.Common.Users;
using BW.Framework;
using BW.Common;

using BW.Agent;
using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Web;
using SP.Studio.Data;
using SP.Studio.Data.Linq;
using DataContextExtension = SP.Studio.Data.Linq.DataContextExtension;

namespace BW.Handler
{
    public abstract class IHandler
    {
        /// <summary>
        /// 当前登录的用户
        /// </summary>
        protected virtual User UserInfo
        {
            get
            {
                return (User)HttpContext.Current.Items[BetModule.USERINFO];
            }
        }

        /// <summary>
        /// 当前站点
        /// </summary>
        protected virtual Site SiteInfo
        {
            get
            {
                return (Site)HttpContext.Current.Items[BetModule.SITEINFO];
            }
        }

        /// <summary>
        /// 当前登录的管理员
        /// </summary>
        protected virtual Admin AdminInfo
        {
            get
            {
                return AdminAgent.Instance().GetAdminInfo();
            }
        }

        /// <summary>
        /// linq操作对象
        /// </summary>
        protected virtual BetDataContext BDC
        {
            get
            {
                return DbSetting.GetSetting().CreateDataContext<BetDataContext>();
            }
        }

        /// <summary>
        /// 当前要查询的页码
        /// </summary>
        protected virtual int PageIndex
        {
            get
            {
                return QF("PageIndex", 1);
            }
        }

        /// <summary>
        /// 默认的数量
        /// </summary>
        protected virtual int PageSize
        {
            get
            {
                return QF("PageSize", 20);
            }
        }

        /// <summary>
        /// 判断用户是否登录
        /// </summary>
        /// <param name="context"></param>
        protected void CheckUserLogin(HttpContext context)
        {
            if (UserInfo == null)
            {
                context.Response.Write(false, "请先登录", new
                {
                    Type = ErrorType.Login
                });
            }
        }

        protected void CheckAdminLogin(HttpContext context)
        {
            if (this.AdminInfo == null)
            {
                context.Response.Write(false, "请先登录");
            }
        }

        /// <summary>
        /// 检查当前管理员是否拥有对应权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="permission"></param>
        protected void CheckAdminLogin(HttpContext context, string permission)
        {
            if (AdminInfo == null || !AdminInfo.HasPermission(permission))
            {
                context.Response.Write(false, "您没有进行此项操作的权限");
            }
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
            if (converter == null) converter = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string json = null;
            if (this.PageIndex == 1)
            {
                json = list.Take(this.PageSize).ToList().ConvertAll(converter).ToJson();
            }
            else
            {
                json = list.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList().ConvertAll(converter).ToJson();
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
            if (this.PageIndex == 1)
            {
                json = list.Take(this.PageSize).ToList().ConvertAll(converter).ToJson();
            }
            else
            {
                json = list.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToList().ConvertAll(converter).ToJson();
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
                result = string.Concat("[", string.Join(",", list.ToList().ConvertAll(converter)), "]");
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

        /// <summary>
        /// 输出一个带锁定类型类型的查询结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="list"></param>
        /// <param name="table"></param>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected string ShowLinqResult<T, TOutput, TSource>(IQueryable<T> list, Table<TSource> table, LockType lockType = LockType.READPAST, Func<TSource, TOutput> converter = null, Object data = null) where TOutput : class
            where TSource : class, new()
        {
            if (converter == null) converter = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string result = list.WITH(table, lockType).Select(t => converter.Invoke(t)).ToJson();
            sb.Append("{")
               .AppendFormat("\"RecordCount\":\"{0}\",", list.Count())
               .AppendFormat("\"data\":{0},", data == null ? "null" : data.ToJson())
               .AppendFormat("\"list\":{0}", result)
               .Append("}");

            return sb.ToString();
        }

        /// <summary>
        /// 有排序的输出对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="list"></param>
        /// <param name="table"></param>
        /// <param name="lockType"></param>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected string ShowLinqResult<T, TOutput, TSource>(IOrderedQueryable<T> list, Table<TSource> table, LockType lockType = LockType.READPAST, Func<TSource, TOutput> converter = null, Object data = null) where TOutput : class
            where TSource : class, new()
        {
            IQueryable<T> query;
            if (this.PageIndex == 1)
            {
                query = list.Take(this.PageSize);
            }
            else
            {
                query = list.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize);
            }

            StringBuilder sb = new StringBuilder();
            string json = query.WITH(table, lockType).Select(t => converter.Invoke(t)).ToJson();
            sb.Append("{")
               .AppendFormat("\"RecordCount\":\"{0}\",", list.Count())
               .AppendFormat("\"PageIndex\":\"{0}\",", this.PageIndex)
               .AppendFormat("\"PageSize\":\"{0}\",", this.PageSize)
               .AppendFormat("\"data\":{0}", data == null ? "null" : data.ToJson())
               .AppendFormat(",\"list\":{0}", json)
               .Append("}");

            return sb.ToString();
        }


        protected string QF(string key)
        {
            string value = QF(key, string.Empty);
            if (AdminInfo == null) value = HttpUtility.HtmlDecode(value);
            return value;
        }

        /// <summary>
        /// 经过HTML转义的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string QFS(string key)
        {
            return HttpUtility.HtmlEncode(this.QF(key));
        }

        protected T QF<T>(string key, T defaultValue)
        {
            return WebAgent.QF(key, defaultValue);
        }

        /// <summary>
        /// 获取时间（不能早于系统设置的最多可查询时间）
        /// 如果是管理员则不受系统时间设置影响
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected DateTime QF(string key, DateTime defaultValue)
        {
            DateTime datetime = WebAgent.QF(key, defaultValue);
            if (AdminInfo == null && datetime < SiteInfo.StartDate) return SiteInfo.StartDate;
            return datetime;
        }

        /// <summary>
        /// 显示返回值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <param name="successMessage">成功后的默认显示内容</param>

        protected virtual void ShowResult(HttpContext context, bool result, string successMessage = "执行成功", object info = null)
        {
            if (result)
            {
                context.Response.Write(true, successMessage, info);
            }
            else
            {
                context.Response.Write(false, (string)context.Items[BetModule.MESSAGE]);
            }
        }

        /// <summary>
        /// 显示运行时间
        /// </summary>
        /// <returns></returns>
        protected virtual string StopwatchMessage(HttpContext context)
        {
            if (!context.Items.Contains(BetModule.STOPWATCH)) return string.Empty;
            Stopwatch sw = (Stopwatch)context.Items[BetModule.STOPWATCH];
            return string.Format("{0}ms", sw.ElapsedMilliseconds);
        }

        public virtual T Show<T>(T? value) where T : struct
        {
            return value == null ? default(T) : value.Value;
        }


        protected void layimerror(HttpContext context, int code, string msg, string src = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"code\":{0}", code)
                .AppendFormat(",\"msg\":\"{0}\"", msg)
                .Append(",\"data\":{")
                .AppendFormat("\"src\":\"{0}\"", src)
                .Append("}  }");
            context.Response.Write(sb);
            context.Response.End();
        }

        /// <summary>
        /// 输出枚举类型
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        protected virtual void enumtype(HttpContext context, Type type)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(type.ToList(), t => new
            {
                text = t.Description,
                value = t.Name
            }));
        }

        /// <summary>
        /// 测试帐号的linq查询列表
        /// </summary>
        protected virtual IQueryable<int> TestUserList
        {
            get
            {
                return BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.IsTest).Select(t => t.ID);
            }
        }

        /// <summary>
        /// 获取报表存储过程的参数
        /// </summary>
        /// <param name="context"></param>
        protected virtual void GetReportParameber(HttpContext context)
        {
            string procname = QF("procname");

            List<EnumObject> list = SystemAgent.Instance().GetReportInfo(procname);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Name,
                t.Description,
                Type = t.Picture
            }));
        }

        /// <summary>
        /// 错误类型
        /// </summary>
        public enum ErrorType
        {
            /// <summary>
            /// 未登录
            /// </summary>
            Login,
            /// <summary>
            /// 未设置资金密码
            /// </summary>
            PayPassword,
            /// <summary>
            /// 未设置银行卡
            /// </summary>
            BankAccount,
            /// <summary>
            /// 站点暂停
            /// </summary>
            Stop
        }
    }
}
