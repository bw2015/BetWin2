using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Linq;

using BW.GateWay.Planning;

using SP.Studio.Xml;
using SP.Studio.Array;
using SP.Studio.Core;

namespace BW.Common.Users
{
    partial class Contract
    {
        public Contract() { }

        /// <summary>
        /// 继承系统的活动参数配置
        /// </summary>
        /// <param name="type"></param>
        /// <param name="plan"></param>
        public Contract(ContractType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// 契约类型
        /// </summary>
        public enum ContractType : byte
        {
            /// <summary>
            /// 工资契约
            /// </summary>
            [Description("工资契约")]
            WagesAgent = BW.GateWay.Planning.PlanType.WagesAgent,
            /// <summary>
            /// 分红契约
            /// </summary>
            [Description("分红契约")]
            Bouns = BW.GateWay.Planning.PlanType.Bonus,
            /// <summary>
            /// 挂单的工资契约
            /// </summary>
            [Description("挂单契约")]
            LossWages = BW.GateWay.Planning.PlanType.LossWages,
            /// <summary>
            /// 第三方的游戏工资
            /// </summary>
            [Description("游戏工资")]
            GameWages = BW.GateWay.Planning.PlanType.GameWages
        }

        public enum ContractStatus : byte
        {
            [Description("待确认")]
            None,
            [Description("正常")]
            Normal,
            [Description("请求取消")]
            AcceptCancel,
            [Description("已取消")]
            Cancel
        }

        private List<ContractSetting> _setting;
        /// <summary>
        /// 契约可设定参数
        /// </summary>
        public List<ContractSetting> Setting
        {
            get
            {
                if (this._setting == null)
                {
                    this._setting = new List<ContractSetting>();

                    BW.Common.Sites.Planning plan = BW.Agent.SiteAgent.Instance().GetPlanInfo(this.SiteID, this.Type.ToEnum<PlanType>());
                    if (plan == null || !plan.IsOpen) return this._setting;

                    this.loadSetting(plan.PlanSetting);

                }
                return this._setting;
            }
        }

        private Dictionary<string, decimal> _data;
        /// <summary>
        /// 契约数据
        /// </summary>
        public Dictionary<string, decimal> Data
        {
            get
            {
                if (this._data == null)
                {
                    if (!string.IsNullOrEmpty(this.Content))
                    {
                        XElement root = XElement.Parse(this.Content);
                        this._data = root.Elements().ToDictionary(t => t.GetAttributeValue("name"), t => t.GetAttributeValue("value", decimal.Zero));
                    }
                    else
                    {
                        this._data = new Dictionary<string, decimal>();
                    }
                }

                return this._data;
            }
            set
            {
                this._data = value;
                XElement root = new XElement("root");
                foreach (KeyValuePair<string, decimal> item in this._data)
                {
                    XElement t = new XElement("item");
                    t.SetAttributeValue("name", item.Key);
                    t.SetAttributeValue("value", item.Value);
                    root.Add(t);
                }
                this.Content = root.ToString();
            }
        }

        /// <summary>
        /// 加载自身的配置资源
        /// </summary>
        /// <param name="plan">系统活动配置</param>
        internal void loadSetting(IPlan plan)
        {
            this._setting = new List<ContractSetting>();
            Dictionary<string, ItemSetting> planSetting = plan.SettingList.ToDictionary(t => t.Key, t => t);
            Regex regex = new Regex(@"^Money(?<ID>\d+)$");
            string agentKey;
            switch (this.Type)
            {
                case ContractType.WagesAgent:
                    plan.SettingList.ForEach(t =>
                    {
                        if (!regex.IsMatch(t.Key)) return;
                        agentKey = "Agent" + regex.Match(t.Key).Groups["ID"].Value;
                        if (!planSetting.ContainsKey(agentKey)) return;
                        ItemSetting item = planSetting[agentKey];

                        decimal value = this.Data.Get(agentKey, plan.Value[agentKey]);
                        if (value == decimal.Zero) return;

                        this._setting.Add(new ContractSetting(t.Name, plan.Value[t.Key], string.Empty, string.Empty, true));
                        this._setting.Add(new ContractSetting(item.Name, value, string.Format("不能超过{0}", value), agentKey, false));
                    });
                    break;
                case ContractType.Bouns:
                    plan.SettingList.ForEach(t =>
                    {
                        if (!regex.IsMatch(t.Key)) return;
                        agentKey = "Agent" + regex.Match(t.Key).Groups["ID"].Value;
                        string userKey = "User" + regex.Match(t.Key).Groups["ID"].Value;
                        string saleKey = "Sale" + regex.Match(t.Key).Groups["ID"].Value;

                        if (!planSetting.ContainsKey(agentKey)) return;
                        ItemSetting item = planSetting[agentKey];

                        decimal value = this.Data.Get(agentKey, plan.Value[agentKey]);
                        if (value == decimal.Zero) return;

                        this._setting.Add(new ContractSetting(t.Name, plan.Value[t.Key], string.Empty, string.Empty, true));
                        if (planSetting.ContainsKey(userKey))
                        {
                            this._setting.Add(new ContractSetting(planSetting[userKey].Name, plan.Value[userKey], string.Empty, string.Empty, true));
                        }
                        if (planSetting.ContainsKey(saleKey))
                        {
                            this._setting.Add(new ContractSetting(planSetting[saleKey].Name, plan.Value[saleKey], string.Empty, string.Empty, true));
                        }

                        this._setting.Add(new ContractSetting(item.Name, value, string.Format("不能超过{0}", value), agentKey, false));
                    });
                    break;
                case ContractType.LossWages:
                    plan.SettingList.ForEach(t =>
                    {
                        if (!regex.IsMatch(t.Key)) return;
                        agentKey = "Agent" + regex.Match(t.Key).Groups["ID"].Value;
                        string memberKey = "Member" + regex.Match(t.Key).Groups["ID"].Value;
                        if (!planSetting.ContainsKey(agentKey) || !planSetting.ContainsKey(memberKey)) return;
                        ItemSetting item = planSetting[agentKey];
                        ItemSetting member = planSetting[memberKey];

                        decimal value = this.Data.Get(agentKey, plan.Value[agentKey]);
                        if (value == decimal.Zero) return;

                        this._setting.Add(new ContractSetting(t.Name, plan.Value[t.Key], string.Empty, string.Empty, true));
                        this._setting.Add(new ContractSetting(member.Name, plan.Value[memberKey], string.Empty, string.Empty, true));
                        this._setting.Add(new ContractSetting(item.Name, value, string.Format("不能超过{0}", value), agentKey, false));
                    });
                    break;
                case ContractType.GameWages:
                    plan.SettingList.ForEach(t =>
                    {
                        decimal value = this.Data.Get(t.Key, plan.Value[t.Key]);
                        if (value == decimal.Zero) return;
                        this._setting.Add(new ContractSetting(t.Name, value, string.Format("不能超过{0}", value), t.Key, false));
                    });
                    break;
            }
        }

        /// <summary>
        /// 当前可以进行的操作动作类型
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string[] GetAction(int userId)
        {
            string agree = "Agree", inject = "Inject", delete = "Delete";

            string[] action = new string[] { };

            switch (this.Status)
            {
                case Contract.ContractStatus.None:
                    if (this.User2 == userId)
                    {
                        action = new string[] { agree, inject };
                    }
                    break;
                case Contract.ContractStatus.Normal:
                    if (this.User1 == userId)
                    {
                        action = new string[] { delete };
                    }
                    break;
                case Contract.ContractStatus.AcceptCancel:
                    if (this.User2 == userId)
                    {
                        action = new string[] { agree, inject };
                    }
                    break;
            }
            return action;
        }
        /// <summary>
        /// 契约的可设定参数
        /// </summary>
        public struct ContractSetting
        {
            public ContractSetting(string name, decimal maxValue, string description, string key, bool isReadonly)
            {
                this.Name = name;
                this.MaxValue = maxValue;
                this.Description = description;
                this.Key = key;
                this.ReadOnly = isReadonly;
            }

            /// <summary>
            /// 名称
            /// </summary>
            public string Name;

            /// <summary>
            /// 最大值
            /// </summary>
            public decimal MaxValue;

            /// <summary>
            /// 备注信息
            /// </summary>
            public string Description;

            /// <summary>
            /// 保存的字段名字
            /// </summary>
            public string Key;

            /// <summary>
            /// 是否只读
            /// </summary>
            public bool ReadOnly;


            public override string ToString()
            {
                return string.Concat("{",
                    string.Format("\"name\":\"{0}\",", this.Name),
                    string.Format("\"maxvalue\":{0},", this.MaxValue),
                    string.Format("\"description\":\"{0}\",", this.Description),
                    string.Format("\"key\":\"{0}\",", this.Key),
                    string.Format("\"readonly\":{0}", this.ReadOnly ? 1 : 0),
                    "}");
            }
        }
    }
}
