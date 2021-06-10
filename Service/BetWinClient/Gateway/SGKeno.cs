//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.ComponentModel;
//using System.Xml.Linq;

//using SP.Studio.Xml;
//using SP.Studio.Net;

//namespace BetWinClient.Gateway
//{
//    /// <summary>
//    /// 新加坡快乐彩
//    /// </summary>
//    public partial class SGKeno : IGateway
//    {
//        [Description("开彩网API")]
//        private Dictionary<string, string> getResultByAPIUS()
//        {
//            Dictionary<string, string> list = Utils.GetAPI(API.opencai, this.GetType());
//            return list.ToDictionary(t => t.Key, t => Utils.GetOpenCode(t.Value));
//        }
//    }
//}
