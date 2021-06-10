using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Common.Reports;
namespace BW.Common
{
    partial class BetDataContext
    {
        /// <summary>
        /// 用户的日资金变动
        /// </summary>
        public Table<UserDateMoney> UserDateMoney
        {
            get
            {
                return this.GetTable<UserDateMoney>();
            }
        }

        /// <summary>
        /// 第三方游戏报表
        /// </summary>
        public Table<UserDateGame> UserDateGame
        {
            get
            {
                return this.GetTable<UserDateGame>();
            }
        }

        /// <summary>
        /// 团队的日资金报表（包括自己）
        /// </summary>
        public Table<TeamDateMoney> TeamDateMoney
        {
            get
            {
                return this.GetTable<TeamDateMoney>();
            }
        }
    }
}
