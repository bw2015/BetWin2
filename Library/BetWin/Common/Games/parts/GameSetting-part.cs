using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.GateWay.Games;
using BW.Common.Systems;
using BW.Agent;

namespace BW.Common.Games
{
    partial class GameSetting
    {
        /// <summary>
        /// 转化成为json字符串（只能在管理员后台调用）
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Type\":\"{0}\",", this.Type)
                .AppendFormat("\"Rate\":\"{0}\",", 0)
                .AppendFormat("\"IsRegister\":{0},", this.IsRegister ? 1 : 0)
                .AppendFormat("\"IsOpen\":{0},", this.IsOpen ? 1 : 0)
                .AppendFormat("\"Turnover\":\"{0}\",", this.Turnover)
                .AppendFormat("\"SettingString\":\"{0}\"", HttpUtility.JavaScriptStringEncode(this.SettingString))
                .Append("}");
            return sb.ToString();
        }

        private GameInterface _game;
        private GameInterface Game
        {
            get
            {
                if (_game == null)
                {
                    _game = SystemAgent.Instance().GetGameInterfaceInfo(this.Type, this.SettingString);
                }
                return _game;
            }
        }

        /// <summary>
        /// 系统是否开放当前接口
        /// </summary>
        public bool IsSystemOpen
        {
            get
            {
                return this.Game.IsOpen;
            }
        }

        public IGame Setting
        {
            get
            {
                return this.Game.Setting;
            }
        }
    }
}
