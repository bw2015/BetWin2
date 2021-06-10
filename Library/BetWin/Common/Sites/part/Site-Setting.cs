using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Web;
using System.Reflection;

using BW.Agent;

using BW.Common.Admins;
using BW.GateWay.SMS;
using SP.Studio.Core;
using SP.Studio.Xml;
using SP.Studio.Model;

namespace BW.Common.Sites
{
    partial class Site
    {
        private SiteSetting _setting;

        public SiteSetting Setting
        {
            get
            {
                if (_setting == null)
                {
                    _setting = new SiteSetting(this.SettingString);
                }
                return _setting;
            }
            set
            {
                _setting = value;
                this.SettingString = _setting.ToString();
            }
        }

        public class SiteSetting : SettingBase
        {
            public SiteSetting() : base() { }

            public SiteSetting(string setting) : base(setting) { }

            /// <summary>
            /// 最小返点
            /// </summary>
            public int MinRebate { get; set; }
            /// <summary>
            /// 最大返点
            /// </summary>
            public int MaxRebate { get; set; }
            /// <summary>
            /// 是否允许开同级号
            /// </summary>
            public bool IsSameRebate { get; set; }

            /// <summary>
            /// 默认密码
            /// </summary>
            public string DefaultPassword { get; set; }

            /// <summary>
            /// 可绑定的银行卡数量
            /// </summary>
            public int MaxCard { get; set; }

            /// <summary>
            /// 新添加的账户的冻结时间
            /// </summary>
            public int CardTime { get; set; }

            /// <summary>
            /// 允许提现的开始时间
            /// </summary>
            public string WithdrawTime1 { get; set; }

            /// <summary>
            /// 允许提现的结束时间
            /// </summary>
            public string WithdrawTime2 { get; set; }


            /// <summary>
            /// 提现时间说明
            /// </summary>
            public string WithdrawTime
            {
                get
                {
                    if (this.WithdrawTime1 == this.WithdrawTime2) return "无限制";

                    TimeSpan time1 = TimeSpan.Parse(this.WithdrawTime1);
                    TimeSpan time2 = TimeSpan.Parse(this.WithdrawTime2);

                    return string.Format("{0}～{2}{1}", this.WithdrawTime1, this.WithdrawTime2, time2 < time1 ? "次日" : "");
                    //return string.Format("{0}:00～{1}{2}:00",
                    //    this.WithdrawTime1.ToString().PadLeft(2, '0'),
                    //    this.WithdrawTime2 > 24 ? "次日" : "",
                    //    (this.WithdrawTime2 > 24 ? this.WithdrawTime2 - 24 : this.WithdrawTime2).ToString().PadLeft(2, '0'));
                }
            }

            /// <summary>
            /// 当前是否在系统允许的提款时间内
            /// </summary>
            public bool IsWithdrawTime
            {
                get
                {

                    TimeSpan time1;
                    if (!TimeSpan.TryParse(this.WithdrawTime1, out time1)) return true;

                    TimeSpan time2;
                    if (!TimeSpan.TryParse(this.WithdrawTime2, out time2)) return true;

                    if (time1 == time2) return false;

                    int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

                    int t1 = (int)time1.TotalMinutes;
                    int t2 = (int)time2.TotalMinutes;


                    if (t2 > t1 && now >= t1 && now <= t2) return true;
                    if (t2 < t1 && (now >= t1 || now <= t2)) return true;

                    return false;
                }
            }

            /// <summary>
            /// 系统支持的提现银行
            /// </summary>
            public string WithdrawBank { get; set; }

            /// <summary>
            /// 必须先绑定了银行卡信息方可进行充值操作
            /// </summary>
            public bool RechargeNeedBank { get; set; }

            /// <summary>
            /// 支持的提现银行枚举
            /// </summary>
            public BankType[] WithdrawBankList
            {
                get
                {
                    if (string.IsNullOrEmpty(WithdrawBank)) return null;
                    string[] bank = Enum.GetNames(typeof(BankType));
                    return this.WithdrawBank.Split(',').Where(t => !string.IsNullOrEmpty(t) && bank.Contains(t)).Select(t => t.ToEnum<BankType>()).ToArray();
                }
            }

            /// <summary>
            /// 允许提现的最小金额
            /// </summary>
            public int WithdrawMin { get; set; }

            /// <summary>
            /// 允许提现的最大金额
            /// </summary>
            public int WithdrawMax { get; set; }

