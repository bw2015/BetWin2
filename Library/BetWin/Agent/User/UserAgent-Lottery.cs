using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using SP.Studio.Data;
using BW.Common.Lottery;
using BW.Common.Users;

namespace BW.Agent
{
    /// <summary>
    /// 彩票操作相关
    /// </summary>
    partial class UserAgent
    {
        /// <summary>
        /// 保存用户的自定义彩种排序
        /// </summary>
        /// <param name="lottery"></param>
        /// <returns></returns>
        public bool UpdateLotterySort(int userId, LotteryType[] lottery)
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    for (short i = 0; i < lottery.Length; i++)
                    {
                        UserLotterySetting setting = new UserLotterySetting()
                        {
                            SiteID = SiteInfo.ID,
                            Game = lottery[i],
                            Sort = i,
                            UserID = userId
                        };
                        if (setting.Exists(db))
                        {
                            setting.Update(db, t => t.Sort);
                        }
                        else
                        {
                            setting.Add(db);
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.Message(ex.Message);
                    db.Rollback();
                    return false;
                }
                db.Commit();
            }
            return true;
        }

        /// <summary>
        /// 获取用户的自定义排序的彩种
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserLotterySetting> GetUserLottery(int userId)
        {
            return BDC.UserLotterySetting.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).OrderBy(t => t.Sort).ToList();
        }
    }
}
