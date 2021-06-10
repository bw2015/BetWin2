using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using System.Resources;

using BW.Common.Users;
using SP.Studio.Core;
using SP.Studio.Xml;
using BW.Agent;


namespace BW.GateWay.Planning
{
    /// <summary>
    /// 活动的规划基类
    /// </summary>
    public abstract class IPlan
    {
        #region ============ 系统常量  ===============

        /// <summary>
        /// 最低金额
        /// </summary>
        public const string KEY_MINMONEY = "MinMoney";

        #endregion

        private static XElement _setting;
        /// <summary>
        /// 资源文件配置
        /// </summary>
        public static XElement setting
        {
            get
            {
                if (_setting == null)
                {
                    _setting = XElement.Parse(BW.Resources.Res.Plan);
                }
                return _setting;
            }
        }

        /// <summary>
        /// 当前活动类型的系统配置
        /// </summary>
        protected XElement Setting
        {
            get
            {
                return setting.Elements("item").Where(t => t.GetAttributeValue("PlanType") == this.Type.ToString()).FirstOrDefault();
            }
        }

        public IPlan()
        {
            this.Type = this.GetType().Name.ToEnum<PlanType>();
            XElement root = this.Setting;
            if (root == null) return;
            this.Type = root.GetAttributeValue("PlanType").ToEnum<PlanType>();
            this.MoneyType = root.GetAttributeValue("MoneyType").ToEnum<MoneyLog.MoneyType>();
            this.Name = root.GetAttributeValue("Name");
            this.Description = root.GetAttributeValue("Description");

            this.SettingList = new List<ItemSetting>();
            foreach (XElement item in root.Elements())
            {
                this.SettingList.Add(new ItemSetting(item));
            }

            this.Value = this.SettingList.ToDictionary(t => t.Key, t => t.Value);
        }

        public IPlan(XElement root)
            : this()
        {
            XElement setting = this.Setting;
            foreach (XElement item in root.Elements())
            {
                string name = item.Name.ToString();
                if (this.Value.ContainsKey(name))
                {
                    this.Value[name] = item.GetValue(null, this.Value[name]);
                }
            }
        }

        /// <summary>
        /// 活动类型
        /// </summary>
        public PlanType Type { get; private set; }

        /// <summary>
        /// 资金类型
        /// </summary>
        public MoneyLog.MoneyType MoneyType { get; private set; }

        /// <summary>
        /// 活动名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 设定的值
        /// </summary>
        public Dictionary<string, decimal> Value { get; set; }

        /// <summary>
        /// 可设定值的列表
        /// </summary>
        public List<ItemSetting> SettingList { get; private set; }


        /// <summary>
        /// 返回编辑资料所需要的json
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Type\":\"{0}\",", this.Type)
                .AppendFormat("\"PlanType\":\"{0}\",", this.Type)
                .AppendFormat("\"Name\":\"{0}\",", this.Name)
                .AppendFormat("\"Description\":\"{0}\",", this.Description)
                .AppendFormat("\"MoneyType\":\"{0}\",", this.MoneyType.GetDescription())
                .AppendFormat("\"Setting\":[{0}]", string.Join(",", this.SettingList.Select(t => t.ToString(this.Value[t.Key]))))
                .Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 返回不特定的值
        /// </summary>
        /// <returns></returns>
        public virtual decimal GetValue(params decimal[] values)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 活动类型
    /// </summary>
    public enum PlanType : byte
    {
        /// <summary>
        /// 真人游戏的上级返水   0
        /// </summary>
        [Description("真人游戏上级返水")]
        VideoGameAgent = 0,
        /// <summary>
        /// 电子游戏的上级返水   1
        /// </summary>
        [Description("电子游戏上级返水")]
        SlotGameAgent = 1,
        /// <summary>
        /// 彩票的上级返点（点差） 2
        /// </summary>
        [Description("彩票投注返点")]
        LotteryBetAgent = 2,
        /// <summary>
        /// 代理日工资   3
        /// </summary>
        [Description("总代日工资")]
        WagesAgent = 3,
        /// <summary>
        /// 契约工资
        /// </summary>
        [Description("契约工资")]
        WagesContract = 103,
        /// <summary>
        /// 首充奖励
        /// </summary>
        [Description("首充奖励")]
        FirstRecharge = 4,
        /// <summary>
        /// 消费返点
        /// </summary>
        [Description("消费返点")]
        LotteryConsumption = 5,
        /// <summary>
        /// 绑定银行卡赠送
        /// </summary>
        [Description("绑定银行卡彩金")]
        BankAccount = 6,
        /// <summary>
        /// 彩票单日亏损佣金
        /// </summary>
        [Description("彩票单日亏损佣金")]
        LotteryLossBrokerage = 7,
        /// <summary>
        /// 真人游戏会员反水
        /// </summary>
        [Description("真人游戏会员反水")]
        VideoGame = 8,
        /// <summary>
        /// 电子游戏会员反水
        /// </summary>
        [Description("电子游戏会员反水")]
        SlotGame = 9,
        /// <summary>
        /// 体育游戏代理返水
        /// </summary>
        [Description("体育代理返水")]
        SportGameAgent = 10,
        /// <summary>
        /// 体育游戏会员返水
        /// </summary>
        [Description("体育会员返水")]
        SportGame = 11,
        /// <summary>
        /// 针对总代的分红
        /// </summary>
        [Description("分红")]
        Bonus = 12,
        /// <summary>
        /// 代理与下级签订的分红
        /// </summary>
        [Description("契约分红")]
        BonusContract = 112,
        /// <summary>
        /// 总代的挂单工资
        /// </summary>
        [Description("挂单工资")]
        LossWages = 13,
        /// <summary>
        /// 代理与下级签订的契约挂单工资
        /// </summary>
        [Description("契约挂单工资")]
        LossWagesContract = 113,
        /// <summary>
        /// 第三方游戏的日工资
        /// </summary>
        [Description("第三方游戏工资")]
        GameWages = 14,
        /// <summary>
        /// 第三方游戏的契约日工资
        /// </summary>
        [Description("契约游戏工资")]
        GameWagesContract = 114,
        /// <summary>
        /// 单层分红
        /// </summary>
        [Description("单层分红")]
        SingleBonus = 15
    }

    /// <summary>
    /// 单项的设定值
    /// </summary>
    public struct ItemSetting
    {
        public ItemSetting(XElement item)
        {
            this.Key = item.Name.ToString();
            this.Name = item.GetAttributeValue("Name");
            this.Description = item.GetAttributeValue("Description");
            this.Value = item.GetValue(null, decimal.Zero);
        }

        /// <summary>
        /// 类型
        /// </summary>
        public string Key;

        public string Name;

        public string Description;

        public decimal Value;

        public string ToString(decimal value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Key\":\"{0}\",", this.Key)
                .AppendFormat("\"Name\":\"{0}\",", this.Name)
                .AppendFormat("\"Description\":\"{0}\",", this.Description)
                .AppendFormat("\"Value\":\"{0}\"", value)
                .Append("}");
            return sb.ToString();
        }
    }
}
