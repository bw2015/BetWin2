using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using BW.IM.Common;
using BW.IM.Framework;
using System.Net;
namespace BW.IM.Handler
{
    /// <summary>
    /// 待执行方法的基类
    /// </summary>
    public abstract class IHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// 当前登录的用户|管理员
        /// </summary>
        protected virtual User UserInfo
        {
            get
            {
                if (HttpContext.Current == null) return null;
                return (User)HttpContext.Current.Items[Utils.USERINFO];
            }
        }

        /// <summary>
        /// layim的错误显示
        /// </summary>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="src"></param>
        protected void layimerror(HttpContext context, int code, string msg, string src = null)
        {
            context.Response.ContentType = "application/json";

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
        /// 调用接口上传图片
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected bool upload(HttpContext context, string type, out string msg)
        {
            HttpPostedFile file = context.Request.Files["file"];
            if (file == null)
            {
                msg = "没有选择要上传的文件";
                return false;
            }
            if (file.ContentLength == 0 || file.ContentLength > 1024 * 1024)
            {
                msg = "文件大小不能超过1M";
                return false;
            }
            string fileType = file.ContentType;

            Regex regex = null;
            string ext = null;

            switch (type)
            {
                case "upload":
                    regex = new Regex(@"^image/(?<Type>jpg|gif|png|jpeg)$", RegexOptions.IgnoreCase);
                    if (!regex.IsMatch(fileType)) fileType = "image/jpg";
                    ext = regex.Match(fileType).Groups["Type"].Value.ToLower();
                    break;
            }

            if (string.IsNullOrEmpty(ext))
            {
                msg = "文件格式错误";
                return false;
            }

            BinaryReader b = new BinaryReader(file.InputStream);
            byte[] data = b.ReadBytes((int)file.InputStream.Length);

            string url = SysSetting.GetSetting().imgServer + "/imageupload.ashx?type=" + type + "&ext=" + ext;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    string result = Encoding.UTF8.GetString(wc.UploadData(url, "POST", data));
                    if (string.IsNullOrEmpty(result))
                    {
                        msg = "图片上传失败";
                        return false;
                    }
                    msg = result;
                    return true;
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                    return false;
                }
            }
        }
    }
}
