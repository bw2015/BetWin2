using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

using Newtonsoft.Json;
using SP.Studio.Web;
using SP.Studio.Configuration;
using SP.Studio.Core;
using System.Web;

namespace SP.Studio.Model
{
    /// <summary>
    /// 用于返回结果
    /// </summary>
    [Serializable]
    public class Result
    {
        /// <summary>
        /// 获取json的返回值
        /// </summary>
        public Result(string json)
        {
            Result result = JsonConvert.DeserializeObject<Result>(json);
            this.Success = result.Success;
            this.Message = result.Message;
            this.Info = result.Info;
        }

        /// <summary>
        /// 默认成功的节奏
        /// </summary>
        public Result() : this(true, string.Empty) { }

        /// <summary>
        /// 设置结果和返回信息
        /// </summary>
        /// <param name="success">结果</param>
        /// <param name="message">返回信息</param>
        public Result(int success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        /// <summary>
        /// 设置结果、返回信息和json对象
        /// </summary>
        /// <param name="success">结果</param>
        /// <param name="message">返回信息</param>
        /// <param name="info">JSON对象</param>
        public Result(int success, string message, object info)
            : this(success, message)
        {
            this.Info = info;
        }

        public Result(bool success, string message)
            : this(success ? 1 : 0, message)
        {
        }

        public Result(bool success, string message, object info)
            : this(success ? 1 : 0, message, info)
        {
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        [DataMember(Name = "success", IsRequired = true)]
        public int Success { get; set; }

        /// <summary>
        /// 返回的信息
        /// </summary>
        [
        DataMember(Name = "msg", IsRequired = true),
        Newtonsoft.Json.JsonProperty(PropertyName = "msg")
        ]
        public string Message { get; set; }

        /// <summary>
        /// 需要返回的对象
        /// </summary>
        [DataMember(Name = "info", IsRequired = false)]
        public object Info { get; set; }

        /// <summary>
        /// 输出成为json
        /// </summary>
        public string ToJson()
        {
            string info;
            if (this.Info == null)
            {
                info = "null";
            }
            else if (this.Info.GetType() == typeof(string) || this.Info.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
            {
                info = this.Info.ToString();
            }
            else
            {
                try
                {
                    info = this.Info.ToJson();
                }
                catch
                {
                    info = this.Info.ToString();
                }
            }
            return string.Concat("{\"success\" : ", this.Success, " , \"msg\" : \"", HttpUtility.JavaScriptStringEncode(this.Message), "\",\"info\":", info + " }");
        }

        /// <summary>
        /// 转化成为JSON输出
        /// </summary>
        /// <returns>JSON</returns>
        public override string ToString()
        {
            return this.ToJson();
        }

        /// <summary>
        /// 默认转化成为字符串
        /// </summary>
        /// <param name="result">当前对象</param>
        /// <returns>JSON</returns>
        public static implicit operator string(Result result)
        {
            return result.ToJson();
        }
    }

    public static class ResultResponse
    {
        /// <summary>
        /// 输出页面JSON，并且终止页面执行
        /// </summary>
        /// <param name="response"></param>
        /// <param name="success"></param>
        /// <param name="message"></param>
        public static void Write(this HttpResponse response, bool success, string message)
        {
            response.ContentType = "application/json";
            response.Write(new Result(success, message.Get(success)));
            response.End();
        }

        /// <summary>
        /// 输出页面JSON，并且终止页面执行
        /// </summary>
        public static void Write(this HttpResponse response, bool success, string message, object info)
        {
            response.ContentType = "application/json";
            response.Write(new Result(success, message.Get(success), info));
            response.End();
        }

        /// <summary>
        /// 输出JSON内容
        /// </summary>
        /// <param name="response"></param>
        /// <param name="result"></param>
        public static void Write(this HttpResponse response, Result result, bool end)
        {
            response.ContentType = "application/json";
            response.Write(result);
            if (end) response.End();
        }

        private static string Get(this string message, bool success = false)
        {
            if (string.IsNullOrEmpty(message) && !success) message = "发生不可描述的错误";
            return message;
        }
    }
}
