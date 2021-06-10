using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.ComponentModel;

using SP.Studio.Array;
using SP.Studio.IO;

namespace SP.Studio.Drawing
{
    public static class ImageAgent
    {
        /// <summary>
        /// 创建缩略图
        /// </summary>
        /// <param name="file">源图</param>
        /// <param name="type">类型</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="quality">缩略图质量</param>
        /// <param name="createFile">要创建的缩略图路径。 省略此参数则创造一个随机文件名并且与原图在同一文件夹内</param>
        /// <returns>缩略图的物理路径</returns>
        public static string CreateThumbnail(string file, ThumbType type, int width, int height, long quality = 60, string createFile = null)
        {
            if (!File.Exists(file)) throw new Exception("找不到文件路径：" + file);

            file = file.Replace('/', '\\');
            if (string.IsNullOrEmpty(createFile)) createFile = string.Format("{0}{1}{2}.jpg", file.Substring(0, file.LastIndexOf('\\') + 1),
                DateTime.Now.ToString("HHmmss"), Guid.NewGuid().ToString("N").Substring(0, 6));

            FileAgent.CreateDirectory(createFile, true);

            if (type == ThumbType.Cut)
            {
                Cut(file, 0, 0, width, height, quality, createFile, true);
            }
            else
            {
                using (Image image = Image.FromFile(file))
                {
                    double zoom1, zoom2;
                    switch (type)
                    {
                        case ThumbType.Width:
                            height = (int)(((double)width / (double)image.Width) * (double)image.Height);
                            break;
                        case ThumbType.Height:
                            width = (int)(((double)height / (double)image.Height) * (double)image.Width);
                            break;
                        case ThumbType.Max:
                        case ThumbType.Cut:
                            zoom1 = (double)width / (double)image.Width;
                            zoom2 = (double)height / (double)image.Height;
                            if (zoom1 > zoom2)
                            {
                                height = (int)((double)image.Height * zoom1);
                            }
                            else
                            {
                                width = (int)((double)image.Width * zoom2);
                            }
                            break; ;
                        case ThumbType.Min:
                            zoom1 = (double)width / (double)image.Width;
                            zoom2 = (double)height / (double)image.Height;
                            if (zoom1 < zoom2)
                            {
                                height = (int)((double)image.Height * zoom1);
                            }
                            else
                            {
                                width = (int)((double)image.Width * zoom2);
                            }
                            break;
                    }

                    using (Bitmap bm = new Bitmap(width, height))
                    {
                        using (Graphics g = Graphics.FromImage(bm))
                        {
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.Clear(Color.White);
                            g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                        }
                        ImageCodecInfo ici = ImageCodecInfo.GetImageEncoders().First(t => t.MimeType == "image/jpeg");
                        EncoderParameters param = new EncoderParameters();
                        param.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                        image.Dispose();    // 提前释放源图操作对象
                        bm.Save(createFile + (type == ThumbType.Cut ? ".org" : ""), ici, param);
                    }
                }
            }

            return createFile;
        }

