using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SP.Studio.Net;
using SP.Studio.Text;
using System.ComponentModel;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// VR赛马
    /// </summary>
    public partial class VRRace : IGateway
    {


        [Description("竞速彩票官网")]
        /// <summary>
        /// 竞速官网获取最新一期
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getResultByVRByTop1()
        {
            return Utils.GetVRTop1("http://videoracing.com/open_8_1.aspx");
        }

        private Dictionary<string, string> getResultByVR()
        {
            return Utils.GetVRList("http://videoracing.com/open_8_2.aspx");
        }
    }
}
