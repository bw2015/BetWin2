using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Windows.Forms;

using SP.Studio.Net;
using SP.Studio.Xml;

namespace BetWinClient
{
    /// <summary>
    /// 配置文件
    /// </summary>
    public class Config
    {
        public Config()
        {
            string file = Application.StartupPath + @"\BetWinClient.xml";
            if (!File.Exists(file))
            {
                Console.WriteLine("配置文件{0}不存在", file);
                return;
            }
            XElement root = XElement.Parse(File.ReadAllText(file, Encoding.UTF8));
            this.Gateway = root.GetAttributeValue("gateway");
            string game = root.GetAttributeValue("game");
            if (!string.IsNullOrEmpty(game)) this.Game = game.Split(',');
            foreach (XElement item in root.Elements())
            {
                string key = item.Attribute("name").Value;
                string value = item.Value;
                if (!this.API.ContainsKey(key)) this.API.Add(key, value);
                if (!this._lastTime.ContainsKey(key)) this._lastTime.Add(key, DateTime.MinValue);
            }
        }

        /// <summary>
        /// 网关地址
        /// </summary>
        public string Gateway { get; set; }

        /// <summary>
        /// 要采集的彩种
        /// </summary>
        public string[] Game { get; set; }

        /// <summary>
        /// 付费接口的API
        /// </summary>
        public readonly Dictionary<string, string> API = new Dictionary<string, string>();

        internal bool IsGame(string game)
        {
            if (this.Game == null || this.Game.Length == 0) return true;
            return this.Game.Contains(game);
        }


        /// <summary>
        /// 上次采集的时间（3秒钟不重复采集）
        /// </summary>
        private readonly Dictionary<string, DateTime> _lastTime = new Dictionary<string, DateTime>();

        /// <summary>
        /// 从付费接口中获取
        /// </summary>
        /// <param name="api"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetContent(API api, string game)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            string key = string.Format("{0}.{1}", api, game);
            if (!this.API.ContainsKey(key)) return dic;
            if (((TimeSpan)(DateTime.Now - _lastTime[key])).TotalSeconds < 3) return dic;

            string url = this.API[key];
            string result = string.Empty;
            XElement root;
            try
            {
                result = NetAgent.DownloadData(url, Encoding.UTF8);
                root = XElement.Parse(result);
            }
            catch (Exception ex)
            {
                Utils.SaveErrorLog(url + "\n\r" + ex.Message + "\n\r" + result + "\n\r==================================\n\r");
                return dic;
            }
            finally
            {
                _lastTime[key] = DateTime.Now;
            }

            switch (api)
            {
                case BetWinClient.API.cpk:
                    dic = root.Elements().ToDictionary(t => t.GetAttributeValue("id"), t => t.Value);
                    break;
                case BetWinClient.API.mcai:
                    dic = root.Elements().ToDictionary(t => t.Element("issue").Value, t => t.Element("code").Value);
                    break;
                case BetWinClient.API.opencai:
                    dic = root.Elements().ToDictionary(t => t.GetAttributeValue("expect"), t => t.GetAttributeValue("opencode"));
                    break;
            }
            return dic;
        }
    }
}
