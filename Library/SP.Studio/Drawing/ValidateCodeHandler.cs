using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Reflection;
using System.Resources;

using SP.Studio.Model;

using SP.Studio.Array;
using SP.Studio.Web;

namespace SP.Studio.Drawing
{
    /// <summary>
    /// 验证码处理
    /// 需在Web.Config内增加结点
    /// </summary>
    public class ValidateCodeHandler : IHttpHandler, IRequiresSessionState
    {
        private string _randomString;
        private string randomString
        {
            get
            {
                if (_randomString == null)
                {
                    string rnd = Guid.NewGuid().ToString("n");
                    _randomString = string.Empty;
                    char[] number = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                    foreach (char n in rnd)
                    {
                        if (number.Contains(n)) _randomString += n;
                        if (_randomString.Length == 4) break;
                    }
                    //_randomString = Guid.NewGuid().ToString("n").Substring(0, 4).ToUpper();
                }
                return _randomString;
            }
        }

        public Bitmap CreateCode(string text, int width = 80, int height = 30)
        {
            try
            {
                Bitmap bmp = new Bitmap(width, height);
                Color[] colorList = new Color[] { Color.Black, Color.Red, Color.Yellow, Color.Green, Color.Blue };

                //typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public).ToList(). ConvertAll(t => (Color)t.GetValue(null, null)).FindAll(t => t != Color.White).ToArray();
                Brush[] brushList = typeof(Brushes).GetProperties(BindingFlags.Static | BindingFlags.Public).ToList().
                   ConvertAll(t => (Brush)t.GetValue(null, null)).ToArray();
                var rand = new Random();
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    string[] fonts = new string[] { "MS Sans Serif" };
                    using (StringFormat f = new StringFormat())
                    {
                        f.Alignment = StringAlignment.Near;
                        f.LineAlignment = StringAlignment.Center;
                        f.FormatFlags = StringFormatFlags.NoWrap;
                        PrivateFontCollection privateFonts = new PrivateFontCollection();
                        for (int index = 0; index < text.Length; index++)
                        {
                            string s = text[index].ToString();
                            int random = Guid.NewGuid().GetHashCode();
                            Font myFont = new Font(fonts.GetIndex(random % fonts.Length), random % 4 + 16, FontStyle.Regular);
                            g.DrawString(s, myFont, Brushes.Black, new RectangleF(1 + (width / text.Length) * index, 1, width, height), f);
                            g.DrawString(s, myFont, brushList[rand.Next(0, brushList.Length)], new RectangleF((width / text.Length) * index, 0, width, height), f);
                        }
                    }

                    // 制造噪声 杂点面积占图片面积的 30%
                    int num = width * height * 1 / 100;
                    for (int iCount = 0; iCount < num; iCount++)
                    {
                        bmp.SetPixel(rand.Next(0, width), rand.Next(0, height), colorList[rand.Next(0, colorList.Length)]);
                    }
                }
                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{3}\nText={0}&Width={1}&Height={2}", text, width, height, ex.Message));
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            string name = WebAgent.GetParam("Name", "vcode");
            string code = WebAgent.GetParam("Code");

            switch (WebAgent.QS("ac"))
            {
                case "check":
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(Check(context, name, code) ? 1 : 0);
                    break;
                case "valid":   // 验证验证码是否正确 返回 Result 格式的json
                    context.Response.ContentType = "text/json";
                    bool success = context.Session[name] != null && ((string)(context.Session[name])).Equals(code, StringComparison.CurrentCultureIgnoreCase);
                    context.Response.Write(new Result(success, success ? "验证码正确" : "验证码错误"));
                    break;
                default:
                    context.Response.ContentType = "image/png";
                    context.Session[name] = randomString;
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        Bitmap bitmap = this.CreateCode(randomString, WebAgent.QS("width", 80), WebAgent.QS("height", 30));
                        bitmap.Save(ms, ImageFormat.Png);
                        context.Response.BinaryWrite(ms.ToArray());
                    }
                    break;
            }
        }


        public static bool Check(HttpContext context, string name, string code)
        {
            return context.Session[name] != null && ((string)(context.Session[name])).Equals(code, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
