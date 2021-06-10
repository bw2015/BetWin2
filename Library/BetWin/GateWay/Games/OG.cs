using BW.Agent;
using BW.Common.Games;
using BW.Common.Users;
using Newtonsoft.Json.Linq;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Games
{
    public class OG : IGame
    {
        public OG() : base() { }

        public OG(string setting) : base(setting) { }

        /// <summary>
        /// 经营方代码
        /// </summary>
        public string XOperator { get; set; } = "mog109boqu";

        /// <summary>
        /// 经营方API金钥
        /// </summary>
        public string XKey { get; set; } = "14dNNaE9YRn6NdFQ";

        /// <summary>
        /// 游戏供应商id : 1
        /// </summary>
        public int providerId { get; set; } = 1;

        /// <summary>
        /// 游戏代号 : oglive
        /// </summary>
        public string GameCode { get; set; } = "oglive";

        /// <summary>
        /// 网关地址
        /// http://sample.domain.com/
        /// </summary>
        public string Gateway { get; set; } = "http://api01.oriental-game.com:8085/";

        public string APIGateway { get; set; } = "http://mucho.oriental-game.com:8057/";

        public override TransferStatus CheckTransfer(int userId, string id)
        {
            TransferLog order = GameAgent.Instance().GetTransferInfo(int.Parse(id));
            if (order == null) return TransferStatus.Faild;

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"Operator",this.XOperator },
                {"Key",this.XKey },
                {"SDate",order.CreateAt.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss") },
                {"EDate",order.CreateAt.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss") },
                {"TransferCode",id }
            };
            string postData = string.Join("&", data.Select(t => $"{t.Key}={t.Value}"));
            string url = this.APIGateway + "Transfer";
            string result = NetAgent.UploadData(url, postData);

            try
            {
                JArray list = JArray.Parse(result);
                return list.Count == 1 ? TransferStatus.Success : TransferStatus.Faild;
            }
            catch (Exception ex)
            {
                this.Message(ex.Message + "\n" + result);
                return TransferStatus.None;
            }

        }

        public override bool CreateUser(int userId, params object[] args)
        {
            string username = GameAgent.Instance().CreatePlayerName(userId);
            string password = Guid.NewGuid().ToString("N").Substring(0, 8);

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "username",username },
                {"country","China" },
                {"fullname",UserAgent.Instance().GetUserName(userId) },
                {"email",userId + "@gmail.com" },
                {"language","cn" },
                {"birthdate","1988-08-08" }
            };

            JObject info;
            if (!this.POST("register", data, out info)) return false;

            return UserAgent.Instance().AddGameAccount(userId, this.Type, username, password);
        }

        public override bool Deposit(int userId, decimal money, string id, out decimal amount)
        {
            return this.transfer(userId, money, "IN", id, out amount);
        }

        public override void FastLogin(int userId, string key)
        {
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null) return;

            JObject info;
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"username",user.PlayerName }
            };
            if (!this.GET("game-providers/" + this.providerId + "/games/" + this.GameCode + "/key", data, out info))
            {
                context.Response.Write(this.Message());
                return;
            }

            string gameToken = info["key"].Value<string>();
            if (!this.GET("game-providers/" + this.providerId + "/play", new Dictionary<string, string>()
            {
                {"key",gameToken },
                {"type",WebAgent.IsMobile() ? "mobile":"desktop" }
            }, out info))
            {
                context.Response.Write(this.Message());
                return;
            }

            WebAgent.SuccAndGo(info["url"].Value<string>());
        }

        public override decimal GetBalance(int userId)
        {
            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null) return decimal.Zero;

            JObject info;
            string userName = user.PlayerName;
            if (!this.GET("game-providers/" + this.providerId + "/balance", new Dictionary<string, string>() {
                { "username",userName }
            }, out info)) return decimal.MinusOne;

            return info["balance"].Value<decimal>();


        }

        public override bool Withdraw(int userId, decimal money, string orderId, out decimal amount)
        {
            return this.transfer(userId, money, "OUT", orderId, out amount);
        }

        /// <summary>
        /// 获取游戏日志(时间采用北京时间）
        /// </summary>
        /// <param name="startAt">开始时间（北京时间）</param>
        /// <param name="endAt">结束时间（北京时间）</param>
        /// <returns></returns>
        public string GetLog(DateTime startAt, DateTime endAt)
        {
            string url = this.APIGateway + "Transaction";
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"Operator",this.XOperator },
                {"Key",this.XKey },
                {"SDate",startAt.AddHours(-8).ToString("yyyy-MM-dd HH:mm:ss") },
                {"EDate",endAt.AddHours(-8).ToString("yyyy-MM-dd HH:mm:ss") }
            };
            string postData = string.Join("&", data.Select(t => $"{t.Key}={t.Value}"));
            return NetAgent.UploadData(url, postData);

        }

        #region ========= method ==============

        /// <summary>
        /// 转账
        /// </summary>
        /// <returns></returns>
        private bool transfer(int userId, decimal money, string action, string orderId, out decimal amount)
        {
            amount = decimal.Zero;
            if (money < decimal.Zero) return false;

            GameAccount user = UserAgent.Instance().GetGameAccountInfo(userId, this.Type);
            if (user == null)
            {
                this.Message("用户未注册");
                return false;
            }

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"username",user.PlayerName },
                {"balance",Math.Abs(money).ToString("0.00") },
                {"action",action },
                {"transferId",orderId }
            };

            JObject info;
            if (this.POST("game-providers/" + this.providerId + "/balance", data, out info))
            {
                amount = info["balance"].Value<decimal>();
                return true;
            }
            return false;
        }

        private static DateTime lastToken = DateTime.MinValue;

        private static string _token = null;

        /// <summary>
        /// 取得进入存取凭证    每30分钟会失效
        /// </summary>
        /// <returns></returns>
        private string GetToken()
        {
            if (string.IsNullOrEmpty(_token) || lastToken < DateTime.Now.AddMinutes(-15))
            {
                string url = $"{this.Gateway}token";
                string token = null;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("X-Operator", this.XOperator);
                    wc.Headers.Add("X-key", this.XKey);
                    string result = wc.DownloadString(url);

                    JObject obj = JObject.Parse(result);
                    if (obj.Value<string>("status") != "success") return token;

                    token = obj["data"]["token"].Value<string>();
                }
                if (token != null)
                {
                    _token = token;
                    lastToken = DateTime.Now;
                }
                return token;
            }
            return _token;
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool POST(string action, Dictionary<string, string> data, out JObject info)
        {
            string url = this.Gateway + action;
            string token = this.GetToken();
            Dictionary<string, string> header = new Dictionary<string, string>()
            {
                { "X-Token",token }
            };
            string result = string.Empty;
            try
            {
                result = NetAgent.UploadData(url, Encoding.UTF8, header, Encoding.UTF8.GetBytes(data.ToJson()));
                JObject obj = JObject.Parse(result);
                info = (JObject)obj["data"];
                return obj["status"].Value<string>() == "success";
            }
            catch (Exception ex)
            {
                info = null;
                this.Message(ex.Message);
                return false;
            }
            finally
            {
                this.SaveLog(0, result, "Url", url, "Token", token);
            }
        }

        /// <summary>
        /// 通过GET获取数据
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool GET(string action, Dictionary<string, string> data, out JObject info)
        {
            string url = this.Gateway + action + "?" + string.Join("&", data.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string token = this.GetToken();
            string result = string.Empty;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers.Add("X-Token", token);
                    result = wc.DownloadString(url);
                    JObject obj = JObject.Parse(result);
                    info = (JObject)obj["data"];
                    if (obj["status"].Value<string>() != "success")
                    {
                        this.Message(result);
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    info = null;
                    this.Message(ex.Message);
                    return false;
                }
                finally
                {

                    this.SaveLog(0, result, "Url", url, "Token", token);
                }
            }
        }

        #endregion
    }
}
