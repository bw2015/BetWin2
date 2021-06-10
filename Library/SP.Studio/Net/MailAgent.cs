using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace SP.Studio.Net
{
    /// <summary>
    /// 邮件处理相关
    /// </summary>
    public class MailAgent : IDisposable
    {
        /// <summary>
        /// 参数设置
        /// </summary>
        private readonly string Host, Account, Password, FromAddress, DisplayName;

        private readonly int Port;

        /// <summary>
        /// 是否开启SSL加密
        /// </summary>
        private readonly bool SSL;

        /// <summary>
        /// 参数设置的构造函数
        /// </summary>
        public MailAgent(string host, int port, string account, string password, string fromAddress, string displayName, bool ssl = false)
        {
            this.Host = host;
            this.Port = port;
            this.Account = account;
            this.Password = password;
            this.FromAddress = fromAddress;
            this.DisplayName = displayName;
            this.SSL = ssl;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="attachments">附件</param>
        public void Send(string subject, string body, string toAddress, bool isBodyHtml = true, params Attachment[] attachments)
        {
            using (SmtpClient client = new SmtpClient(this.Host, this.Port))
            {
                client.Credentials = new NetworkCredential(Account, Password);
                client.Port = this.Port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = this.SSL;

                MailAddress from = new MailAddress(this.FromAddress, this.DisplayName, Encoding.UTF8);
                MailMessage message = new MailMessage();
                message.From = from;
                message.To.Add(toAddress);
                message.BodyEncoding = message.SubjectEncoding = Encoding.UTF8;
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isBodyHtml;
                message.Priority = MailPriority.High;
                foreach (Attachment att in attachments)
                {
                    message.Attachments.Add(att);
                }
                object userState = message;
                client.Send(message);
            }
        }

     
        public void Dispose()
        {

        }
    }
}
