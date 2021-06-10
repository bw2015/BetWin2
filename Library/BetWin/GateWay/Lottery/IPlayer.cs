using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using System.Resources;

using SP.Studio.Core;
using SP.Studio.Xml;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery
{
    /// <summary>
    /// 彩票玩法接口
    /// </summary>
    public abstract class IPlayer
    {
        /// <summary>
        /// 所属的限号组
        /// </summary>
        public virtual LimitedType Limited
        {
            get
            {
                return LimitedType.None;
            }
        }

        private XElement _xml;
        /// <summary>
        /// 从XML中获取当前玩法的设置
        /// </summary>
        /// <returns></returns>
        private XElement GetXml()
        {
            if (_xml != null) return _xml;
            ResourceManager rm = new ResourceManager(typeof(BW.Resources.Res));
            XElement xml = XElement.Parse((string)rm.GetObject(this.Type.ToString()));
            _xml = xml.Element(this.GetType().Name);
            return _xml;
        }

        /// <summary>
        /// 分类
        /// </summary>
        public virtual string Group
        {
            get
            {
                return this.GetXml().GetAttributeValue("Category");
            }
        }

        /// <summary>
        /// 二级分类
        /// </summary>
        public virtual string Label
        {
            get
            {
                return this.GetXml().GetAttributeValue("Label");
            }
        }

        /// <summary>
        /// 玩法名称
        /// </summary>
        public virtual string Name
        {
            get
            {
                return this.GetXml().GetAttributeValue("Name");
            }
        }

        /// <summary>
        /// 全部的注数
        /// </summary>
        public virtual int Full
        {
            get
            {
                return this.GetXml().GetAttributeValue("Full", 0);
            }
        }

        /// <summary>
        /// 玩法的介绍
        /// </summary>
        public virtual string Tip
        {
            get
            {
                return this.GetXml().Value;
            }
        }

        /// <summary>
        /// 奖金
        /// </summary>
        public virtual decimal RewardMoney
        {
            get { return decimal.Zero; }
        }

        /// <summary>
        /// 获取自定义的奖金（没有设置返回默认值）
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        protected virtual decimal GetRewardMoney(decimal money)
        {
            if (money == decimal.Zero) return this.RewardMoney;
            return money;
        }

        /// <summary>
        /// 玩法代码
        /// </summary>
        public string Code
        {
            get
            {
                return string.Format("{0}", this.GetType().Name);
            }
        }

        /// <summary>
        /// 获取当前玩法的编号
        /// </summary>
        public int ID
        {
            get
            {
                return int.Parse(this.GetType().Name.Replace("Player", ""));
            }
        }


        /// <summary>
        /// 当前类型可接受的投注号码范围
        /// </summary>
        public virtual string[] InputBall
        {
            get
            {
                return this.Type.GetAttribute<LotteryCategoryAttribute>().Ball.Split(',');
            }
        }

        /// <summary>
        /// 退还本金的奖金
        /// </summary>
        protected const decimal RETURNMONEY = 2M;

        /// <summary>
        /// 当前彩种的类型
        /// </summary>
        public abstract LotteryCategory Type { get; }


        /// <summary>
        /// 投注的号码是否匹配当前玩法
        /// </summary>
        /// <param name="input">投注号码</param>
        public abstract bool IsMatch(string input);


        /// <summary>
        /// 获取选择的号码所对应的注数
        /// </summary>
        /// <param name="input">投注的内容</param>
        /// <returns></returns>
        public abstract int Bet(string input);


        /// <summary>
        /// 获取奖金
        /// </summary>
        /// <param name="input">投注号码</param>
        /// <param name="number">开奖号码</param>
        /// <param name="rewardMoney">自定义的奖金</param>
        /// <returns></returns>
        public abstract decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero);

        /// <summary>
        /// 把投注号码转化成为需要检查的限号队列
        /// </summary>
        /// <param name="input">投注号码</param>
        /// <returns>返回空值表示检查失败</returns>
        public virtual IEnumerable<string> ToLimited(string input)
        {
            return null;
        }

        /// <summary>
        /// 开奖号码是否符合规则
        /// </summary>
        /// <param name="number">开奖号码</param>
        /// <returns></returns>
        public virtual bool IsResult(string number)
        {
            LotteryCategoryAttribute category = this.Type.GetAttribute<LotteryCategoryAttribute>();
            return category.IsMatch(number);
        }

        /// <summary>
        /// 投注号码是否符合规则
        /// </summary>
        /// <param name="number">投注号码（已被分割开的，使用逗号隔开的号码）</param>
        /// <param name="minLength">最少长度</param>
        /// <param name="maxLength">最多长度</param>
        /// <returns></returns>
        public virtual bool IsMatch(string number, int minLength, int maxLength)
        {
            LotteryCategoryAttribute category = this.Type.GetAttribute<LotteryCategoryAttribute>();
            return category.IsMatch(number, minLength, maxLength);
        }

        /// <summary>
        /// 翻译聊天投注类型
        /// </summary>
        /// <param name="content"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public virtual string GetBetChat(string content, out int times)
        {
            throw new NotImplementedException("不支持该投注内容");
        }
        
    }


}
