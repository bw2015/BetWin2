using BW.Agent;
using BW.Common.Sites;
using BW.Common.Users;
using BW.Framework;
using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.PageBase;
using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using BankType = BW.Common.Sites.BankType;

namespace BW.Handler.user
{
    /// <summary>
    /// 用户的信息
    /// </summary>
    public class info : IHandler
    {
        /// <summary>
        /// 用户的基本信息
        /// </summary>
        /// <param name="context"></param>
        private void get(HttpContext context)
        {
            UserMoney userMoney = UserAgent.Instance().GetTotalMoney(UserInfo.ID);
            string userSession = WebAgent.QC(BetModule.USERKEY);
            if (string.IsNullOrEmpty(userSession)) userSession = UserAgent.Instance().GetUserSession(UserInfo.ID).ToString("N");

            string groupOnline = string.Concat("{}");

            object user = new
            {
                UserInfo.ID,
                Session = userSession,
                UserInfo.UserName,
                UserInfo.Name,
                UserInfo.Type,
                Money = userMoney.Money,
                Wallet = userMoney.Wallet,
                LockMoney = userMoney.LockMoney,
                TotalMoney = userMoney.TotalMoney,
                Withdraw = Math.Min(userMoney.Money, userMoney.Withdraw),
                UserInfo.Rebate,
                FaceShow = UserInfo.FaceShow,
                NewMessage = UserAgent.Instance().GetNewMessage(UserInfo.ID),
                GroupOnline = new JsonString(groupOnline),
                InviteRebate = SiteInfo.Setting.IsSameRebate ? UserInfo.Rebate : Math.Max(SiteInfo.Setting.MinRebate, UserInfo.Rebate - 2)
            };

            context.Response.Write(true, this.StopwatchMessage(context), user);
        }

        /// <summary>
        /// 获取用户的通知
        /// </summary>
        /// <param name="context"></param>
        private void notify(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(UserAgent.Instance().GetNotifyList(SiteInfo.ID, UserInfo.ID), t => new
            {
                t.ID,
                t.Type,
                t.Message
            }));
        }

