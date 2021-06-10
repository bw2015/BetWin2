using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Common.Games;
using BW.GateWay.Games;

using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    /// 公共的游戏接口设置
    /// </summary>
    [Table(Name = "sys_GameInterface")]
    public class GameInterface
    {

        /// <summary>
        /// 接口类型
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public GameType Type { get; set; }

        /// <summary>
        /// 是否开放
        /// </summary>
        [Column(Name = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// 参数设定字符串
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

        private IGame _setting;
        /// <summary>
        /// 游戏参数设定
        /// </summary>
        public IGame Setting
        {
            get
            {
                if (_setting == null) _setting = GameFactory.CreateGame(this.Type, this.SettingString);
                return _setting;
            }
            set
            {
                this.SettingString = _setting = value;
            }
        }
    }
}
