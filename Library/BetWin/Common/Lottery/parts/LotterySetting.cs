using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using SP.Studio.Core;
using SP.Studio.Data;

namespace BW.Common.Lottery
{
    partial class LotterySetting : CommBase
    {
        public LotterySetting() { }

        public LotterySetting(DataSet ds)
        {
            if (ds.Tables[0].Rows.Count != 1) return;

            DataRow dr = ds.Tables[0].Rows[0];
            this.SiteID = dr.GetValue<int>("SiteID");
            this.Game = dr.GetValue<LotteryType>("Game");
            this.Name = dr.GetValue<string>("GameName");
            this.IsOpen = dr.GetValue<bool>("IsOpen");
            this.NoChase = dr.GetValue<bool>("NoChase");
            this.MaxRebate = dr.GetValue<int>("MaxRebate");
            this.Sort = dr.GetValue<short>("Sort");
            this.RewardPercent = dr.GetValue<decimal>("RewardPercent");
            this.IsManual = dr.GetValue<bool>("IsManual");
            this.MaxBet = dr.GetValue<int>("MaxBet");
            this.CateID = dr.GetValue<int>("CateID");
            this.SinglePercent = dr.GetValue<decimal>("SinglePercent");
            this.SingleReward = dr.GetValue<decimal>("SingleReward");
            this.MaxPercent = dr.GetValue<decimal>("MaxPercent");
        }

        /// <summary>
        /// 默认的彩票设定
        /// </summary>
        /// <param name="type"></param>
        public LotterySetting(LotteryType type)
        {
            this.Game = type;
            this.Name = type.GetDescription();
            this.MaxRebate = SiteInfo == null ? 0 : SiteInfo.Setting.MaxRebate;
        }

        /// <summary>
        /// 彩种的属性
        /// </summary>
        public LotteryAttribute Info
        {
            get
            {
                return this.Game.GetAttribute<LotteryAttribute>();
            }
        }

        /// <summary>
        /// 获取用户在该彩种中可得的奖金
        /// </summary>
        /// <param name="userRebate"></param>
        /// <returns></returns>
        public int GetRebate(int userRebate)
        {
            return Utils.GetRebate(SiteInfo.Setting.MaxRebate, userRebate, this.MaxRebate);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"Game\":\"{0}\",", this.Game);
            sb.AppendFormat("\"Name\":\"{0}\",", this.Name);
            sb.AppendFormat("\"Description\":\"{0}\",", this.Description);
            sb.AppendFormat("\"IsOpen\":{0},", this.IsOpen ? 1 : 0);
            sb.AppendFormat("\"NoChase\":{0},", this.NoChase ? 1 : 0);
            sb.AppendFormat("\"IsManual\":{0},", this.IsManual ? 1 : 0);
            sb.AppendFormat("\"MaxRebate\":{0},", this.MaxRebate);
            sb.AppendFormat("\"RewardPercent\":\"{0}\",", this.RewardPercent.ToString("0.00"));
            sb.AppendFormat("\"MaxBet\":{0},", this.MaxBet);
            sb.AppendFormat("\"Sort\":{0},", this.Sort);
            sb.AppendFormat("\"CateID\":{0},", this.CateID);
            sb.AppendFormat("\"SinglePercent\":{0},", this.SinglePercent);
            sb.AppendFormat("\"SingleReward\":{0},", this.SingleReward);
            sb.AppendFormat("\"MaxPercent\":{0},", this.MaxPercent);
            sb.AppendFormat("\"Info\":{0}", this.Info.ToString());
            sb.Append("}");

            return sb.ToString();
        }
    }
}
