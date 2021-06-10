using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BW.Framework;
using SP.Studio.Xml;
using SP.Studio.Core;
using SP.Studio.PageBase;
namespace BW.Common.Games
{
    /// <summary>
    /// 老虎机游戏
    /// </summary>
    public struct SlotGame
    {
        public SlotGame(GameType type, XElement item)
        {
            this.Type = type;
            this.Name = item.GetAttributeValue("value");
            this.ID = item.GetAttributeValue("name");
            this.Category = item.GetAttributeValue("category");
            string[] platform = item.GetAttributeValue("platform").Split(',');
            this.IsPC = platform.Contains("PC");
            this.IsMobile = platform.Contains("Mobile");
        }

        /// <summary>
        /// 接口类型
        /// </summary>
        public GameType Type;

        /// <summary>
        /// 游戏名字
        /// </summary>
        public string Name;

        /// <summary>
        /// 游戏标识类型
        /// </summary>
        public string ID;

        /// <summary>
        /// 所属分类（中文）
        /// </summary>
        public string Category;

        /// <summary>
        /// 是否支持PC平台
        /// </summary>
        public bool IsPC;

        /// <summary>
        /// 是否支持移动平台
        /// </summary>
        public bool IsMobile;

        public string Cover
        {
            get
            {
                return string.Format("{0}/images/{1}/{2}.jpg", SysSetting.GetSetting().imgServer, this.Type.ToString().ToLower(), this.ID);
            }
        }

        public override string ToString()
        {
            return string.Concat("{",
                "\"Type\":\"", this.Type, "\",",
                "\"Name\":\"", this.Name, "\",",
                "\"ID\":\"", this.ID, "\",",
                "\"Category\":\"", this.Category, "\",",
                "\"Cover\":\"", this.Cover, "\"}");
        }
    }
}
