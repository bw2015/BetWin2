using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BW.Common.Users
{
    /// <summary>
    /// 资金的帐变类型
    /// </summary>
    partial class MoneyLog
    {
        /// <summary>
        /// 资金类型（带有Agent关键词的表示是代理的返佣收入）
        /// 收入为奇数、支出为偶数
        /// </summary>
        public enum MoneyType : byte
        {
            [Description("全部")]
            None = 0,

            #region ========== 收入  ===============
            /// <summary>
            /// 充值
            /// </summary>
            [Description("充值"), MoneyCategory(MoneyCategoryType.Recharge)]
            Recharge = 1,
            /// <summary>
            /// 中奖奖金
            /// </summary>
            [Description("中奖"), MoneyCategory(MoneyCategoryType.Reward, MoneyGameType.Lottery)]
            Reward = 3,
            /// <summary>
            /// 返点 会员投注选择小于本身的奖金模式，可以得到的差值返点
            /// </summary>
            [Description("消费返点"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Lottery, true)]
            Rebate = 5,
            /// <summary>
            /// 下级充值的返佣
            /// </summary>
            [Description("充值返佣"), MoneyCategory(MoneyCategoryType.Agent, MoneyGameType.Site, true)]
            Agent = 7,
            /// <summary>
            /// 上级代为充值
            /// </summary>
            [Description("代充"), MoneyCategory(MoneyCategoryType.Transfer)]
            AgentRecharge = 9,
            /// <summary>
            /// 退回提现
            /// </summary>
            [Description("提现退回"), MoneyCategory(MoneyCategoryType.Withdraw, MoneyGameType.Site, true)]
            WithdrawFaild = 11,
            /// <summary>
            /// 充值可以获得的额外奖励
            /// </summary>
            [Description("充值奖励"), MoneyCategory(MoneyCategoryType.Activity, MoneyGameType.Site, true)]
            RechargeReward = 13,
            /// <summary>
            /// 投注奖励，用于 [rebate_Bet] ,每日累计投注奖励，奖励用户自身
            /// </summary>
            [Description("投注奖励"), MoneyCategory(MoneyCategoryType.Activity, MoneyGameType.Lottery, true)]
            BetReward = 15,
            /// <summary>
            /// 下级的累积消费返点，用于 [rebate_ConsumeAgent]，每天用户的累积投注消费，对上3层代理进行返佣
            /// </summary>
            [Description("下级消费返点"), MoneyCategory(MoneyCategoryType.Agent, MoneyGameType.Lottery, true)]
            AgentConsume = 17,
            /// <summary>
            /// 取消投注
            /// </summary>
            [Description("撤单"), MoneyCategory(MoneyCategoryType.Bet, MoneyGameType.Lottery)]
            BetRevoke = 19,
            /// <summary>
            /// 下级投注返点（按点差）
            /// </summary>
            [Description("下级投注返点"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Lottery, true)]
            BetAgent = 21,
            /// <summary>
            /// 一级代理的工资
            /// </summary>
            [Description("代理工资"), MoneyCategory(MoneyCategoryType.Wages, MoneyGameType.Lottery, true)]
            WagesAgent = 23,

            /// <summary>
            /// 扑克游戏奖金
            /// </summary>
            [Description("扑克游戏奖金"), MoneyCategory(MoneyCategoryType.Reward, MoneyGameType.Site, true)]
            PokerReward = 25,
            /// <summary>
            /// 电子游戏奖金
            /// </summary>
            [Description("电子游艺奖金"), MoneyCategory(MoneyCategoryType.Reward, MoneyGameType.Site, true)]
            GameWin = 27,
            /// <summary>
            /// 总代亏损奖励
            /// </summary>
            [Description("平台分红"), MoneyCategory(MoneyCategoryType.Bonus, MoneyGameType.Lottery, true)]
            TopLossAgent = 29,
            /// <summary>
            /// 单日的代理亏损奖励 对应活动类型  LotteryLossBrokerage = 7
            /// </summary>
            [Description("亏损奖励"), MoneyCategory(MoneyCategoryType.Agent, MoneyGameType.Lottery, true)]
            LossAgent = 31,
            /// <summary>
            /// 绑定提现账户
            /// </summary>
            [Description("绑定银行卡赠送"), MoneyCategory(MoneyCategoryType.Activity, MoneyGameType.Site, true)]
            ActivityBindBank = 33,

            /// <summary>
            /// 转账至主账户
            /// </summary>
            [Description("转账至主账户"), MoneyCategory(MoneyCategoryType.Transfer, MoneyGameType.Site, true)]
            TransferToSite = 35,

            [Description("真人返水"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Video, true)]
            VideoGame = 37,

            [Description("真人下级返水"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Video, true)]
            VideoGameAgent = 39,

            /// <summary>
            /// 本级的电子游戏返水
            /// </summary>
            [Description("电子游戏返水"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Slot, true)]
            SlotGame = 41,

            /// <summary>
            /// 下级电子游戏返水
            /// </summary>
            [Description("电子游戏下级返水"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Slot, true)]
            SlotGameAgent = 43,

            [Description("体育游戏返水"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Sport, true)]
            SportGame = 45,

            [Description("体育游戏下级返水"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Sport, true)]
            SportGameAgent = 47,
            /// <summary>
            /// 彩票日消费达到指定金额的奖励
            /// </summary>
            [Description("消费奖励"), MoneyCategory(MoneyCategoryType.Return, MoneyGameType.Lottery, true)]
            LotteryConsome = 49,

            [Description("合买中奖"), MoneyCategory(MoneyCategoryType.Reward, MoneyGameType.Lottery, true)]
            UnitedReward = 51,

            [Description("合买佣金"), MoneyCategory(MoneyCategoryType.Reward, MoneyGameType.Lottery, true)]
            UnitedCommission = 53,
            /// <summary>
            /// 收到转账
            /// </summary>
            [Description("转账收入"), MoneyCategory(MoneyCategoryType.Transfer)]
            TransferIn = 55,
            /// <summary>
            /// 契约转入
            /// </summary>
            [Description("工资契约转入"), MoneyCategory(MoneyCategoryType.Wages)]
            ContractIn = 57,
            /// <summary>
            /// 上级分红转入
            /// </summary>
            [Description("分红转入"), MoneyCategory(MoneyCategoryType.Bonus, MoneyGameType.Lottery, true)]
            BonusTransferIn = 59,
            /// <summary>
            /// PT游戏奖励
            /// </summary>
            [Description("PT奖金"), MoneyCategory(MoneyCategoryType.Game)]
            GamePTReward = 61,
            /// <summary>
            /// AG游戏盈利
            /// </summary>
            [Description("AG奖金"), MoneyCategory(MoneyCategoryType.Game)]
            GameAGReward = 63,
            /// <summary>
            /// BBIN奖金
            /// </summary>
            [Description("BBIN奖金"), MoneyCategory(MoneyCategoryType.Game)]
            GameBBINReward = 65,
            /// <summary>
            /// 总代挂单工资
            /// </summary>
            [Description("挂单工资"), MoneyCategory(MoneyCategoryType.Wages)]
            LossWagesAgent = 67,
            /// <summary>
            /// 挂单的契约工资
            /// </summary>
            [Description("挂单契约工资转入"), MoneyCategory(MoneyCategoryType.Wages)]
            LossWagesIn = 69,
            /// <summary>
            /// 第三方游戏工资
            /// </summary>
            [Description("游戏工资"), MoneyCategory(MoneyCategoryType.Wages)]
            GameWages = 71,
            /// <summary>
            /// 第三方游戏工资下级的收入
            /// </summary>
            [Description("游戏工资契约转入"), MoneyCategory(MoneyCategoryType.Wages)]
            GameWagesIn = 73,
            /// <summary>
            /// 电竞奖金
            /// </summary>
            [Description("电竞奖金"), MoneyCategory(MoneyCategoryType.Game)]
            GameBWReward = 75,
            /// <summary>
            /// 单次分红，对应活动SingleBonus
            /// </summary>
            [Description("公司分红"), MoneyCategory(MoneyCategoryType.Bonus)]
            SingleBonus = 77,
            #endregion

            #region ========== 支出  =============

            /// <summary>
            /// 投注
            /// </summary>
            [Description("投注"), MoneyCategory(MoneyCategoryType.Bet, MoneyGameType.Lottery)]
            Bet = 2,
            /// <summary>
            /// 提现申请
            /// </summary>
            [Description("提现"), MoneyCategory(MoneyCategoryType.Withdraw)]
            Withdraw = 4,
            /// <summary>
            /// 转账给下级
            /// </summary>
            [Description("转账"), MoneyCategory(MoneyCategoryType.Transfer)]
            Transfer = 6,
            /// <summary>
            /// 管理员扣款
            /// </summary>
            [Description("管理员扣款"), MoneyCategory(MoneyCategoryType.Other)]
            Withhold = 8,
            /// <summary>
            /// 中奖之后退回奖金
            /// </summary>
            [Description("退回奖金"), MoneyCategory(MoneyCategoryType.Reward, MoneyGameType.Lottery)]
            RewardRevoke = 10,
            /// <summary>
            /// 撤销充值
            /// </summary>
            [Description("撤销充值"), MoneyCategory(MoneyCategoryType.Recharge)]
            RechargeRevoke = 12,
            /// <summary>
            /// 扑克游戏投注
            /// </summary>
            [Description("扑克游戏投注"), MoneyCategory(MoneyCategoryType.Bet)]
            Poker = 14,
            /// <summary>
            /// 电子游艺投注
            /// </summary>
            [Description("电子游艺投注"), MoneyCategory(MoneyCategoryType.Bet)]
            GameLose = 16,
            /// <summary>
            /// 账户转账
            /// </summary>
            [Description("转至第三方"), MoneyCategory(MoneyCategoryType.Transfer)]
            TransferToGame = 18,
            /// <summary>
            /// 提现手续费
            /// </summary>
            [Description("提现手续费"), MoneyCategory(MoneyCategoryType.Withdraw)]
            WithdrawFee = 20,
            /// <summary>
            /// 合买投注
            /// </summary>
            [Description("合买投注"), MoneyCategory(MoneyCategoryType.Bet)]
            UnitedBet = 22,
            /// <summary>
            /// 合买保底金额的投注
            /// </summary>
            [Description("合买保底"), MoneyCategory(MoneyCategoryType.Bet)]
            UnitedPackage = 24,
            /// <summary>
            /// 契约转出
            /// </summary>
            [Description("工资契约转出"), MoneyCategory(MoneyCategoryType.Wages)]
            ContractOut = 26,
            /// <summary>
            /// 上级分红转入
            /// </summary>
            [Description("分红转出"), MoneyCategory(MoneyCategoryType.Bonus, MoneyGameType.Lottery, true)]
            BonusTransferOut = 28,
            /// <summary>
            /// 充值手续费（对应的是充值奖励）
            /// </summary>
            [Description("充值手续费"), MoneyCategory(MoneyCategoryType.Recharge, MoneyGameType.Site)]
            RechargeFee = 30,
            /// <summary>
            /// PT游戏投注
            /// </summary>
            [Description("PT投注"), MoneyCategory(MoneyCategoryType.Game)]
            GamePTBet = 32,
            /// <summary>
            /// AG游戏投注
            /// </summary>
            [Description("AG投注"), MoneyCategory(MoneyCategoryType.Game)]
            GameAGBet = 34,
            /// <summary>
            /// BBIN投注
            /// </summary>
            [Description("BBIN投注"), MoneyCategory(MoneyCategoryType.Game)]
            GameBBINBet = 36,
            /// <summary>
            /// 总代挂单工资
            /// </summary>
            [Description("挂单契约工资转出"), MoneyCategory(MoneyCategoryType.Wages)]
            LossWagesOut = 38,
            /// <summary>
            /// 上级的游戏工资契约转出
            /// </summary>
            [Description("游戏工资契约转出"), MoneyCategory(MoneyCategoryType.Wages)]
            GameWagesOut = 40,
            /// <summary>
            /// 电竞投注
            /// </summary>
            [Description("电竞投注"), MoneyCategory(MoneyCategoryType.Game)]
            GameBWBet = 42
            #endregion
        }

        /// <summary>
        /// 资金类型的分类属性
        /// </summary>
        public class MoneyCategoryAttribute : Attribute
        {
            public MoneyCategoryAttribute(MoneyCategoryType type)
                : this(type, MoneyGameType.Site)
            {
            }

            public MoneyCategoryAttribute(MoneyCategoryType type, MoneyGameType game)
            {
                this.Type = type;
                this.Game = game;
            }

            public MoneyCategoryAttribute(MoneyCategoryType type, MoneyGameType game, bool isWithdraw)
            {
                this.Type = type;
                this.Game = game;
                this.IsWithdraw = isWithdraw;
            }

            /// <summary>
            /// 资金分类
            /// </summary>
            public MoneyCategoryType Type { get; set; }

            /// <summary>
            /// 资金的游戏类型
            /// </summary>
            public MoneyGameType Game { get; set; }

            /// <summary>
            /// 可直接提现
            /// </summary>
            public bool IsWithdraw { get; set; }

        }

        /// <summary>
        /// 资金类型的分类
        /// </summary>
        public enum MoneyCategoryType
        {
            /// <summary>
            /// 充值
            /// </summary>
            [Description("充值")]
            Recharge = 1,
            /// <summary>
            /// 提现
            /// </summary>
            [Description("提现")]
            Withdraw = 2,
            /// <summary>
            /// 转账（站内上级对下级、第三方游戏中心转账）
            /// </summary>
            [Description("转账")]
            Transfer = 3,
            /// <summary>
            /// 投注
            /// </summary>
            [Description("投注")]
            Bet = 4,
            /// <summary>
            /// 中奖
            /// </summary>
            [Description("派奖")]
            Reward = 5,
            /// <summary>
            /// 会员返点
            /// </summary>
            [Description("返点")]
            Return = 6,
            /// <summary>
            /// 代理佣金
            /// </summary>
            [Description("佣金")]
            Agent = 7,
            /// <summary>
            /// 活动发放的奖金
            /// </summary>
            [Description("活动")]
            Activity = 8,
            /// <summary>
            /// 日工资
            /// </summary>
            [Description("日工资")]
            Wages = 9,
            /// <summary>
            /// 分红
            /// </summary>
            [Description("分红")]
            Bonus = 10,
            /// <summary>
            /// 外部的第三方游戏
            /// </summary>
            [Description("游戏")]
            Game = 11,
            /// <summary>
            /// 其他
            /// </summary>
            [Description("其他")]
            Other = 200

        }

        /// <summary>
        /// 资金的游戏类型
        /// </summary>
        public enum MoneyGameType
        {
            /// <summary>
            /// 站点
            /// </summary>
            Site = 0,
            /// <summary>
            /// 彩票
            /// </summary>
            Lottery = 1,
            /// <summary>
            /// 视讯真人
            /// </summary>
            Video = 2,
            /// <summary>
            /// 电子游戏
            /// </summary>
            Slot = 3,
            /// <summary>
            /// 体育
            /// </summary>
            Sport = 4
        }

        /// <summary>
        /// 不统计盈亏的类型
        /// </summary>
        public static MoneyLog.MoneyCategoryType[] NO_WIN_CATEGORY
        {
            get
            {
                return new MoneyLog.MoneyCategoryType[] { MoneyLog.MoneyCategoryType.Other, MoneyLog.MoneyCategoryType.Transfer, MoneyLog.MoneyCategoryType.Recharge, MoneyLog.MoneyCategoryType.Withdraw };
            }
        }

    }
}
