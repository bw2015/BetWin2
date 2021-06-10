using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.ComponentModel;

using SP.Studio.Core;
using SP.Studio.IO;
using System.Drawing;

namespace SP.Studio.Net
{
    /// <summary>
    /// 适用于Web程序的上传类
    /// </summary>
    public class UploadAgent
    {
        /// <summary>
        /// 图片类型
        /// </summary>
        public static readonly string[] IMAGE_EXTENSION = new string[] { "gif", "png", "jpg", "jpeg", "bmp" };

        /// <summary>
        /// 安全的文件类型
        /// </summary>
        public static readonly string[] SAFE_EXTENSION = new string[] { "gif", "png", "jpg", "jpeg", "bmp", "zip", "rar", "txt", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "wps", "pdf" };


        /// <summary>
        /// 上传文件的通用方法
        /// </summary>
        /// <param name="file">文件流</param>
        /// <param name="maxSize">允许的最大尺寸</param>
        /// <param name="type">允许的文件类型</param>
        /// <param name="path">要保存到的路径 
        /// 如果是个包括文件名的完整路径则保存至该路径
        /// 如果只是文件夹则自动在该文件夹下建立年/月/日的子文件夹，随机命名保存</param>
        /// <param name="fileType">自定义的文件后缀名 如果拥有该参数则type失效</param>
        /// <returns>上传信息对象</returns>
        public static UploadInfo UploadFile(HttpPostedFile file, int maxSize, UploadFileType type, string path, params string[] fileType)
        {
            int startPath = path.Length;
            path = path.Replace('/', '\\');
            if (file == null || file.ContentLength == 0) return new UploadInfo(UploadFaildType.NoFile);
            if (file.ContentLength > maxSize) return new UploadInfo(UploadFaildType.TooBig);

            if (fileType.Length == 0)
            {
                switch (type)
                {
                    case UploadFileType.Image:
                        fileType = IMAGE_EXTENSION;
                        break;
                    case UploadFileType.Safe:
                        fileType = SAFE_EXTENSION;
                        break;
                }
            }
            if (!IsMatchExtension(file.FileName, fileType)) return new UploadInfo(UploadFaildType.NotFormat);

            string savePath = path;
            // 是文件
            if (path.LastIndexOf('.') > path.LastIndexOf('\\'))
            {
                FileAgent.CreateDirectory(path, true);
            }
            else
            {
                if (!path.EndsWith("\\")) path += "\\";
                FileAgent.CreateDirectory(path, false);
                savePath = string.Format(@"{0}{1}\{2}", path, FileAgent.CreateDirectory(path), FileAgent.CreateRandomFileName(file.FileName.GetExtension()));
            }

            file.SaveAs(FileAgent.MapPath(savePath));
            savePath = savePath.Replace('\\', '/');

            short width = 0;
            short height = 0;
            if (type == UploadFileType.Image)
            {
                using (Image image = Image.FromFile(savePath))
                {
                    width = (short)image.Width;
                    height = (short)image.Height;
                }
            }
            string fileName = file.FileName;
            if (fileName.Contains('\\')) fileName = fileName.Substring(fileName.LastIndexOf('\\'));
            if (fileName.Contains('/')) fileName = fileName.Substring(fileName.LastIndexOf('/'));
            return new UploadInfo(UploadFaildType.None)
            {
                ErrorMsg = null,
                FileName = fileName,
                FilePath = savePath,
                SavePath = savePath.Length == startPath ? savePath : savePath.Substring(startPath),
                FileSize = file.ContentLength,
                Image = new Tuple<short, short>(width, height)
            };
        }

        private static bool IsMatchExtension(string fileName, string[] fileType)
        {
            if (fileType.Length == 0) return true;
            string extName = fileName.GetExtension();
            return fileType.Contains(extName);
        }
    }

    /// <summary>
    /// 上传信息
    /// </summary>
    public struct UploadInfo
    {
        public UploadInfo(UploadFaildType type)
            : this(type, null)
        {

        }

        public UploadInfo(UploadFaildType type, string msg)
            : this()
        {
            this.FaildType = type;
            this.ErrorMsg = string.IsNullOrEmpty(msg) ? type.GetDescription() : msg;
        }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件路径（物理路径）
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 保存路徑(相对路径)
        /// 如果指定了保存路径则返回物理路径
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// 错误类型
        /// </summary>
        public UploadFaildType FaildType { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 图片信息 宽度/高度
        /// </summary>
        public Tuple<short, short> Image { get; set; }
    }

    /// <summary>
    /// 上传的错误信息
    /// </summary>
    public enum UploadFaildType
    {
        /// <summary>
        /// 上传成功
        /// </summary>
        [Description("没有错误")]
        None,
        [Description("没有选择要上传的文件")]
        NoFile,
        [Description("超过允许的最大尺寸")]
        TooBig,
        [Description("上传文件格式不正确")]
        NotFormat
    }

    public enum UploadFileType
    {
        /// <summary>
        /// 图片
        /// </summary>
        Image,
        /// <summary>
        /// 安全的文件（除了可被执行的脚本后缀的所有文件）
        /// </summary>
        Safe,
        /// <summary>
        /// 所有文件
        /// </summary>
        All
    }
}
