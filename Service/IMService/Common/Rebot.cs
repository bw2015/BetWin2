using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SP.Studio.Core;
using IMService.Framework;

namespace IMService.Common
{
    /// <summary>
    /// 机器人设置
    /// </summary>
    public class Rebot : SettingBase
    {
        public Rebot(string setting) : base(setting)
        {

        }

        /// <summary>
        /// 是否开启机器人
        /// </summary>
        public bool IsOpen { get; set; }

        private string _name = "机器人";
        /// <summary>
        /// 机器人的名字
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _face = "/images/rebot.jpg";
        /// <summary>
        /// 存储的头像
        /// </summary>
        public string Face
        {
            get
            {
                return _face;
            }
            set
            {
                _face = value;
            }
        }

        /// <summary>
        /// 对外显示的头像
        /// </summary>
        public string FaceShow
        {
            get
            {
                return Utils.GetImage(this.Face);
            }
        }
    }
}
