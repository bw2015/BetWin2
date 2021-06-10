using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Json;

namespace SP.Studio.GateWay.WeChat
{
    /// <summary>
    /// 用户的信息
    /// </summary>
    public struct SNSInfo
    {
        public SNSInfo(string result)
        {
            openid = JsonAgent.GetValue<string>(result, "openid");
            nickname = JsonAgent.GetValue<string>(result, "nickname");
            sex = JsonAgent.GetValue<string>(result, "sex");
            city = JsonAgent.GetValue<string>(result, "city");
            province = JsonAgent.GetValue<string>(result, "province");
            country = JsonAgent.GetValue<string>(result, "country");
            headimgurl = JsonAgent.GetValue<string>(result, "headimgurl");
        }

        //{"openid":"o6zor1rJsJvhIzTGvzFrfNWiDAxI","nickname":"S鉁≒","sex":1,"language":"en","city":"Guangzhou","province":"Guangdong","country":"CN","headimgurl":"http:\/\/thirdwx.qlogo.cn\/mmopen\/vi_32\/PiajxSqBRaEKpdicm0Eia8Nvic1ypX4IRyRe1D56DEKTvvQ67Miambep5rEUynbvrWMS2sCuUVznOuKS9NiaAyOiaYBgw\/132","privilege":[]}

        public string openid;

        public string nickname;

        public string sex;

        public string city;

        public string province;

        public string country;

        public string headimgurl;

        public override string ToString()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("openid", this.openid);
            dic.Add("nickname", this.nickname);
            dic.Add("sex", this.sex);
            dic.Add("city", this.city);
            dic.Add("province", this.province);
            dic.Add("country", this.country);
            dic.Add("headimgurl", this.headimgurl);

            return dic.ToJson();
        }
    }
}
