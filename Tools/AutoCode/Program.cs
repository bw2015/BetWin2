using AutoCode.Methods;
using SP.Studio.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AutoCode
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("没有指定要执行的参数");
                Console.ReadLine();
                return;
            }

            switch (args[0])
            {
                case "admin":
                    BuidCode.Run(args.Skip(1).ToArray());
                    break;
                // 编译客户端
                case "client":
                    BuildClient.Run(args.Skip(1).ToArray());
                    break;
                // 采集服务使用硬编码
                case "BetWinClient":
                    BetWinClient.Run(args.Skip(1).ToArray());
                    break;
                //生成实体类
                case "model":
                    Model.Run(args.Skip(1).ToArray());
                    break;
                // 生成文档
                case "document":
                    BuildDocument.Run(args.Skip(1).ToArray());
                    break;
            }
        }

    }
}