        /// <summary>
        /// 剪裁图片流
        /// </summary>
        public static byte[] CreateThumbnail(byte[] file, ThumbType type, int width, int height, long quality = 60)
        {
            using (Image image = Image.FromStream(new MemoryStream(file)))
            {
                double zoom1, zoom2;
                switch (type)
                {
                    case ThumbType.Width:
                        height = (int)(((double)width / (double)image.Width) * (double)image.Height);
                        break;
                    case ThumbType.Height:
                        width = (int)(((double)height / (double)image.Height) * (double)image.Width);
                        break;
                    case ThumbType.Max:
                    case ThumbType.Cut:
                        zoom1 = (double)width / (double)image.Width;
                        zoom2 = (double)height / (double)image.Height;
                        if (zoom1 > zoom2)
                        {
                            height = (int)((double)image.Height * zoom1);
                        }
                        else
                        {
                            width = (int)((double)image.Width * zoom2);
                        }
                        break; ;
                    case ThumbType.Min:
                        zoom1 = (double)width / (double)image.Width;
                        zoom2 = (double)height / (double)image.Height;
                        if (zoom1 < zoom2)
                        {
                            height = (int)((double)image.Height * zoom1);
                        }
                        else
                        {
                            width = (int)((double)image.Width * zoom2);
                        }
                        break;
                }

                using (Bitmap bm = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(bm))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.Clear(Color.White);
                        g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                    }
                    ImageCodecInfo ici = ImageCodecInfo.GetImageEncoders().First(t => t.MimeType == "image/jpeg");
                    EncoderParameters param = new EncoderParameters();
                    param.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                    MemoryStream ms = new MemoryStream();
                    bm.Save(ms, ici, param);
                    return ms.GetBuffer();
                }
            }
        }

        /// <summary>
        /// 剪裁图片
        /// </summary>
        /// <param name="file">要被剪裁的图片</param>
        /// <param name="createFile">剪裁完成之后保存的路径</param>
        /// <param name="isZoom">是否需要缩放</param>
        public static void Cut(string file, int left, int top, int width, int height, long quality = 60, string createFile = null, bool isZoom = false)
        {
            if (string.IsNullOrEmpty(createFile)) createFile = file;
            Stream fileStream;
            using (Image bm = new Bitmap(width, height))
            {
                using (Image image = Image.FromFile(file))
                {
                    using (Graphics g = Graphics.FromImage(bm))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.Clear(Color.White);

                        double zoomWidth, zoomHeight;   // 缩放系数
                        int width1 = width;
                        int height1 = height;

                        if (isZoom)
                        {
                            zoomWidth = (double)(image.Width - left) / width;
                            zoomHeight = (double)(image.Height - top) / height;

                            if (zoomWidth > zoomHeight)
                            {
                                width1 = (int)(width * Math.Min(zoomWidth, zoomHeight)) + left;
                                height1 = image.Height;
                            }
                            else
                            {
                                width1 = image.Width;
                                height1 = (int)(height * Math.Min(zoomWidth, zoomHeight)) + top;
                            }
                            // 如果需要缩放图片则自动截取中间的图片
                            left = Math.Abs((image.Width - width1) / 2);
                            top = Math.Abs((image.Height - height1) / 2);
                        }

                        //在指定位置并且按指定大小绘制原图片的指定部分 
                        g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(left, top, width1, height1), GraphicsUnit.Pixel);

                        ImageCodecInfo ici = ImageCodecInfo.GetImageEncoders().First(t => t.MimeType == "image/jpeg");
                        EncoderParameters param = new EncoderParameters();
                        param.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                        fileStream = new MemoryStream();
                        bm.Save(fileStream, ici, param);
                    }
                }
            }
            FileAgent.SaveStreamToFile(fileStream, createFile);
        }


        /// <summary>
        /// 创建图片水印
        /// </summary>
        /// <param name="fileName">要加水印的图片路径</param>
        /// <param name="markImage">水印图片</param>
        /// <param name="x">水印的横向位置</param>
        /// <param name="y">水印的纵向位置</param>
        /// <param name="width">要剪裁的水印图片宽度</param>
        /// <param name="height">要剪裁的水印图片高度</param>
        /// <returns>是否增加成功</returns>
        public static bool CreateWaterMark(string fileName, Image markImage, int x, int y, int width = 0, int height = 0)
        {

            bool isCreate = false;
            Bitmap bitmap = null;
            try
            {
                using (Image image = Image.FromFile(fileName))
                {
                    bitmap = new Bitmap(image.Width, image.Height);
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(image, 0, 0, image.Width, image.Height);
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        if (width == 0 || height == 0)
                        {
                            if (markImage.Width > image.Width || markImage.Height > image.Height) return true;
                            g.DrawImage(markImage, new Rectangle(x, y, markImage.Width, markImage.Height), 0, 0, markImage.Width, markImage.Height, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            if (width > image.Width || height > image.Height) return true;
                            g.DrawImage(markImage, new Rectangle(x, y, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
                        }
                    }
                    isCreate = true;
                }
            }
            finally
            {
                if (bitmap != null)
                {
                    EncoderParameters ep = new EncoderParameters();
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);
                    ImageCodecInfo ici = ImageCodecInfo.GetImageEncoders().First(t => t.MimeType == "image/jpeg");
                    if (ici == null) throw new Exception();
                    bitmap.Save(fileName, ici, ep);
                    bitmap.Dispose();
                }
            }
            return isCreate;
        }

        /// <summary>
        /// 创建水印
        /// </summary>
        public static bool CreateWaterMark(string fileName, Image markImage, WaterMarkPlace place)
        {
            if (place == WaterMarkPlace.Random)
            {
                place = (WaterMarkPlace)Enum.Parse(typeof(WaterMarkPlace), Enum.GetNames(typeof(WaterMarkPlace)).ToList().FindAll(t => t != place.ToString()).GetRandom());
            }

            int x, y;
            int width, height;
            int markWidth = markImage.Width;
            int markHeight = markImage.Height;
            x = y = 0;

            using (Image image = Image.FromFile(fileName))
            {
                width = image.Width;
                height = image.Height;
            }

            if (markWidth > width || markHeight > height) return false;

            switch (place)
            {
                case WaterMarkPlace.LeftTop:
                    x = y = 10;
                    break;
                case WaterMarkPlace.CenterTop:
                    x = (width - markWidth) / 2;
                    y = 10;
                    break;
                case WaterMarkPlace.RightTop:
                    x = width - markWidth - 10;
                    y = 10;
                    break;
                case WaterMarkPlace.LeftBottom:
                    x = 10;
                    y = height - markHeight - 10;
                    break;
                case WaterMarkPlace.CenterBottom:
                    x = (width - markWidth) / 2;
                    y = height - markHeight - 10;
                    break;
                case WaterMarkPlace.RightBottom:
                    x = width - markWidth - 10;
                    y = height - markHeight - 10;
                    break;
                case WaterMarkPlace.MiddleCenter:
                    x = (width - markWidth) / 2;
                    y = (height - markHeight) / 2;
                    break;
            }

            return CreateWaterMark(fileName, markImage, x, y);
        }

        public static bool CreateWaterMark(string fileName, string markName, WaterMarkPlace place)
        {
            return CreateWaterMark(fileName, Image.FromFile(markName), place);
        }

        /// <summary>
        /// 获取图片文件的尺寸
        /// </summary>
        public static Size GetSize(string path)
        {
            using (Image image = Image.FromFile(path))
            {
                return image.Size;
            }
        }
    }

    public enum ThumbType
    {
        /// <summary>
        /// 占位符
        /// </summary>
        None,
        /// <summary>
        /// 按最小边
        /// </summary>
        Min,
        /// <summary>
        /// 按最大边
        /// </summary>
        Max,
        /// <summary>
        /// 缩放宽度 忽略高度
        /// </summary>
        Width,
        /// <summary>
        /// 缩放高度 忽略宽度
        /// </summary>
        Height,
        /// <summary>
        /// 剪裁
        /// </summary>
        Cut
    }

    public enum WaterMarkPlace
    {
        [Description("左上角")]
        LeftTop,
        [Description("顶部中央")]
        CenterTop,
        [Description("右上角")]
        RightTop,
        [Description("左下角")]
        LeftBottom,
        [Description("底部中央")]
        CenterBottom,
        [Description("右下角")]
        RightBottom,
        [Description("居中")]
        MiddleCenter,
        [Description("随机")]
        Random
    }
}
