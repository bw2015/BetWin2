using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using BW.Common.Lottery;

using SP.Studio.Web;
using BW.Agent;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 全局的中奖名单
    /// </summary>
    public struct RewardTip
    {
        public RewardTip(DataRow dr)
        {
            this.UserName = (string)dr["UserName"];
            this.Reward = (decimal)dr["Reward"];
            this.Type = (LotteryType)dr["Type"];
        }

        public string UserName;

        public decimal Reward;

        public LotteryType Type;

        public override string ToString()
        {
            return string.Concat("{",
                string.Format("\"UserName\":\"{0}\",\"Reward\":{1},\"Type\":\"{2}\"",
                WebAgent.HiddenName(this.UserName), this.Reward, LotteryAgent.Instance().GetLotteryName(this.Type)
                ), "}");
        }
    }
}