            /// <summary>
            /// 提现拆单的单位金额
            /// </summary>
            public int WithdrawUnit { get; set; }

            /// <summary>
            /// 单日可以提现的次数
            /// </summary>
            public int WithdrawCount { get; set; }

            /// <summary>
            /// 同名的提现开户名最多可用于几个账户
            /// </summary>
            public int SameAccountName { get; set; }

            /// <summary>
            /// 默认的注册用户邀请编号
            /// </summary>
            public string RegisterInvite { get; set; }

            /// <summary>
            /// 客服服务器地址，多个地址用逗号隔开，如果为空则调用当前域名
            /// </summary>
            public string ServiceServer { get; set; }

            /// <summary>
            /// 第三方客服系统地址
            /// </summary>
            public string CustomerServer { get; set; }


            private string _lotteryMode = "元,角,分,厘";
            /// <summary>
            /// 彩票的资金模式
            /// </summary>
            public string LotteryMode
            {
                get
                {
                    return _lotteryMode;
                }
                set
                {
                    _lotteryMode = value;
                }
            }

            /// <summary>
            /// 短信网关类型
            /// </summary>
            public SMSProvider SMS { get; set; }

            /// <summary>
            /// 短信网关账户名
            /// </summary>
            public string SMSUser { get; set; }

            /// <summary>
            /// 短信网关密码
            /// </summary>
            public string SMSPass { get; set; }

            /// <summary>
            /// 验证码的有效时间(秒）
            /// </summary>
            public int SMSCodeTime { get; set; }

            /// <summary>
            /// 同一个手机号码重复发送需要等待的时间（秒）
            /// </summary>
            public int SendDelay { get; set; }

            /// <summary>
            /// 系统自动锁定未绑定银行卡且没有资金流水超过设定天数的账户，为0表示不执行该锁定条件
            /// </summary>
            public int LockNoBank { get; set; }

            /// <summary>
            /// 消费流水（为0表示不计算消费流水）
            /// </summary>
            public decimal Turnover { get; set; }

            private int _expireDay = 90;

            /// <summary>
            /// 前台显示的数据有效期
            /// </summary>
            public int ExpireDay
            {
                get
                {
                    return this._expireDay;
                }
                set
                {
                    _expireDay = value;
                }
            }

            /// <summary>
            /// 合买发起人的最低认购值
            /// </summary>
            public decimal UnitedMin { get; set; }

            /// <summary>
            /// 安卓APP下载路径
            /// </summary>
            public string APPAndroid { get; set; }

            /// <summary>
            /// 苹果APP下载路径
            /// </summary>
            public string APPIOS { get; set; }

            /// <summary>
            /// PC客户端下载路径
            /// </summary>
            public string APPPC { get; set; }

            /// <summary>
            /// APP下载路径
            /// </summary>
            public string APP { get; set; }

            /// <summary>
            /// APP最新版本
            /// </summary>
            public string APPVersion { get; set; }

            /// <summary>
            /// 默认的微信进入地址
            /// </summary>
            public string Wechat { get; set; }

            /// <summary>
            /// 挂机的下载地址
            /// </summary>
            public string Guaji { get; set; }

            /// <summary>
            /// 彩票的有效用户投注量
            /// </summary>
            public int EffectUser { get; set; }

            /// <summary>
            /// 最大有效投注奖金组（超过该奖金组的按照返点操作）
            /// </summary>
            public int MaxBetRebate { get; set; }

            /// <summary>
            /// API密钥
            /// </summary>
            public string APIKey { get; set; }

            private string _moneyType;
            /// <summary>
            /// 资金类型的参数设定
            /// </summary>
            public string MoneyType { get { return _moneyType; } set { _moneyType = value; this._moneyTypeSetting = null; } }

            private List<MoneyTypeSetting> _moneyTypeSetting;
            public List<MoneyTypeSetting> MoneyTypeSetting
            {
                get
                {
                    if (_moneyTypeSetting == null)
                    {
                        _moneyTypeSetting = new List<MoneyTypeSetting>();
                        if (string.IsNullOrEmpty(MoneyType)) return _moneyTypeSetting;
                        XElement root = XElement.Parse(this.MoneyType);
                        foreach (XElement item in root.Elements("item"))
                        {
                            _moneyTypeSetting.Add(new MoneyTypeSetting(item));
                        }
                        return _moneyTypeSetting;
                    }
                    return _moneyTypeSetting;
                }
            }

