using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;

using SP.Studio.IO;

namespace SP.Studio.Drawing
{
    /// <summary>
    /// 接收并且保存二进制上传的图片数据
    /// </summary>
    public class ImageUploadHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.InputStream.Length == 0) return;
            byte[] data = new byte[context.Request.InputStream.Length];
            context.Request.InputStream.Read(data, 0, data.Length);
            string fileExt = context.Request.QueryString["ext"];
            if (string.IsNullOrEmpty(fileExt)) fileExt = "jpg";
            string fileName = FileAgent.GetMD5(data);
            switch (context.Request.QueryString["type"])
            {
                case "face":
                    fileName = string.Format(@"/face/{0}.{1}", fileName, fileExt);
                    break;
                case "upload":
                    string uploadFolder = context.Server.MapPath("~/upload/" + DateTime.Now.ToString("yyyyMM"));
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                    fileName = string.Format("/upload/{0}/{1}.{2}", DateTime.Now.ToString("yyyyMM"), fileName, fileExt);
                    break;
            }
            string filePath = context.Server.MapPath("~" + fileName);
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, data);
            }
            context.Response.Write(fileName);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
