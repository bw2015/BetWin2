using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Timers;

using Fleck;
using BW.Agent;
using SP.Studio.Json;
using SP.Studio.Web;
using SP.Studio.Model;
using BW.Common.Sites;

using IMService.Agent;
using IMService.Common;
using IMService.Common.Send;
using IMService.Framework;

namespace IMService
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                string fileName = Assembly.GetExecutingAssembly().Location;
                Site[] siteList = SystemAgent.Instance().GetSiteList().Where(t => t.Status == Site.SiteStatus.Normal).ToArray();
                if (siteList.Length == 0) return;
                int timerIndex = 0;
                Timer timer = new Timer(1000);
                timer.Elapsed += (object sender, ElapsedEventArgs e) =>
                {
                    Site site = siteList[timerIndex % siteList.Length];
                    Console.Title = string.Format("{0}({1})", site.Name, site.ID);
                    timerIndex++;
                };
                timer.Start();
                System.Threading.Tasks.Parallel.ForEach(siteList, site =>
                {
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo(fileName, site.ID.ToString())
                    {
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                    process.Start();
                    process.WaitForExit();
                });
                return;
            }

            Socket(int.Parse(args[0]));
            Command();
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        static void Socket(int siteId)
        {
            SysSetting.GetSetting().SiteID = siteId;

            WebSocketServer server = new WebSocketServer(string.Format("ws://0.0.0.0:{0}", SysSetting.GetSetting().SiteID));

            // 每个socket链接之后产生一个新的进程
            server.Start(socket =>
            {
                // 新链接
                socket.OnOpen = () =>
                {
                    SysSetting.GetSetting().Online.Add(socket, new OnlineStatus());
                    Console.WriteLine("收到上线信息：{0}", socket.ConnectionInfo.Id);
                };

                // 链接关闭
                socket.OnClose = () =>
                {
                    Console.WriteLine("收到关闭信息：{0}", socket.ConnectionInfo.Id);

                    OnlineStatus info = SysSetting.GetSetting().Online[socket];
                    SysSetting.GetSetting().Online.Remove(socket);
                    if (!string.IsNullOrEmpty(info.ID))
                    {
                        new IMService.Common.Message.Offline(info.ID).Run();

                        if (SysSetting.GetSetting().Client.ContainsKey(info.ID))
                        {
                            SysSetting.GetSetting().Client.Remove(info.ID);
                        }
                    }
                };

                // 收到信息
                socket.OnMessage = message =>
                {
                    Console.WriteLine("收到信息：{0}", message);
                    IMessage msg = MessageFactory.GetMessage(message, socket);
                    if (msg == null)
                    {
                        Console.WriteLine("ERROR:{0}", message);
                        return;
                    }
                    msg.Run();
                };

                // 测试在线
                socket.OnPong = data =>
                {
                    if (SysSetting.GetSetting().Online.ContainsKey(socket))
                    {
                        SysSetting.GetSetting().Online[socket].ActiveTime = DateTime.Now;
                    }
                };
            });

            Timer timer = new Timer(60 * 1000);
            timer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                //#1 删除超过3分钟没有回应的链接
                foreach (IWebSocketConnection socket in SysSetting.GetSetting().Online.Where(t => t.Value.ActiveTime < DateTime.Now.AddMinutes(-3)).Select(t => t.Key).ToArray())
                {
                    socket.Close();
                }

                //#2 发送ping
                foreach (IWebSocketConnection socket in SysSetting.GetSetting().Online.Where(t => t.Key.IsAvailable).Select(t => t.Key))
                {
                    socket.SendPing(new byte[] { 0 });
                }

                //#3 发送系统通知
                /*
                foreach (var notify in UserAgent.Instance().GetNotifyList(SysSetting.GetSetting().SiteID))
                {
                    string key = string.Concat(UserAgent.IM_USER, "-", notify.UserID);
                    Console.WriteLine(key);
                    if (SysSetting.GetSetting().Client.ContainsKey(key))
                    {
                        Console.WriteLine(notify.Message);
                        SysSetting.GetSetting().Client[key].Send(new NotifyMessage()
                        {
                            Message = notify.Message,
                            NotifyType = notify.Type
                        });
                    }
                }
                */
            };
            timer.Start();

        }


        /// <summary>
        /// 执行命令
        /// </summary>
        static void Command()
        {
            ConsoleColor color = Console.ForegroundColor;

            bool isRead = true;
            while (isRead)
            {
                string input = Console.ReadLine();
                switch (input)
                {
                    case "userlist":
                        Console.ForegroundColor = ConsoleColor.Green;
                        foreach (KeyValuePair<string, User> item in SysSetting.GetSetting().Client)
                        {
                            Console.WriteLine("{0}\t{1}", item.Key, item.Value.Name);
                        }
                        Console.ForegroundColor = color;
                        break;
                    case "online":
                        Console.ForegroundColor = ConsoleColor.Green;
                        foreach (KeyValuePair<IWebSocketConnection, OnlineStatus> item in SysSetting.GetSetting().Online)
                        {
                            Console.WriteLine("{0}:{1}\t{2}\t{3} {4}", item.Key.ConnectionInfo.ClientIpAddress, item.Key.ConnectionInfo.ClientPort, item.Key.ConnectionInfo.Id, item.Value.ID, item.Value.ActiveTime);
                        }
                        Console.ForegroundColor = color;
                        break;
                    case "rebot":
                        Console.WriteLine(Rebot.tuling.GetResult("你好", "0"));
                        break;
                    case "rebotinfo":
                        Console.WriteLine(SysSetting.GetSetting().SiteInfo.Rebot.ToString());
                        break;
                    case "ping":
                        foreach (IWebSocketConnection socket in SysSetting.GetSetting().Client.Select(t => t.Value.Socket))
                        {
                            socket.SendPing(new byte[] { 0 });
                        }
                        break;
                    case "cls":
                        Console.Clear();
                        break;
                    case "exit":
                    case "quit":
                        isRead = false;
                        break;
                }
            }
        }
    }
}
