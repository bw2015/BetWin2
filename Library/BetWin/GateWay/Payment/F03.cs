using Newtonsoft.Json.Linq;
using SP.Studio.Array;
using SP.Studio.Net;
using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Payment
{
    public class F03 : IPayment
    {
        [Description("软件账号")]
        public string account { get; set; }

        [Description("生成类型")]
        public string type { get; set; }

        [Description("sktype")]
        public string sktype { get; set; }

        [Description("内网穿透网址")]
        public string nat { get; set; }

        [Description("网关")]
        public string Gateway { get; set; } = "https://www.f03.top/index/api/aliGetCode";

        [Description("软件密码")]
        public string pwd { get; set; }

        public F03(string settingString) : base(settingString)
        {
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            //?money=金额&desc=备注标识& timestamp=时间戳&token=MD5(加密密匙＋金额＋备注标识＋时间戳)

            //money=1.00&desc=20190304135613286&timestamp=1551679010575&token=c57ef342e988d404c6e9a001f7fd4440&type=union&orderno=2019030413564945776325BFA4A9CABC81475235E6251AF85410E8D56FF35D-79809695864350-00250001000499929458230304135649_00 

            systemId = WebAgent.GetParam("orderno").Substring(0, 50);
            money = WebAgent.GetParam("money", decimal.Zero);
            return WebAgent.GetParam("desc");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"money",this.Money.ToString("0.00") },
                {"desc",this.OrderID },
                {"account",this.account },
                {"type",this.type },
                {"sktype",this.sktype },
                {"nat",this.nat }
            };

            //md5(md5(money=money&desc=desc&account=account&pwd=软件密码)生成类型)

            string signStr = $"money={dic["money"]}&desc={dic["desc"]}&account={dic["account"]}&pwd={this.pwd}";
            signStr = MD5.toMD5(signStr).ToLower() + dic["type"];
            dic.Add("token", MD5.toMD5(signStr).ToLower());
            string url = $"{this.Gateway}?{dic.ToQueryString()}";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            try
            {
                JObject info = JObject.Parse(result);
                if (info["status"].Value<int>() == 0)
                {
                    context.Response.Write(info["msg"].Value<string>());
                    return;
                }
                string code = info["code"].Value<string>();
                switch (this.type)
                {
                    case "wx":
                        this.CreateWXCode(code);
                        break;
                    case "alipay":
                        this.CreateAliCode(code);
                        break;
                    case "union":
                        this.CreateQRCode(code);
                        break;
                }
            }
            catch
            {
                context.Response.Write(result);
            }
        }

        public override string ShowCallback()
        {
            return "success";
        }

        public override bool Verify(VerifyCallBack callback)
        {
            //money=1.00&desc=20190304135613286&timestamp=1551679010575&token=c57ef342e988d404c6e9a001f7fd4440&type=union&orderno=2019030413564945776325BFA4A9CABC81475235E6251AF85410E8D56FF35D-79809695864350-00250001000499929458230304135649_00 

            string money = WebAgent.GetParam("money");
            string desc = WebAgent.GetParam("desc");
            string timespan = WebAgent.GetParam("timestamp");
            string token = WebAgent.GetParam("token");

            if (string.IsNullOrEmpty(token)) return false;
            callback.Invoke();
            return true;
        }
    }
}
