using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;
using System.Data;

namespace BW.Common.Games
{
    /// <summary>
    /// 用户余额视图
    /// </summary>
    public class UserGameCredit
    {
        public UserGameCredit(DataRow dr)
        {
            this.UserID = (int)dr["UserID"];
            this.SiteID = (int)dr["SiteID"];
            this.Credit = new Dictionary<GameType, decimal>();
            foreach (GameType value in Enum.GetValues(typeof(GameType)))
            {
                string fieldName = ((byte)value).ToString();
                if (dr.Table.Columns.Contains(fieldName) && dr[fieldName] != DBNull.Value)
                {
                    this.Credit.Add(value, (decimal)dr[fieldName]);
                }
            }
        }
        public int UserID { get; set; }

        public int SiteID { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        public Dictionary<GameType, decimal> Credit { get; set; }
    }
}
