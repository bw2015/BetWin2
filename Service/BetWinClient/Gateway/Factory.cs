using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Configuration;

using SP.Studio.Core;


namespace BetWinClient.Gateway
{
    /// <summary>
    /// 采集工厂
    /// </summary>
    public partial class LotteryFactory
    {
        private static string[] _game;
        /// <summary>
        /// 需要开奖的彩种
        /// </summary>
        public static string[] Games
        {
            get
            {
                if (_game == null)
                {
                    string game = ConfigurationManager.AppSettings["game"];
                    if (string.IsNullOrEmpty(game))
                    {
                        _game = new string[] { };
                    }
                    else
                    {
                        _game = game.Split(',');
                    }
                }
                return _game;
            }
        }

        private static Type[] type;

        public static void Run(string typeName = null)
        {
            if (type == null)
            {
                Assembly ass = typeof(LotteryFactory).Assembly;
                type = ass.GetTypes().Where(t => t.IsBaseType(typeof(IGateway)) && !t.IsAbstract).ToArray();
                if (!string.IsNullOrEmpty(typeName)) type = type.Where(t => t.Name == typeName).ToArray();
                if (Games.Length != 0) type = type.Where(t => Games.Contains(t.Name)).ToArray();
                Utils.WriteLine(string.Join(",", type.Select(t => t.FullName)));
            }
            System.Threading.Tasks.Parallel.ForEach(type, t =>
            {
                Utils.WriteLine(t.FullName);
                if (t.GetType().IsAbstract) return;
                Activator.CreateInstance(t);
            });
        }
    }
}
