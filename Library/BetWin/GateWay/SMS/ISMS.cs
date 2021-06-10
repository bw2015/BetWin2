using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.ComponentModel;

namespace BW.GateWay.SMS
{
    /// <summary>
    /// 短信网关接口
    /// </summary>
    public interface ISMS
    {
        /// <summary>
        /// 短信商
        /// </summary>
        SMSProvider Provider { get; }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="destnumbers">要发送到的手机号码</param>
        /// <param name="msg">要发送的内容</param>
        /// <param name="sendTime">发送时间 为null表示立刻发送</param>
        int Send(string destnumbers, string msg, out SMSStatus status, out string result, DateTime? sendTime = null);

        /// <summary>
        /// 获取余额
        /// </summary>
        int Balance();

        /// <summary>
        /// 获取上行短信内容
        /// </summary>
        /// <returns>返回null表示没有获取到上行短信</returns>
        List<SMSReply> GetReply();
    }

    public abstract class SMSBase
    {
        /// <summary>
        /// 记录短信网关的返回值
        /// </summary>
        public const string SMSGATEWAY = "SMSGATEWAY";

        protected internal string UserName { get; private set; }

        protected internal string Password { get; private set; }

        public SMSBase(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }

        /// <summary>
        /// 保存返回的信息
        /// </summary>
        protected virtual void SaveGateway(string result)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Items[SMSGATEWAY] = result;
        }

        /// <summary>
        /// 获取原始数据
        /// </summary>
        public virtual string GetGateway()
        {
            if (HttpContext.Current != null)
                return (string)HttpContext.Current.Items[SMSGATEWAY];
            return string.Empty;
        }
    }

    /// <summary>
    /// 短信发送状态
    /// </summary>
    public enum SMSStatus : byte
    {
        [Description("发送成功")]
        Success = 0,
        [Description("服务器故障")]
        Server = 1,
        /// <summary>
        /// 帐号错误
        /// </summary>
        [Description("帐号错误")]
        Account = 2,
        /// <summary>
        /// 手机号码错误
        /// </summary>
        [Description("号码错误")]
        Number = 3,
        /// <summary>
        /// 内容错误 短信内容过长/内容为空/有非法字符
        /// </summary>
        [Description("内容错误")]
        Message = 4,
        /// <summary>
        /// 余额不足
        /// </summary>
        [Description("余额不足")]
        Money = 5,
        [Description("未配置发送")]
        Config = 8,
        [Description("其他故障")]
        Other = 10,
        [Description("没有签名")]
        NoSign = 20,
        [Description("没有配置模板")]
        NoTemplate = 100,
        /// <summary>
        /// 该号码已被使用
        /// </summary>
        [Description("该号码已被使用")]
        Repeater = 101,
        [Description("发送太过频繁，请稍候再发送")]
        Buzy = 102,
        /// <summary>
        /// 等待被发送
        /// </summary>
        [Description("等待发送")]
        Wait = 255,
    }

    /// <summary>
    /// 短信回复内容
    /// </summary>
    public struct SMSReply
    {
        public SMSReply(string mobile, string message, DateTime createAt, object id = null)
        {
            this.Mobile = mobile;
            this.Message = message;
            this.CreateAt = createAt;
            this.ID = id;
        }

        public SMSReply(SMSReply reply, object id = null)
        {
            this.ID = id;
            this.Mobile = reply.Mobile;
            this.Message = reply.Message;
            this.CreateAt = reply.CreateAt;
        }

        /// <summary>
        /// 用于外站的唯一索引
        /// </summary>
        public object ID;

        /// <summary>
        /// 发送的手机号码
        /// </summary>
        public string Mobile;

        /// <summary>
        /// 短信内容
        /// </summary>
        public string Message;

        /// <summary>
        /// 回复的时间
        /// </summary>
        public DateTime CreateAt;
    }

}
