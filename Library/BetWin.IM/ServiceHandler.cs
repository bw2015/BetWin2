using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using System.Net.WebSockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

using BW.IM.Agent;
using BW.IM.Common;
using SP.Studio.Model;
using SP.Studio.Web;

namespace BW.IM
{
    public class ServiceHandler : IHttpHandler
    {
        /// <summary>
        /// 当前登录的用户
        /// </summary>
        private User UserInfo
        {
            get
            {
                if (HttpContext.Current == null) return null;
                return (User)HttpContext.Current.Items[Utils.USERINFO];
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(ProcessChat);
            }
            else
            {
                string url = context.Request.RawUrl;
                if (Regex.IsMatch(url, @"/service/\d{4}/(USER|ADMIN)/[0-9a-f]{32}$"))
                {
                    this.ProcessChat(context);
                    return;
                }
                Regex regex = new Regex(@"/service/(?<Type>\w+)/(?<Method>\w+)");
                if (!regex.IsMatch(url))
                {
                    context.Response.Write(false, "非WebSocket请求");
                }

                Type type = this.GetType().Assembly.GetType("BW.IM.Handler." + regex.Match(url).Groups["Type"].Value);
                if (type == null)
                {
                    Utils.showerror(context, "404 Type Not Found");
                }

                MethodInfo method = type.GetMethod(regex.Match(url).Groups["Method"].Value, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null)
                {
                    Utils.showerror(context, "404 Method Not Found");
                }

                method.Invoke(Activator.CreateInstance(type), new object[] { context });
            }
        }

        /// <summary>
        /// websocket模式
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ProcessChat(AspNetWebSocketContext context)
        {
            AspNetWebSocket socket = (AspNetWebSocket)context.WebSocket;

            if (UserInfo == null)
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "未登录", new CancellationToken());
                return;
            }

            string key = UserInfo.KEY;

            Utils.SOCKETLIST.AddOrUpdate(key, socket, (t, ws) => socket);
            lock (Utils.LOCK_USERLIST)
            {
                if (!Utils.USERLIST.Exists(t => t.KEY == key)) Utils.USERLIST.Add(UserInfo);
            }
            int index = 0;
            SiteAgent.Instance().AddSystemLog(8080, string.Format("socket.State:{0} SiteID:{1} UserID:{2}", socket.State, UserInfo.SiteID, UserInfo.ID));
            while (socket.State == WebSocketState.Open)
            {
                User user = (User)context.Items[Utils.USERINFO];
                if (user == null) break;
                index++;
                String userMessage = null;

                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4096]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                userMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                try
                {
                    Utils.GetMessage(user, userMessage);
                }
                catch (Exception ex)
                {
                    SiteAgent.Instance().AddErrorLog(8080, ex, "接受信息：" + userMessage);
                }
                finally
                {
                    UserAgent.Instance().SetOnlineStatus(user);
                }
            }
            Utils.Close(key);
        }

        /// <summary>
        /// POST模式
        /// </summary>
        /// <param name="context"></param>
        private void ProcessChat(HttpContext context)
        {
            User user = (User)context.Items[Utils.USERINFO];
            if (user == null)
            {
                Utils.showerror(context, "未登录");
            }
            string userMessage = new StreamReader(context.Request.InputStream).ReadToEnd();
            try
            {
                Utils.GetMessage(user, userMessage);
            }
            catch (Exception ex)
            {
                SiteAgent.Instance().AddErrorLog(user.SiteID, ex, "接受信息：" + userMessage);
            }
            finally
            {
                UserAgent.Instance().SetOnlineStatus(user);
            }
        }
    }
}
