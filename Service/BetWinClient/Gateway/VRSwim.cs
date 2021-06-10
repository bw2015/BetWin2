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
    public partial class VRSwim : IGateway
    {
        private Dictionary<string, string> getResultByVRByTop1()
        {
            return Utils.GetVRTop1("http://videoracing.com/open_9_1.aspx");
        }

        private Dictionary<string, string> getResultByVR()
        {
            return Utils.GetVRList("http://videoracing.com/open_9_2.aspx");
        }
    }
}
