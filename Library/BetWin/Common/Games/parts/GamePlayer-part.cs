using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.Common.Games
{
    partial class GamePlayer
    {
        public GamePlayer() { }

        public GamePlayer(SlotGame game, int playerId = 0)
        {
            this.ID = playerId;
            this.IsOpen = true;
            this.IsTop = false;
            this.Type = game.Type;
            this.Code = game.ID;
            this.GameInfo = game;
        }

        /// <summary>
        /// 系统的游戏配置信息
        /// </summary>
        public SlotGame GameInfo { get; set; }

        public string Name
        {
            get
            {
                return this.GameInfo.Name;
            }
        }

        public string Category
        {
            get
            {
                return this.GameInfo.Category;
            }
        }

        public string Cover
        {
            get
            {
                return this.GameInfo.Cover;
            }
        }

        public bool IsMobile
        {
            get
            {
                return this.GameInfo.IsMobile;
            }
        }

        public bool IsPC
        {
            get
            {
                return this.GameInfo.IsPC;
            }
        }

        public string Platform
        {
            get
            {
                List<string> platform = new List<string>();
                if (this.IsPC) platform.Add("PC端");
                if (this.IsMobile) platform.Add("移动端");
                return string.Join(",", platform);
            }
        }
    }
}