        /// <summary>
        /// 获取是新信息数量（客服信息+站内信）
        /// </summary>
        /// <param name="context"></param>
        private void newnotify(HttpContext context)
        {
            bool service = QF("Service", 0) == 1;
            bool message = QF("Message", 0) == 1;
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Service = !service ? 0 : BDC.ChatLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.IMID && !t.IsRead).Count(),
                Message = !message ? 0 : BDC.UserMessage.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && !t.IsRead).Count()
            });
        }

        /// <summary>
        /// 获取当前系统的设置
        /// </summary>
        /// <param name="context"></param>
        private void site(HttpContext context)
        {
            this.CheckUserLogin(context);

            context.Response.Write(true, SiteInfo.Name, new
            {
                Name = SiteInfo.Name,
                Setting = new JsonString(SiteInfo.Setting.ToJson())
            });
        }

        /// <summary>
        /// 获取用户的上一次资料设定情况
        /// </summary>
        /// <param name="context"></param>
        private void last(HttpContext context)
        {
            this.CheckUserLogin(context);
            List<UserInfoLog> list = UserAgent.Instance().GetUserInfoLog(UserInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context),
                list.ToDictionary(t => t.Type, t => new JsonString(new
                {
                    Type = t.Type.GetDescription(),
                    Time = WebAgent.GetTimeDiff(t.UpdateAt)
                }.ToJson())
                ).ToJson());
        }

        /// <summary>
        /// 获取提现银行的设置
        /// </summary>
        /// <param name="context"></param>
        private void getwithdrawbank(HttpContext context)
        {
            if (string.IsNullOrEmpty(UserInfo.PayPassword.Trim()))
            {
                context.Response.Write(false, "您暂未设置资金密码", new
                {
                    Type = ErrorType.PayPassword
                });
            }
            if (SiteInfo.Setting.WithdrawBankList == null)
            {
                context.Response.Write(false, "未设置提现支持的银行，请与客服联系");
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                AccountName = string.IsNullOrEmpty(UserInfo.AccountName) ? "" : WebAgent.HiddenName(UserInfo.AccountName),
                BankList = new JsonString(SiteInfo.Setting.WithdrawBankList.Select(t => new
                {
                    text = t.GetDescription(),
                    value = t
                }).OrderBy(t => t.text).ToList().ToJson()),
                Account = new JsonString("[", string.Join(",", UserAgent.Instance().GetBankAccountList(UserInfo.ID).Select(t => string.Format("\"{0}\"", t.ToString()))), "]")
            });
        }

        /// <summary>
        /// 获取银行帐号信息
        /// </summary>
        /// <param name="context"></param>
        private void getbankinfo(HttpContext context)
        {
            if (string.IsNullOrEmpty(UserInfo.PayPassword.Trim()))
            {
                context.Response.Write(false, "您暂未设置资金密码");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                AccountName = string.IsNullOrEmpty(UserInfo.AccountName) ? "" : WebAgent.HiddenName(UserInfo.AccountName)
            });
        }

        /// <summary>
        /// 检查银行卡是否符合规则
        /// </summary>
        /// <param name="context"></param>
        private void checkaccount(HttpContext context)
        {
            this.CheckUserLogin(context);
            string account = QF("Account");
            if (!WebAgent.IsBankCard(account))
            {
                context.Response.Write(false, "银行卡号错误");
            }
            string bank = WebAgent.GetBankCard(account);
            if (string.IsNullOrEmpty(bank))
            {
                context.Response.Write(false, "不支持的银行卡");
            }
            BankType type = bank.ToEnum<Common.Sites.BankType>();
            if (!SiteInfo.Setting.WithdrawBankList.Contains(type))
            {
                context.Response.Write(false, string.Format("系统不支持“{0}”", type.GetDescription()));
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Type = type,
                Bank = type.GetDescription()
            });
        }

        /// <summary>
        /// 添加提现银行卡帐号
        /// </summary>
        /// <param name="context"></param>
        private void addcard(HttpContext context)
        {
            this.CheckUserLogin(context);
            this.ShowResult(context, UserAgent.Instance().AddUserAccount(UserInfo.ID, QF("AccountName"), QF("Account"), QF("Type").ToEnum<BankType>(),
                QF("PayPassword"), QF("Bank")), "保存成功");
        }

        /// <summary>
        /// 获取用户已经绑定的银行卡列表
        /// </summary>
        /// <param name="context"></param>
        private void getcardlist(HttpContext context)
        {
            this.CheckUserLogin(context);
            List<BankAccount> list = UserAgent.Instance().GetBankAccountList(UserInfo.ID);
            context.Response.Write(true, this.StopwatchMessage(context),
            this.ShowResult(list, t => new
            {
                t.ID,
                Account = t.ToString(),
                Time = WebAgent.GetTimeDiff(t.CreateAt),
                t.Type,
                t.IsWithdraw
            }));
        }

        /// <summary>
        /// 登录日志
        /// </summary>
        /// <param name="context"></param>
        private void loginlog(HttpContext context)
        {
            this.CheckUserLogin(context);
            var list = BDC.UserLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.AdminID == 0 && t.CreateAt > DateTime.Now.Date.AddDays(-7)).OrderByDescending(t => t.ID);

            context.Response.Write(true, this.StopwatchMessage(context),
                this.ShowResult(list, t => new
                {
                    t.ID,
                    CreateAt = t.CreateAt.ToString("yyyy-MM-dd HH:mm"),
                    IP = WebAgent.HiddenIP(t.IP),
                    IPAddress = UserAgent.Instance().GetIPAddress(t.IP)
                }));
        }

        /// <summary>
        /// 保存用户上传的头像
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void face(HttpContext context)
        {
            if (UserInfo != null && string.IsNullOrEmpty(QF("_tb_token_")))
            {
                string data = QF("data");
                Regex regex = new Regex(@"^data:image/(?<Type>\w+);base64,");
                if (regex.IsMatch(data))
                {
                    string ext = regex.Match(data).Groups["Type"].Value;
                    data = regex.Replace(data, string.Empty);
                    byte[] file = Convert.FromBase64String(data);
                    this.ShowResult(context, UserAgent.Instance().UpdateUserFace(UserInfo.ID, file, ext), "头像上传成功");
                }
                else
                {
                    this.ShowResult(context, UserAgent.Instance().UpdateUserFace(UserInfo.ID, context.Request.Files.Count == 0 ? null : context.Request.Files[0]), "头像上传成功");
                }
            }
            else
            {
                context.Response.ContentEncoding = Encoding.GetEncoding("GBK");
                int userId = UserAgent.Instance().GetUserID(QF("_tb_token_", Guid.Empty));

                if (UserAgent.Instance().UpdateUserFace(userId, context.Request.Files["Filedata"]))
                {
                    context.Response.Write("{\"isSuccess\":true,\"msg\":\"头像保存成功\",\"erCode\":\"\"}");
                }
                else
                {
                    context.Response.Write("{\"isSuccess\":false,\"msg\":\"" + UserAgent.Instance().Message() + "\",\"erCode\":\"\"}");
                }
            }
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="context"></param>
        private void getsecretkey(HttpContext context)
        {
            if (UserInfo.SecretKey != Guid.Empty)
            {
                context.Response.Write(false, "您已经设定了谷歌验证码，如需取消请联系客服");
            }

            string key = Guid.NewGuid().ToString("N");
            SetupCode info = new GoogleAuthenticator().GenerateSetupCode(SiteInfo.ID.ToString(), UserInfo.UserName, key);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Key = key,
                Code = info.QrCodeSetupImageUrl
            });
        }

        /// <summary>
        /// 保存谷歌验证码
        /// </summary>
        /// <param name="context"></param>
        private void savesecretkey(HttpContext context)
        {
            string key = QF("Key");
            string code = QF("Code");
            this.ShowResult(context, UserAgent.Instance().SaveSecretKey(UserInfo.ID, key, code), "保存成功");
        }

        #region ========== 手机验证 ==========


        /// <summary>
        /// 获取用户的手机号码
        /// </summary>
        /// <param name="context"></param>
        private void getmobile(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Mobile = string.IsNullOrEmpty(UserInfo.Mobile) ? "" : WebAgent.HiddenMobile(UserInfo.Mobile)
            });
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="context"></param>
        private void sendcode(HttpContext context)
        {
            string mobile = QF("Mobile");
            if (!UserAgent.Instance().CheckMobile(UserInfo.ID, mobile))
            {
                context.Response.Write(false, UserAgent.Instance().Message());
            }
            if (string.IsNullOrEmpty(mobile))
            {
                mobile = UserInfo.Mobile;
            }
            if (!string.IsNullOrEmpty(UserInfo.Mobile) && mobile != UserInfo.Mobile)
            {
                context.Response.Write(false, "手机号码与您当前绑定的手机号码不一致");
            }

            this.ShowResult(context, SiteAgent.Instance().SendCode(mobile), "发送成功");
        }

        /// <summary>
        /// 保存手机
        /// </summary>
        /// <param name="context"></param>
        private void mobile(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().SaveUserMobile(UserInfo.ID, QF("Mobile"), QF("Code")), "手机号码绑定成功");
        }

        #endregion

        #region ==========  站内信  ===========

        /// <summary>
        /// 站内信列表
        /// </summary>
        /// <param name="context"></param>
        private void messagelist(HttpContext context)
        {
            IQueryable<UserMessage> list = BDC.UserMessage.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.Title,
                t.CreateAt,
                t.IsRead
            }));
        }

        /// <summary>
        /// 读取短信
        /// </summary>
        /// <param name="context"></param>
        private void messageinfo(HttpContext context)
        {
            UserMessage message = UserAgent.Instance().GetMessageInfo(QF("id", 0));
            if (message == null || message.UserID != UserInfo.ID)
            {
                context.Response.Write(false, "编号错误");
            }
            if (!message.IsRead) UserAgent.Instance().MessageRead(message.ID);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                message.ID,
                message.Title,
                message.Content,
                message.CreateAt
            });
        }

        /// <summary>
        /// 删除站内信
        /// </summary>
        /// <param name="context"></param>
        private void messagedelete(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().MessageDelete(QF("id", 0)), "删除成功");
        }

        #endregion



    }
}
