using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Users;
using BW.Common.Games;

namespace BW.Common.Reports
{
    /// <summary>
    /// 用户的盈亏报表
    /// </summary>
    public struct UserReport
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId">用户名</param>
        /// <param name="_money">资金记录</param>
        /// <param name="_game">第三方游戏记录</param>
        public UserReport(int userId, Dictionary<MoneyLog.MoneyType, decimal> _money, Dictionary<GameType, decimal> _game)
        {
            this.UserID = userId;
            this.data = new Dictionary<MoneyLog.MoneyCategoryType, decimal>();

            foreach (KeyValuePair<MoneyLog.MoneyType, decimal> item in _money)
            {
                MoneyLog.MoneyCategoryType category = item.Key.GetCategory();
                if (!this.data.ContainsKey(category)) this.data.Add(category, decimal.Zero);
                this.data[category] += item.Value;
            }

            this.game = _game;
        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID;

        /// <summary>
        /// 资金分类数据
        /// </summary>
        public Dictionary<MoneyLog.MoneyCategoryType, decimal> data;

        /// <summary>
        /// 第三方游戏的盈亏
        /// </summary>
        public Dictionary<GameType, decimal> game;

        /// <summary>
        /// 整体盈亏
        /// </summary>
        public decimal Money
        {
            get
            {
                decimal money = (this.data.Where(t => !MoneyLog.NO_WIN_CATEGORY.Contains(t.Key)).Sum(t => (decimal?)t.Value) ?? (decimal?)decimal.Zero).Value;
                if (game!= null && game.Count != 0) money += this.game.Sum(t => t.Value);
                return money;
            }
        }
    }
}
