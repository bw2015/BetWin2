using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.WebSockets;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using BW.Agent;
using BW.GateWay.IM;
using SP.Studio.Core;
using SP.Studio.Web;
using SP.Studio.Model;



namespace BW.Handler
{
    public class ServiceHandler : IHandler, IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
               // context.AcceptWebSocketRequest(ProcessChat);
            }
            else
            {
                switch (WebAgent.QS("ac"))
                {
                    case "socket":
                        this.socketlist(context);
                        break;
                    default:
                        context.Response.Write(false, "非WebSocket访问");
                        break;
                }
            }
        }

        /// <summary>
        /// socket链接状态
        /// </summary>
        /// <param name="context"></param>
        private void socketlist(HttpContext context)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"TimeCount\":{0},", Config.TimeCount)
                .AppendFormat("\"Service\":{0} ,", Config.serviceList.ToDictionary(t => t.Key, t => new JsonString("[" + string.Join(",", t.Value) + "]")).ToJson())
                .Append("\"Online\":{")
                .Append(string.Join(",", Config.OnlineList.Select(t => string.Format("\"{0}\":{1}", t.Key, t.Value.ToJson()))))
                .Append("} ,")
                .Append("\"SiteUser\":{")
                .Append(string.Join(",", Config.SiteUser.Select(t => string.Format("\"{0}\":[{1}]", t.Key, string.Join(",", t.Value.Select(p => "\"" + p + "\""))))))
                .Append("},")
                .Append("\"WechatUser\":{")
                .Append(string.Join(",", Config.WechatUser.Select(t => string.Format("\"{0}\":[{1}]", t.Key, string.Join(",", t.Value.Select(p => "\"" + p + "\""))))))
                .Append("}")
                .Append("}");
            context.Response.Write(true, this.StopwatchMessage(context), sb.ToString());
        }


        /// <summary>
        /// 只有发信息过来的时候才会执行
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ProcessChat(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;

            string key = null;

            if (AdminInfo != null)
            {
                key = string.Concat(UserAgent.IM_ADMIN, "-", AdminInfo.ID);
            }
            else if (UserInfo != null)
            {
                key = string.Concat(UserAgent.IM_USER, "-", UserInfo.ID);
            }

            if (key == null) return;

            bool isRun = true;
            while (isRun)
            {
                switch (socket.State)
                {
                    case WebSocketState.Open:
                        try
                        {
                            if (!Config.SiteUser.ContainsKey(SiteInfo.ID)) Config.SiteUser.Add(SiteInfo.ID, new List<string>());
                            if (!Config.SiteUser[SiteInfo.ID].Contains(key)) Config.SiteUser[SiteInfo.ID].Add(key);

                            if (!Config.OnlineList.ContainsKey(key))
                            {
                                Config.OnlineList.Add(key, socket);
                            }
                            else
                            {
                                Config.OnlineList[key] = socket;
                            }
                            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
                            WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                            string userMsg = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                            Config.GetMessage(key, userMsg);
                        }
                        catch (Exception ex)
                        {
                            SystemAgent.Instance().AddErrorLog(SiteInfo.ID, ex, ex.Message);
                            throw ex;
                        }
                        break;
                    default:
                        if (socket.State == WebSocketState.CloseReceived)
                        {
                            Config.GetMessage(key, string.Concat("{\"Action\":\"Offline\",\"ID\":\"", key, "\"}"));
                        }
                        isRun = false;
                        break;
                }
            }

            await socket.CloseAsync(WebSocketCloseStatus.Empty, "断开连接", new CancellationToken());
        }
    }
}
