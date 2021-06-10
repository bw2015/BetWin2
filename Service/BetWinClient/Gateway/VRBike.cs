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
    public partial class VRBike : IGateway
    {
        private Dictionary<string, string> getResultByVRByTop1()
        {
            return Utils.GetVRTop1("http://videoracing.com/open_10_1.aspx", 5);
        }

        private Dictionary<string, string> getResultByVR()
        {
            return Utils.GetVRList("https://numbers.videoracing.com/open_10_2.aspx", 5);
        }
    }
}
