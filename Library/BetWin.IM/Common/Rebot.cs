using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Specialized;
using System.Web;

using SP.Studio.Array;

using BW.IM.Agent;

namespace BW.IM.Common
{
    /// <summary>
    /// 机器人
    /// </summary>
    public class Rebot
    {
        private Regex timeRegex = new Regex(@"(?<Hour>\d{1,2}):(?<Minute>\d{1,2})$");

        public Rebot(DataRow dr)
        {
            this.ID = (int)dr["RebotID"];
            this.SiteID = (int)dr["SiteID"];
            this.Type = (GroupType)dr["Type"];
            this.UserID = (int)dr["UserID"];
            this.Name = string.IsNullOrEmpty((string)dr["NickName"]) ? (string)dr["UserName"] : (string)dr["NickName"];
            this.Face = Utils.GetFace((string)dr["Face"]);
            this.IMKey = string.Format("{0}-{1}", UserType.USER, this.UserID);

            string setting = (string)dr["Setting"];
            NameValueCollection request = HttpUtility.ParseQueryString(setting);
            this.Command = HttpUtility.UrlDecode(request["Command"]).Split(' ', '|').Where(t => !string.IsNullOrEmpty(t)).ToArray();

            string time1 = HttpUtility.UrlDecode(request["Time1"]);
            if (timeRegex.IsMatch(time1))
            {
                this.Time1 = int.Parse(timeRegex.Match(time1).Groups["Hour"].Value) * 60 + int.Parse(timeRegex.Match(time1).Groups["Minute"].Value);
            }

            string time2 = HttpUtility.UrlDecode(request["Time2"]);
            if (timeRegex.IsMatch(time2))
            {
                this.Time2 = int.Parse(timeRegex.Match(time2).Groups["Hour"].Value) * 60 + int.Parse(timeRegex.Match(time2).Groups["Minute"].Value);
            }

            int probability;
            if (int.TryParse(request["Probability"], out probability))
            {
                this.Probability = probability;
            }

        }

        /// <summary>
        /// 机器人编号
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        public int SiteID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        public GroupType Type { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 指令列表
        /// </summary>
        public string[] Command { get; set; }


        /// <summary>
        /// 有效时间1
        /// </summary>
        public int Time1 { get; set; }

        /// <summary>
        /// 有效时间2
        /// </summary>
        public int Time2 { get; set; }

        /// <summary>
        /// 是否在当前时间内
        /// </summary>
        /// <returns></returns>
        public bool IsTime()
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < this.Time1) return false;
            if (this.Time2 != 0 && now > this.Time2) return false;
            return true;
        }

        /// <summary>
        /// 随机获取一个指令
        /// </summary>
        /// <returns></returns>
        public string GetCommand()
        {
            return this.Command.GetRandom();
        }

        /// <summary>
        /// 投注概率
        /// </summary>
        public int Probability { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Face { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IMKey { get; private set; }
    }
}
