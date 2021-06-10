using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SP.Studio.Json;

namespace SP.Studio.GateWay.WeChat
{
    /// <summary>
    /// 用户的token信息
    /// </summary>
    public struct UserToken
    {
        //{"access_token":"9_yP5X3VA-hHdazCRxkaDipC7exbESuoZVQANR2-OpR8KxsZyjale30HtnIC9BbBQ_QzWx8AGProl7EPXzFic2fg","expires_in":7200,"refresh_token":"9_5Hh6li7eGbJaItW4BdoSypXT_O7k_qSL-MT7lWimWiK3mAqjt1A2rGM_Z8ik3jgIotcEnpAri2ioP0X285Lx4w","openid":"o6zor1rJsJvhIzTGvzFrfNWiDAxI","scope":"snsapi_userinfo"}

        //{"errcode":40163,"errmsg":"code been used, hints: [ req_id: hGNMhA0048th20 ]"}
        public UserToken(string result)
        {
            this.access_token = JsonAgent.GetValue<string>(result, "access_token");
            this.expires = DateTime.Now.AddSeconds(JsonAgent.GetValue<int>(result, "expires_in"));
            this.openid = JsonAgent.GetValue<string>(result, "openid");
            this.errmsg = JsonAgent.GetValue<string>(result, "errmsg");
        }

        /// <summary>
        /// 从数据库内获取授权信息
        /// </summary>
        /// <param name="dr"></param>
        public UserToken(DataRow dr)
        {
            this.openid = (string)dr["OpenID"];
            this.expires = (DateTime)dr["TokenExpire"];
            this.access_token = (string)dr["Token"];
            this.errmsg = this.expires < DateTime.Now ? "授权已过期" : string.Empty;
        }

        public string access_token;

        public DateTime expires;

        public string openid;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errmsg;
    }
}
