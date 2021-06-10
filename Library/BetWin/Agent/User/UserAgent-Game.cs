using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Data;
using System.Xml.Linq;

using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Xml;

using BW.Common.Games;
using BW.Common.Users;

namespace BW.Agent
{
    /// <summary>
    /// 第三方游戏的用户名
    /// </summary>
    partial class UserAgent
    {
        /// <summary>
        /// 第三方游戏用户名的缓存
        /// </summary>
        private Dictionary<string, string> _gamePlayerName = new Dictionary<string, string>();

        /// <summary>
        /// 获取第三方游戏接口中的用户名，返回null表示未开户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetPlayerName(int userId, GameType type)
        {
            string key = string.Concat(type, userId);
            lock (_gamePlayerName)
            {
                if (_gamePlayerName.ContainsKey(key)) return _gamePlayerName[key];
                int siteId = this.GetSiteID(userId);
                string playerName = BDC.UserGame.Where(t => t.SiteID == siteId && t.UserID == userId && t.Type == type).Select(t => t.PlayerName).FirstOrDefault();
                if (string.IsNullOrEmpty(playerName)) return null;
                _gamePlayerName.Add(key, playerName);
                return playerName;
            }
        }

        /// <summary>
        /// 获取用户在第三方游戏接口的开户列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<GameAccount> GetGameAccount(int userId)
        {
            return BDC.UserGame.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId)
                .Join(BDC.GameSetting.Where(p => p.SiteID == SiteInfo.ID && p.IsOpen), t => t.Type, t => t.Type, (account, setting) => new
                {
                    account,
                    setting
                }).OrderByDescending(t => t.setting.Sort).Select(t => t.account).ToList();
            //.Select(p => p.Type).Contains(t.Type)).ToList();
        }

        /// <summary>
        /// 第三方游戏账户对应的用户ID
        /// </summary>
        private Dictionary<string, int> _gameUserID = new Dictionary<string, int>();

        /// <summary>
        /// 从第三方游戏账户获取用户ID（适用于非web程序）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="playerName"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetUserID(GameType type, string playerName, int siteId)
        {
            if (siteId == 0) siteId = SiteInfo.ID;
            string key = string.Concat(siteId, type, playerName);
            lock (_gameUserID)
            {
                if (_gameUserID.ContainsKey(key)) return _gameUserID[key];
                int? userId = BDC.UserGame.Where(t => t.SiteID == siteId && t.Type == type && t.PlayerName == playerName).Select(t => (int?)t.UserID).FirstOrDefault();
                if (userId == null) return 0;
                _gameUserID.Add(key, userId.Value);
                return userId.Value;
            }
        }

        /// <summary>
        /// 从第三方游戏账户获取用户ID（不需要带站点ID）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public int GetUserID(GameType type, string playerName)
        {
            string key = string.Concat(0, type, playerName);
            lock (_gameUserID)
            {
                if (_gameUserID.ContainsKey(key)) return _gameUserID[key];
                int? userId = BDC.UserGame.Where(t => t.Type == type && t.PlayerName == playerName).Select(t => (int?)t.UserID).FirstOrDefault();
                if (userId == null) return 0;
                _gameUserID.Add(key, userId.Value);
                return userId.Value;
            }
        }

        /// <summary>
        /// 新建一个第三方游戏账户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="playerName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool AddGameAccount(int userId, GameType type, string playerName, string password)
        {
            if (!string.IsNullOrEmpty(this.GetPlayerName(userId, type)))
            {
                base.Message("该用户已经创建账户");
                return false;
            }

            return new GameAccount()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                Type = type,
                PlayerName = playerName,
                Password = password,
                UpdateAt = DateTime.Now
            }.Add();
        }

        /// <summary>
        /// 获取第三方游戏账户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameAccount GetGameAccountInfo(int userId, GameType type)
        {
            int siteId = this.GetSiteID(userId);
            return this.GetGameAccountInfo(userId, type, siteId);
        }

        /// <summary>
        /// 获取第三方游戏账户信息（在日志服务中调用）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public GameAccount GetGameAccountInfo(int userId, GameType type, int siteId)
        {
            return BDC.UserGame.Where(t => t.SiteID == siteId && t.UserID == userId && t.Type == type).FirstOrDefault();
        }


        /// <summary>
        /// 更新用户余额（在日志服务中调用）
        /// </summary>
        /// <param name="siteId">当前站点(为0的话自动从UserID中读取）</param>
        /// <param name="userId">用户ID</param>
        /// <param name="type">游戏类型</param>
        /// <param name="balance">余额</param>
        /// <param name="updateAt">本次更新时间</param>
        /// <returns></returns>
        public bool UpdateGameAccountMoney(int siteId, int userId, GameType type, decimal balance, DateTime updateAt)
        {
            if (balance < decimal.Zero) return false;
            using (DbExecutor db = NewExecutor())
            {
                if (siteId == 0) siteId = this.GetSiteID(userId, db);
                return db.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE {0} SET Money = @Money, Withdraw = dbo.min_Money(@Money,Withdraw), UpdateAt = @UpdateAt WHERE SiteID = @SiteID AND UserID = @UserID AND Type = @Type AND UpdateAt < @UpdateAt", typeof(GameAccount).GetTableName()),
                    NewParam("@Money", balance),
                    NewParam("@UpdateAt", updateAt),
                    NewParam("@SiteID", siteId),
                    NewParam("@UserID", userId),
                    NewParam("@Type", type)) > 0;
            }
        }

        /// <summary>
        /// 从余额接口获取账户余额
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool UpdateGameAccountMoney(int userId, GameType type)
        {
            int siteId = this.GetSiteID(userId, null);
            GameAccount account = this.GetGameAccountInfo(userId, type, siteId);
            if (account == null)
            {
                base.Message("您暂未开通{0}账户", type.GetDescription());
                return false;
            }

            GameSetting setting = GameAgent.Instance().GetGameSettingInfo(type, siteId);
            decimal money = setting.Setting.GetBalance(userId);
            if (money < decimal.Zero)
            {
                base.Message("获取余额失败");
                return false;
            }

            return this.UpdateGameAccountMoney(0, userId, type, money, DateTime.Now);
        }

    }
}
