using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using SP.Studio.Data.Linq;

using SP.Studio.Data;

using BW.Common.Lottery;
using BW.Common.Admins;

using SP.Studio.Core;
using BW.GateWay.Lottery;

namespace BW.Agent
{
    /// <summary>
    /// 彩票管理
    /// </summary>
    public sealed partial class LotteryAgent : AgentBase<LotteryAgent>
    {
        public LotteryAgent() : base()
        {
            this.openRewardResult.Add(true, 0);
            this.openRewardResult.Add(false, 0);
        }

        /// <summary>
        /// 获取彩票列表（如果没有配置记录则枚举中读取，保持状态为停止）
        /// </summary>
        /// <returns></returns>
        public List<LotterySetting> GetLotteryList()
        {
            return this.GetLotteryList(SiteInfo.ID);
        }

        /// <summary>
        /// 获取系统中所有的彩种设置
        /// </summary>
        /// <returns></returns>
        public List<LotterySetting> GetLotterySettingList()
        {
            return BDC.LotterySetting.ToList();
        }

        public List<LotterySetting> GetLotteryList(int siteId)
        {
            List<LotterySetting> list = BDC.LotterySetting.Where(t => t.SiteID == siteId).OrderByDescending(t => t.Sort).ToList();
            list.AddRange(typeof(LotteryType).ToList().Where(t => !list.Select(p => (int)p.Game).Contains(t.ID)).Select(t => new LotterySetting((LotteryType)t.ID)));
            return list;
        }

        /// <summary>
        /// 获取自定义的彩种名字
        /// </summary>
        public string GetLotteryName(LotteryType type)
        {
            return this.GetLotteryName(type, SiteInfo.ID);
        }

        /// <summary>
        /// 获取自定义彩种的名字（在非web程序中调用）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public string GetLotteryName(LotteryType type, int siteId)
        {
            string name = BDC.LotterySetting.Where(t => t.SiteID == siteId && t.Game == type).Select(t => t.Name).FirstOrDefault();
            if (string.IsNullOrEmpty(name)) return type.GetDescription();
            return name;
        }


        /// <summary>
        /// 获取一条设置信息（如果未配置则new一个缺省值对象,无缓存）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LotterySetting GetLotterySettingInfo(LotteryType type)
        {
            return this.GetLotterySettingInfo(SiteInfo.ID, type);
        }

        /// <summary>
        /// 获取站点设置的彩种信息（适用于非web程序）
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public LotterySetting GetLotterySettingInfo(int siteId, LotteryType type)
        {
            LotterySetting setting = BDC.LotterySetting.Where(t => t.SiteID == siteId && t.Game == type).FirstOrDefault() ?? new LotterySetting(type);
            return setting;
        }

        /// <summary>
        /// 使用sql获取彩种信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public LotterySetting GetLotterySettingInfo(DbExecutor db, int siteId, LotteryType type)
        {
            DataSet ds = db.GetDataSet(CommandType.Text, "SELECT * FROM lot_Setting WHERE SiteID = @SiteID AND Game = @Type",
                NewParam("@SiteID", siteId),
                NewParam("@Type", type));
            if (ds.Tables[0].Rows.Count == 0) return new LotterySetting(type);
            return ds.Fill<LotterySetting>();
        }

