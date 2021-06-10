using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SP.Studio.Web;
using SP.Studio.Core;

using BW.Common.Sites;
using BW.GateWay.SMS;
using SP.Studio.Data;

namespace BW.Agent
{
    /// <summary>
    /// 短信相关
    /// </summary>
    partial class SiteAgent
    {
        /// <summary>
        /// 给当前用户发送短信
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool SendSMS(string content)
        {
            if (UserInfo == null)
            {
                base.Message("用户未登录");
                return false;
            }

            return this.SendSMS(UserInfo.ID, UserInfo.Mobile, content);
        }

        /// <summary>
        /// 给指定手机号码发送短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool SendSMS(int userId, string mobile, string content)
        {
            if (string.IsNullOrEmpty(mobile))
            {
                mobile = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.ID == userId).Select(t => t.Mobile).FirstOrDefault();
            }
            else if (userId == 0)
            {
                if (UserInfo != null)
                {
                    userId = UserInfo.ID;
                }
                else
                {
                    int? uid = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.Mobile == mobile).Select(t => (int?)t.ID).FirstOrDefault();
                    userId = uid == null ? 0 : uid.Value;
                }
            }
            if (!WebAgent.IsMobile(mobile))
            {
                base.Message("手机号码错误");
                return false;
            }

            if (SiteInfo.Setting.SMS == SMSProvider.None)
            {
                base.Message("短信网关错误");
                return false;
            }

            DateTime? lastTime = BDC.SMSLog.Where(t => t.SiteID == SiteInfo.ID && t.Mobile == mobile && t.Status == SMSStatus.Success).OrderByDescending(t => t.ID).Select(t => (DateTime?)t.CreateAt).FirstOrDefault();
            if (lastTime != null && lastTime.Value > DateTime.Now.AddSeconds(SiteInfo.Setting.SendDelay * -1))
            {
                base.Message("请等待{0}秒后再次发送", (int)((TimeSpan)(lastTime.Value.AddSeconds(SiteInfo.Setting.SendDelay) - DateTime.Now)).TotalSeconds);
                return false;
            }

            SMSLog log = new SMSLog()
            {
                Provider = SiteInfo.Setting.SMS,
                Content = content,
                CreateAt = DateTime.Now,
                Mobile = mobile,
                Result = string.Empty,
                SiteID = SiteInfo.ID,
                UserID = userId,
                Status = SMSStatus.Wait
            };
            log.Add(true);

            SMSStatus status = SMSStatus.Server;
            string result = string.Empty;
            try
            {
                SMSFactory.CreateSMS(new SMSFactory.SMSConfig()
                {
                    Provider = SiteInfo.Setting.SMS,
                    UserName = SiteInfo.Setting.SMSUser,
                    Password = SiteInfo.Setting.SMSPass
                }).Send(mobile, content, out status, out result);
            }
            catch (Exception ex)
            {
                result = ex.Message;
                status = SMSStatus.Server;
            }
            finally
            {
                log.Result = result;
                log.Status = status;
                log.Update(null, t => t.Result, t => t.Status);
            }

            if (status != SMSStatus.Success)
            {
                base.Message(status.GetDescription());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 发送验证码短信
        /// </summary>
        /// <param name="mobile">手机号码</param>
        /// <returns></returns>
        public bool SendCode(string mobile)
        {
            string code = WebAgent.GetRandom(0, 999999).ToString().PadLeft(6, '0');
            //您的校验码是：【变量】，有效时间【变量】分钟。请不要泄露给其他人，如非本人操作，可不用理会！
            string content = string.Format("您的校验码是：{0}，有效时间{1}分钟。请不要泄露给其他人，如非本人操作，可不用理会！", code, SiteInfo.Setting.SMSCodeTime / 60);

            if (!this.SendSMS(0, mobile, content))
            {
                return false;
            }

            return new SMSCode()
            {
                Code = code,
                Mobile = mobile,
                SiteID = SiteInfo.ID,
                SendAt = DateTime.Now
            }.Add();
        }

        /// <summary>
        /// 短信验证码是否正确（如果正确则设置成为已验证）
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CheckCode(string mobile, string code)
        {
            SMSCode smsCode = BDC.SMSCode.Where(t => t.SiteID == SiteInfo.ID && t.Mobile == mobile && t.Code == code && t.SendAt > DateTime.Now.AddSeconds(SiteInfo.Setting.SMSCodeTime * -1)).FirstOrDefault();
            if (smsCode == null)
            {
                base.Message("验证码错误");
                return false;
            }
            smsCode.IsValid = true;
            smsCode.ValidAt = DateTime.Now;
            return smsCode.Update(null, t => t.IsValid, t => t.ValidAt) > 0;
        }
    }
}
