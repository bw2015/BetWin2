//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.ComponentModel;
//using System.Text.RegularExpressions;

//namespace BetWinClient.Gateway
//{
//    /// <summary>
//    /// 美国强力球1.5分
//    /// </summary>
//    public partial class USBall15 : IGateway
//    {

//        [Description("美国强力球45秒")]
//        private Dictionary<string, string> getResultByAPIPlus()
//        {
//            Dictionary<string, string> dic = Utils.GetAPI(API.opencai, this.GetType());

//            return dic.ToDictionary(t => Regex.Replace(t.Key, @"(?<Date>\d{8})(?<Index>\d{3})", "${Date}-${Index}"),
//                t => Utils.GetOpenCode(t.Value));
//        }
//    }
//}
