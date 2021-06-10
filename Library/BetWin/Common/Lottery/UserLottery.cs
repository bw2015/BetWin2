using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 用户的彩票设定
    /// </summary>
    public struct UserLottery
    {
        public UserLottery(LotteryType game, string name, string description, short sort, int cateId = 0)
        {
            this.Game = game;
            this.Name = name;
            this.Description = description;
            this.Sort = sort;
            this.CateID = cateId;
        }

        /// <summary>
        /// 从站点彩票设定中获取
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="sort"></param>
        public UserLottery(LotterySetting setting, short sort)
        {
            this.Game = setting.Game;
            this.Name = setting.Name;
            this.Description = setting.Description;
            this.Sort = sort;
            this.CateID = setting.CateID;
        }

        public LotteryType Game;

        public string Name;

        public string Description;

        /// <summary>
        /// 自定义分类
        /// </summary>
        public int CateID;

        /// <summary>
        /// 所属的分类
        /// </summary>
        public LotteryCategory Category
        {
            get
            {
                return this.Game.GetCategory().Cate;
            }
        }

        /// <summary>
        /// 排序值（从小到大）
        /// </summary>
        public short Sort;

        public override string ToString()
        {
            return string.Concat("{", string.Format("\"Game\":\"{0}\",\"Category\":\"{4}\",\"Name\":\"{1}\",\"Description\":\"{2}\",\"Sort\":{3},\"Wechat\":{5},\"CateID\":{6}", this.Game, this.Name, this.Description, this.Sort, this.Category, this.Game.GetCategory().Wechat ? 1 : 0, this.CateID), "}");
        }
    }
}
