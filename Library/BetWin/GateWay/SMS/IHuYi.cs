using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using SP.Studio.Net;
using SP.Studio.Xml;
using SP.Studio.Text;

namespace BW.GateWay.SMS
{
    /// <summary>
    /// 互亿无线
    /// </summary>
    public class IHuYi : SMSBase, ISMS
    {
        public IHuYi(string username, string password) : base(username, password) { }

        public SMSProvider Provider
        {
            get { return SMSProvider.IHuYi; }
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="destnumbers"></param>
        /// <param name="msg"></param>
        /// <param name="status"></param>
        /// <param name="sendTime"></param>
        /// <returns></returns>
        public int Send(string destnumbers, string msg, out SMSStatus status, out string result, DateTime? sendTime = null)
        {
            string url = string.Format("https://106.ihuyi.com/webservice/sms.php?method=Submit&account={0}&password={1}&mobile={2}&content={3}",
                this.UserName, this.Password, destnumbers, msg);

            result = NetAgent.DownloadData(url, Encoding.UTF8);
            status = SMSStatus.Server;
            if (!result.StartsWith("<"))
            {
                BW.Agent.SystemAgent.Instance().AddSystemLog(0, string.Format("短信发送错误,网关：{0}，返回内容：{1}", url, result));
                return 0;
            }

            int count = 0;
            switch (StringAgent.GetString(result, @"<code>(\d+)</code>", 0))
            {
                case 0:
                    status = SMSStatus.Server;
                    break;
                case 2:
                    status = SMSStatus.Success;
                    count = 1;
                    break;
                case 400:
                case 4052:
                    status = SMSStatus.Other;
                    break;
                case 401:
                case 402:
                case 405:
                case 4050:
                    status = SMSStatus.Account;
                    break;
                case 403:
                case 4030:
                case 406:
                    status = SMSStatus.Number;
                    break;
                case 4051:
                    status = SMSStatus.Money;
                    break;
                case 407:
                case 4073:
                    status = SMSStatus.Message;
                    break;
                case 4070:
                    status = SMSStatus.NoSign;
                    break;
                case 4071:
                case 4072:
                    status = SMSStatus.NoTemplate;
                    break;
                case 4081:
                    status = SMSStatus.Buzy;
                    break;
            }
            return count;
        }

        public int Balance()
        {
            string url = string.Format("https://106.ihuyi.com/webservice/sms.php?method=GetNum&account={0}&password={1}",
               this.UserName, this.Password);

            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            XElement root = XElement.Parse(result);
            if (root.GetValue("code", 0) == 2)
            {
                return root.GetValue("num", 0);
            }
            return 0;
        }

        public List<SMSReply> GetReply()
        {
            throw new NotImplementedException();
        }
    }
}