        /// <summary>
        /// 保存一个单项设置
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SaveLotterySetting(LotteryType type, Expression<Func<LotterySetting, object>> field, object value)
        {
            PropertyInfo property = field.ToPropertyInfo();
            if (value == null)
            {
                base.Message("{0}值未指定", property.Name);
                return false;
            }
            LotterySetting setting = this.GetLotterySettingInfo(type);
            setting.SiteID = SiteInfo.ID;
            try
            {
                property.SetValue(setting, value, null);

                if (setting.Exists())
                {
                    if (setting.Update(null, field) == 1)
                    {
                        AdminInfo.Log(AdminLog.LogType.Lottery, "修改彩种{0}的{1}属性为{2}", type.GetDescription(), field.GetPropertyInfo().Name, value);
                        return true;
                    }
                    return false;
                }
                else
                {
                    setting.MaxRebate = this.GetLotterySettingRewardPercent(type.GetAttribute<LotteryAttribute>().Cate);
                    AdminInfo.Log(AdminLog.LogType.Lottery, "开启彩种{0}", type.GetDescription());
                    return setting.Add();
                }
            }
            catch (Exception ex)
            {
                base.Message(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取一个彩票分组的最大返奖额
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private int GetLotterySettingRewardPercent(LotteryCategory category)
        {
            int? maxReward = BDC.LotterySetting.Where(t => t.SiteID == SiteInfo.ID && category.GetLottery().Contains(t.Game)).Max(t => (int?)t.MaxRebate);
            return maxReward == null ? SiteInfo.Setting.MaxRebate : maxReward.Value;
        }

        /// <summary>
        /// 设置彩种的最大返奖
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool UpdateLotteryMaxReward(LotteryCategory category, int maxReward)
        {
            if (maxReward < SiteInfo.Setting.MinRebate)
            {
                base.Message("不能小于系统的最低返奖{0}", SiteInfo.Setting.MinRebate);
                return false;
            }
            if (maxReward > SiteInfo.Setting.MaxRebate)
            {
                base.Message("不能大于系统的最高返奖{0}", SiteInfo.Setting.MaxRebate);
                return false;
            }

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                try
                {
                    foreach (LotteryType type in category.GetLottery())
                    {
                        new LotterySetting()
                        {
                            SiteID = SiteInfo.ID,
                            Game = type,
                            MaxRebate = maxReward
                        }.Update(db, t => t.MaxRebate);
                    }

                    db.Commit();
                }
                catch (Exception ex)
                {
                    this.Message(ex.Message);
                    return false;
                }
            }

            AdminInfo.Log(AdminLog.LogType.Lottery, "设置{0}最大返奖为{1}", category.GetDescription(), maxReward);
            return true;
        }

        /// <summary>
        /// 设置系统
        /// </summary>
        /// <param name="game"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool UpdateLotteryMaxBet(LotteryType game, decimal maxBet)
        {
            if (BDC.LotterySetting.Update(new LotterySetting() { MaxBet = maxBet }, t => t.SiteID == SiteInfo.ID && t.Game == game, t => t.MaxBet) != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Lottery, "设置{0}单期限额为{1}", game.GetDescription(), maxBet);
                return true;
            }
            return false;
        }

        public bool UpdateLotterySort(LotteryType game, short sort)
        {
            if (BDC.LotterySetting.Update(new LotterySetting() { Sort = sort }, t => t.SiteID == SiteInfo.ID && t.Game == game, t => t.Sort) != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Lottery, "设置{0}排序值为{1}", game.GetDescription(), sort);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 修改彩种所属分类
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cateid"></param>
        /// <returns></returns>
        public bool UpdateLotteryCategory(LotteryType game, int cateid)
        {
            if (BDC.LotterySetting.Update(new LotterySetting() { CateID = cateid }, t => t.SiteID == SiteInfo.ID && t.Game == game, t => t.CateID) != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Lottery, "设置{0}分类为{1}", game.GetDescription(), cateid);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 修改彩种的单挑比例
        /// </summary>
        public bool UpdateLotterySinglePercent(LotteryType game, decimal singlePercent)
        {
            if (singlePercent >= decimal.One) { base.Message("比例设置，应小于1"); return false; }
            if (BDC.LotterySetting.Update(new LotterySetting() { SinglePercent = singlePercent }, t => t.SiteID == SiteInfo.ID && t.Game == game, t => t.SinglePercent) != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Lottery, "设置{0}单挑比例为{1}", game.GetDescription(), singlePercent.ToString("p"));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 修改彩种的单挑最大奖金
        /// </summary>
        public bool UpdateLotterySingleReward(LotteryType game, decimal singleReward)
        {
            if (BDC.LotterySetting.Update(new LotterySetting() { SingleReward = singleReward }, t => t.SiteID == SiteInfo.ID && t.Game == game, t => t.SingleReward) != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Lottery, "设置{0}单挑最大奖金为{1}", game.GetDescription(), singleReward.ToString("c"));
                return true;
            }
            return false;
        }


        /// <summary>
        /// 修改彩种的全包设定
        /// </summary>
        public bool UpdateLotteryMaxPercent(LotteryType game, decimal maxPercent)
        {
            if (maxPercent >= decimal.One) { base.Message("比例设置，应小于1"); return false; }
            if (BDC.LotterySetting.Update(new LotterySetting() { MaxPercent = maxPercent }, t => t.SiteID == SiteInfo.ID && t.Game == game, t => t.MaxPercent) != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Lottery, "设置{0}的最大全包比例为{1}", game.GetDescription(), maxPercent.ToString("p"));
                return true;
            }
            return false;
        }


        /// <summary>
        /// 获取玩法（系统玩法+站点自定义参数）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<LotteryPlayer> GetPlayerList(LotteryType type)
        {
            return this.GetPlayerList(SiteInfo.ID, type).ToList();
        }

        public IEnumerable<LotteryPlayer> GetPlayerList(int siteId, LotteryType type)
        {
            List<LotteryPlayer> list = BDC.LotteryPlayer.Where(t => t.SiteID == siteId && t.Type == type).ToList();
            list.AddRange(this.GetPlayerList().Where(t => t.Type == type && !list.Select(p => p.Code).Contains(t.Code)));
            return list.OrderByDescending(t => t.Sort);
        }

        /// <summary>
        /// 获取系统中的所有玩法（不读取自己的）
        /// </summary>
        /// <returns></returns>
        public List<LotteryPlayer> GetPlayerList()
        {
            List<LotteryPlayer> list = new List<LotteryPlayer>();

            Type[] types = this.GetType().Assembly.GetTypes().Where(t => t.IsBaseType(typeof(IPlayer)) && !t.IsAbstract).OrderBy(t => t.Name).ToArray();

            foreach (Type type in types)
            {
                IPlayer player = (IPlayer)Activator.CreateInstance(type);
                foreach (LotteryType game in player.Type.GetLottery())
                {
                    list.Add(new LotteryPlayer(player, game));
                }
            }
            return list;
        }

        /// <summary>
        /// 获取数据库中设置的玩法列表
        /// </summary>
        /// <returns></returns>
        public List<LotteryPlayer> GetLotteryPlayerList()
        {
            return BDC.LotteryPlayer.ToList();
        }

        /// <summary>
        /// 根据代码获取玩法
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public LotteryPlayer GetPlayerInfo(string code)
        {
            LotteryPlayer play = BDC.LotteryPlayer.Where(t => t.SiteID == SiteInfo.ID && t.Code == code).FirstOrDefault();
            if (play != null) return play;
            LotteryType type;
            IPlayer player = PlayerFactory.GetPlayer(code, out type);
            if (player == null)
            {
                base.Message("玩法代码错误");
                return null;
            }
            return new LotteryPlayer(player, type);
        }

        /// <summary>
        /// 根据站点定义的玩法编号获取玩法对象
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public LotteryPlayer GetPlayerInfo(int playerId)
        {
            if (SiteInfo == null)
            {
                return BDC.LotteryPlayer.Where(t => t.ID == playerId).FirstOrDefault();
            }
            return SiteInfo.LotteryPlayerInfo.ContainsKey(playerId) ? SiteInfo.LotteryPlayerInfo[playerId] : null;
        }

        /// <summary>
        /// 根据编号获取玩法ID（从缓存中获取）
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public int GetPlayerID(string code)
        {
            var list = SiteInfo.LotteryPlayerInfo.Where(t => t.Value.Code == code);
            if (list.Count() == 0) return 0;
            return list.FirstOrDefault().Value.ID;
        }

        /// <summary>
        /// 根据编号获取玩法ID（从数据库中获取，适用于非web程序）
        /// </summary>
        /// <param name="code"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetPlayerID(string code, int siteId)
        {
            if (SiteInfo != null) return this.GetPlayerID(code);

            int? playerId = BDC.LotteryPlayer.Where(t => t.SiteID == siteId && t.Code == code).Select(t => (int?)t.ID).FirstOrDefault();
            return playerId == null ? 0 : playerId.Value;
        }

        /// <summary>
        /// 保存玩法的一个单独属性
        /// </summary>
        /// <param name="player">玩法对象</param>
        /// <param name="field">要保存的字段</param>
        /// <param name="value">要保存的值</param>
        /// <returns></returns>
        public bool SaveLotteryPlayer(LotteryPlayer player, Expression<Func<LotteryPlayer, object>> field, object value)
        {
            player.SiteID = SiteInfo.ID;
            PropertyInfo property = field.ToPropertyInfo();
            if (value == null)
            {
                base.Message("{0}值未指定", property.Name);
                return false;
            }
            try
            {
                property.SetValue(player, value, null);
                if (player.ID != 0)
                {
                    return player.Update(null, field) == 1;
                }
                else
                {
                    return player.Add();
                }
            }
            catch (Exception ex)
            {
                base.Message(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取系统中设定的彩期模板
        /// </summary>
        /// <returns></returns>
        public Dictionary<LotteryType, List<TimeTemplate>> GetTimeTemplateList()
        {
            List<TimeTemplate> list = BDC.TimeTemplate.ToList();
            Dictionary<LotteryType, List<TimeTemplate>> dic = new Dictionary<LotteryType, List<TimeTemplate>>();
            list.GroupBy(t => t.Type).Select(t => t.Key).ToList().ForEach(t =>
            {
                dic.Add(t, list.Where(p => p.Type == t).OrderBy(p => p.Seconds).ToList());
            });
            return dic;
        }

        /// <summary>
        /// 获取特殊彩种的开奖时间
        /// </summary>
        /// <returns></returns>
        public Dictionary<LotteryType, List<StartTime>> GetStartTimeList()
        {
            List<StartTime> list = BDC.StartTime.Where(t => t.StartAt > DateTime.Now.AddDays(-7) && t.StartAt < DateTime.Now.AddDays(7)).OrderBy(t => t.StartAt).ToList();
            Dictionary<LotteryType, List<StartTime>> dic = new Dictionary<LotteryType, List<StartTime>>();
            foreach (LotteryType type in list.GroupBy(t => t.Type).Select(t => t.Key))
            {
                dic.Add(type, list.Where(t => t.Type == type).ToList());
            }
            return dic;
        }

        /// <summary>
        /// 获取订单编号
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public LotteryOrder GetLotteryOrderInfo(int orderId)
        {
            return BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID && t.ID == orderId).FirstOrDefault();
        }

        /// <summary>
        /// 从分布式表里面获取订单信息
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public LotteryOrder GetLotteryOrderInfo(DbExecutor db, int orderId, int userId)
        {
            if (userId == 0) return this.GetLotteryOrderInfo(orderId);
            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetLotteryOrderInfo",
                NewParam("@OrderID", orderId),
                NewParam("@UserID", userId));
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return null;
            return new LotteryOrder(ds.Tables[0].Rows[0]);
        }

        /// <summary>
        /// 获取历史订单的信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public LotteryOrderHistory GetLotteryOrderHisotryInfo(int orderId)
        {
            return BDC.LotteryOrderHistory.Where(t => t.SiteID == SiteInfo.ID && t.ID == orderId).FirstOrDefault();
        }

        /// <summary>
        /// 改单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="type"></param>
        /// <param name="playerId"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public bool UpdateOrderNumber(int orderId, int playerId, string number)
        {
            LotteryOrder order = this.GetLotteryOrderInfo(orderId);
            if (order == null || order.Status != LotteryOrder.OrderStatus.Normal)
            {
                base.Message("当前订单不能改单");
                return false;
            }

            LotteryType type;
            IPlayer player = PlayerFactory.GetPlayer(playerId, out type);
            if (player == null)
            {
                base.Message("玩法错误");
                return false;
            }

            if (player.Bet(number) != order.Bet)
            {
                base.Message("前后注数不一致");
                return false;
            }

            string log = string.Format("彩种:{0},玩法:{1},投注内容:{2}", order.Type.GetDescription(), order.PlayerID, order.Number);
            order.Type = type;
            order.PlayerID = playerId;
            order.Number = number;
            string update = string.Format("彩种:{0},玩法:{1},投注内容:{2}", order.Type.GetDescription(), order.PlayerID, order.Number);

            using (DbExecutor db = NewExecutor())
            {
                if (this.SaveOrderNumber(db, order))
                {
                    AdminInfo.Log(AdminLog.LogType.Lottery, "改单，订单号{0}。修改之前：{1}。修改之后：{2}", order.ID, log, update);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取一个彩种的限号设定值
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public List<LimitedSetting> GetLimitedSettingList(LotteryType game)
        {
            return BDC.LimitedSetting.Where(t => t.SiteID == SiteInfo.ID && t.Game == game).ToList();
        }

        /// <summary>
        /// 修改限号封锁值
        /// </summary>
        /// <param name="game"></param>
        /// <param name="type"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public bool UpdateLimitedSetting(LotteryType game, LimitedType type, decimal money)
        {
            if (!Enum.IsDefined(typeof(LotteryType), (byte)game))
            {
                base.Message("玩法错误");
                return false;
            }
            if (type == LimitedType.None)
            {
                base.Message("限号标识错误");
                return false;
            }
            if (money < decimal.Zero)
            {
                base.Message("金额输入错误");
                return false;
            }
            LimitedSetting setting = new LimitedSetting()
            {
                SiteID = SiteInfo.ID,
                Game = game,
                Type = type,
                Money = money
            };
            if (setting.Exists())
            {
                return setting.Update() > 0;
            }
            else
            {
                return setting.Add();
            }
        }

        /// <summary>
        /// 新建一个分类
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AddCategory(string name, string code)
        {
            if (string.IsNullOrEmpty(name))
            {
                base.Message("分类名称不能为空");
                return false;
            }
            return new LotteryCate()
            {
                SiteID = SiteInfo.ID,
                Name = name,
                Code = code
            }.Add();
        }

        /// <summary>
        /// 获取彩种分类
        /// </summary>
        /// <returns></returns>
        public List<LotteryCate> GetLotteryCateList()
        {
            return BDC.LotteryCate.Where(t => t.SiteID == SiteInfo.ID).OrderByDescending(t => t.Sort).ToList();
        }

        /// <summary>
        /// 更新分类信息
        /// </summary>
        /// <param name="cateId">主键ID</param>
        /// <param name="field">要更新的字段</param>
        /// <returns></returns>
        public bool UpdateCategory(int cateId, object value, Expression<Func<LotteryCate, object>> field)
        {
            LotteryCate cate = new LotteryCate() { ID = cateId }.Info();
            if (cate == null)
            {
                base.Message("编号错误");
                return false;
            }

            field.GetPropertyInfo().SetValue(cate, value);
            return cate.Update(null, field) == 1;
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="cateId"></param>
        /// <returns></returns>
        public bool DeleteCategory(int cateId)
        {
            LotteryCate cate = new LotteryCate() { ID = cateId }.Info();
            if (cate == null || cate.SiteID != SiteInfo.ID)
            {
                base.Message("编号错误");
                return false;
            }

            if (BDC.LotterySetting.Where(t => t.SiteID == SiteInfo.ID && t.CateID == cateId).Count() != 0)
            {
                base.Message("该分类下存在彩种，无法删除");
                return false;
            }

            return cate.Delete() == 1;
        }
    }
}
