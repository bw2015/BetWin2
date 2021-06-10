using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using BW.Common.Lottery;

using SP.Studio.Core;
using SP.Studio.Array;
using SP.Studio.Json;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 追号
    /// </summary>
    partial class LotteryChase
    {
        public LotteryChase() { }

        /// <summary>
        /// 初始化追号内容
        /// </summary>
        /// <param name="json"></param>
        /// <param name="items"></param>
        public LotteryChase(string json, out LotteryChaseItem[] items)
        {
            items = null;
            Hashtable ht = JsonAgent.GetJObject(json);
            if (ht == null) return;
            this.Type = ht.GetValue("type", string.Empty).ToEnum<LotteryType>();
            this.IsRewardStop = ht.GetValue("stop", 1) == 1;
            this.Content = ht.GetValue("data", string.Empty);

            Hashtable[] chase = JsonAgent.GetJList(ht.GetValue("chase", string.Empty));
            if (chase == null) return;
            items = new LotteryChaseItem[chase.Length];
            for (int i = 0; i < chase.Length; i++)
            {
                items[i] = new LotteryChaseItem(chase[i]);
            }
        }

        /// <summary>
        /// 追号状态
        /// </summary>
        public enum ChaseStatus : byte
        {
            /// <summary>
            /// 正常等待追号（暂未写入订单）
            /// </summary>
            [Description("正常")]
            Normal = 0,
            /// <summary>
            /// 中奖后终止
            /// </summary>
            [Description("中奖终止")]
            Reward = 1,
            /// <summary>
            /// 手工退出
            /// </summary>
            [Description("撤单")]
            Quit = 2,
            /// <summary>
            /// 完成投注
            /// </summary>
            [Description("完成")]
            Finish = 3,
            /// <summary>
            /// 提交追号失败
            /// </summary>
            [Description("失败")]
            Faild = 4,
            /// <summary>
            /// 开奖
            /// </summary>
            [Description("已开奖")]
            IsOpen = 5
        }
    }
}