            /// <summary>
            /// 保存一个资金类型的参数设定（仅存入SiteSetting.MoneyType 字段，不存入数据库）
            /// </summary>
            /// <param name="setting"></param>
            public void SaveMoneyTypeSetting(MoneyTypeSetting setting)
            {
                if (!this.MoneyTypeSetting.Exists(t => t.ID == setting.ID))
                {
                    this.MoneyTypeSetting.Add(setting);
                }

                this.MoneyType = string.Concat("<root>", string.Join(string.Empty, this.MoneyTypeSetting.Select(t => t.ToString())), "</root>");
            }

            /// <summary>
            /// 保存资金参数的设定值（仅存入SiteSetting.MoneyType 字段，不存入数据库）
            /// </summary>
            /// <param name="id">资金类型值（数字）</param>
            /// <param name="name">要修改的字段</param>
            /// <param name="value">字段值</param>
            /// <returns></returns>
            public bool SaveMoneyTypeSetting(int id, string name, string value)
            {
                XElement root = XElement.Parse(this.MoneyType);
                XElement item = root.Elements("item").Where(t => t.GetAttributeValue("ID", 0) == id).FirstOrDefault();
                if (item == null) return false;
                switch (name)
                {
                    case "NoTrunover":
                        item.SetAttributeValue("NoTrunover", value == "1");
                        break;
                }

                this.MoneyType = root.ToString();
                return true;
            }

        }

        /// <summary>
        /// 资金类型的参数设定
        /// </summary>
        public struct MoneyTypeSetting
        {
            public MoneyTypeSetting(EnumObject obj)
            {
                this.ID = obj.ID;
                this.Key = obj.Name;
                this.Name = obj.Description;
                this.NoTrunover = false;
            }

            public MoneyTypeSetting(XElement item)
            {
                this.ID = item.GetAttributeValue("ID", 0);
                this.Key = item.GetAttributeValue("Key");
                this.Name = item.GetAttributeValue("Name");
                this.NoTrunover = item.GetAttributeValue("NoTrunover", false);
            }

            /// <summary>
            /// 值
            /// </summary>
            public int ID;

            /// <summary>
            /// 枚举标识
            /// </summary>
            public string Key;

            /// <summary>
            /// 名称
            /// </summary>
            public string Name;

            /// <summary>
            /// 不需要消费流水
            /// </summary>
            public bool NoTrunover;

            public override string ToString()
            {
                XElement item = new XElement("item");
                item.SetAttributeValue("ID", this.ID);
                item.SetAttributeValue("Key", this.Key);
                item.SetAttributeValue("Name", this.Name);
                item.SetAttributeValue("NoTrunover", this.NoTrunover);
                return item.ToString();
            }
        }

        /// <summary>
        /// 站点数据的前台可查时间
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return DateTime.Now.AddDays(this.Setting.ExpireDay * -1).Date;
            }
        }

        /// <summary>
        /// 获取当前管理员能够查看的站点参数
        /// </summary>
        /// <param name="admin"></param>
        /// <returns></returns>
        public string ToJson(Admin admin)
        {
            StringBuilder sb = new StringBuilder();

            XElement root = AdminAgent.Instance().GetAdminPermission();

            sb.Append("{")
                .AppendFormat("\"ID\":{0},", this.ID)
                .AppendFormat("\"Name\":\"{0}\",", this.Name)
                .AppendFormat("\"FaceShow\":\"{0}\",", admin.FaceShow)
                .AppendFormat("\"Description\":\"{0}\",", this.Description)
                .AppendFormat("\"CreateAt\":\"{0}\",", this.CreateAt)
                .AppendFormat("\"Status\":\"{0}\",", this.Status)
                .AppendFormat("\"StopDesc\":\"{0}\",", HttpUtility.JavaScriptStringEncode(this.StopDesc))
                .AppendFormat("\"Setting\":{0},", this.Setting.ToJson())
                .AppendFormat("\"MoneyType\":{0},", string.Concat("{", string.Join(",", this.Setting.MoneyTypeSetting.Select(t => string.Format("\"{0}\":\"{1}\"", t.ID, t.Name))), "}"))
                .AppendFormat("\"Menu\":[{0}]", string.Join(",", AdminAgent.Instance().GetAdminPermission(admin.ID).Select(t => t.ToString())))
                .Append("}");

            return sb.ToString();
        }
    }
}
