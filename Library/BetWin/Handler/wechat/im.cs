using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Resources;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

using SP.Studio.Core;
using SP.Studio.Model;
using BW.Common.Users;
using BW.Common.Lottery;
using BW.Common.Sites;
using BW.Common.Systems;
using BW.Agent;
using SP.Studio.Web;
using SP.Studio.Net;
using Encoder = System.Drawing.Imaging.Encoder;

namespace BW.Handler.wechat
{
    public class im : IHandler
    {
        /// <summary>
        /// 微信下注群初始化
        /// </summary>
        /// <param name="context"></param>
        private void init(HttpContext context)
        {
            LotteryType type = QF("Type").ToEnum<LotteryType>();

            List<string> group = new List<string>();
            foreach (ChatTalk.GroupType groupTyype in Enum.GetValues(typeof(ChatTalk.GroupType)))
            {
                group.Add(string.Format("\"{0}\":\"{1}\"", groupTyype, UserAgent.Instance().GetTalkKey(UserInfo.IMID, groupTyype)));
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                ID = UserInfo.IMID,
                Session = UserAgent.Instance().GetUserSession(UserInfo.ID, SP.Studio.PageBase.PlatformType.Wechat),
                Server = SiteInfo.Setting.ServiceServer,
                Name = UserInfo.UserName,
                UserName = UserInfo.UserName,
                Face = UserInfo.FaceShow,
                Money = UserInfo.Money,
                Group = new JsonString(string.Concat("{", string.Join(",", group), "}"))
            });
        }

        /// <summary>
        /// 最近的投注记录
        /// </summary>
        /// <param name="context"></param>
        private void orderlist(HttpContext context)
        {
            LotteryType type = QF("Type").ToEnum<LotteryType>();
            int count = QF("Count", 10);
            var list = BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID && t.Remark != "" && t.Type == type).OrderByDescending(t => t.ID).Take(count);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.CreateAt,
                t.Index,
                Name = UserAgent.Instance().GetUserName(t.UserID),
                Content = t.Remark,
                Avatar = UserAgent.Instance().GetUserFace(t.UserID)
            }));
        }

        /// <summary>
        /// 邀请二维码
        /// </summary>
        /// <param name="context"></param>
        private void invite(HttpContext context)
        {
            if (context.Request.UrlReferrer == null)
            {
                context.Response.Write(false, "未验证来路");
            }
            UserInvite userInvite = UserAgent.Instance().GetUserInviteInfo(WebAgent.GetParam("ID"));
            if (userInvite == null)
            {
                context.Response.Write(false, UserAgent.Instance().Message());
            }

            InviteDomain domain = BDC.InviteDomain.FirstOrDefault();

            string url = string.Format("{0}wx/{1}", domain != null ? domain.Domain : context.Request.UrlReferrer.Authority, userInvite.ID);
            int width = 300;
            int height = 300;
            long quality = 60;

            //
            string api = string.Format("http://chart.googleapis.com/chart?cht=qr&chs={0}x{1}&chl={2}", 200, 200, HttpUtility.UrlDecode(url));
            byte[] data = NetAgent.DownloadFile(api);
            Image qrcode = Image.FromStream(new MemoryStream(data));

            data = NetAgent.DownloadFile(UserInfo.FaceShow);
            Image face = Image.FromStream(new MemoryStream(data));

            Image image = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                Bitmap bitmap = BW.Resources.Res.invite;
                bitmap.Save(ms, ImageFormat.Png);
                image = Image.FromStream(ms);
            }

            using (Bitmap bm = new Bitmap(300, 300))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.Clear(Color.White);
                    g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                    g.DrawImage(qrcode, new Rectangle(50, 70, 200, 200), 0, 0, 200, 200, GraphicsUnit.Pixel);
                    g.DrawImage(face, new Rectangle(10, 10, 42, 42), 0, 0, face.Width, face.Height, GraphicsUnit.Pixel);
                    // 用户名
                    string[] fonts = new string[] { "黑体", "MS Sans Serif" };

                    using (StringFormat f = new StringFormat())
                    {
                        f.Alignment = StringAlignment.Near;
                        f.LineAlignment = StringAlignment.Center;
                        f.FormatFlags = StringFormatFlags.NoWrap;
                        PrivateFontCollection privateFonts = new PrivateFontCollection();
                        Font myFont = new Font("黑体", 16, FontStyle.Regular);
                        g.DrawString(UserInfo.Name, myFont, Brushes.White, 55, 10);
                    }

                }
                ImageCodecInfo ici = ImageCodecInfo.GetImageEncoders().First(t => t.MimeType == "image/jpeg");
                EncoderParameters param = new EncoderParameters();
                param.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                image.Dispose();    // 提前释放源图操作对象


                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    bm.Save(ms, ImageFormat.Png);
                    byte[] codeData = ms.ToArray();
                    if (context.Request.HttpMethod == "POST")
                    {
                        string codeStr = string.Concat("data:image/jpg;base64,", Convert.ToBase64String(codeData));
                        context.Response.ContentType = "text/json";
                        context.Response.Write(true, this.StopwatchMessage(context), new
                        {
                            data = codeStr
                        });
                    }
                    else
                    {
                        context.Response.ContentType = "image/png";
                        context.Response.BinaryWrite(codeData);
                    }
                }
            }

        }
    }
}
