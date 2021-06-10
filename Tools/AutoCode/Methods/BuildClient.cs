using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

using SP.Studio.IO;

namespace AutoCode.Methods
{
    /// <summary>
    /// 生成客户端
    /// </summary>
    public class BuildClient : IDisposable
    {
        private const string PATH = @"D:\Program Files\NW.js\";

        private string ClientPath;

        private string Url;

        private string Name;

        public BuildClient()
        {
        }

        public static void Run(string[] args)
        {
            using (BuildClient client = new BuildClient())
            {
                Console.WriteLine("请输入客户端路径");
                client.ClientPath = Console.ReadLine();

                Console.WriteLine("跳转路径");
                client.Url = Console.ReadLine();

                Console.WriteLine("应用名称");
                client.Name = Console.ReadLine();

                client.CreateJson();
                client.CreateZip();
                client.CreateIcon();

                Console.Write("请修改图标，修改完成之后按回车");
                Console.ReadLine();

                client.CreateExe();
            }

        }

        private void CreateJson()
        {
            StringBuilder sb = new StringBuilder();
            if (!Directory.Exists(this.ClientPath))
            {
                Directory.CreateDirectory(this.ClientPath);
            }
            sb.Append("{")
                .AppendFormat("\"main\":\"{0}\",", this.Url)
                .AppendFormat("\"name\":\"{0}\",", this.Name)
                .AppendFormat("\"description\":\"{0}\",", this.Name)
                .AppendFormat("\"version\":\"2.0.0\",")
                .AppendFormat("\"keywords\":[\"{0}\"],", this.Name)
                .Append("\"window\":{")
                .AppendFormat("\"title\":\"{0}\",", this.Name)
                .Append("\"icon\": \"icon.png\",")
                .Append("\"toolbar\": false,")
                .Append("\"frame\": true,")
                .Append("\"width\": 1280,")
                .Append("\"height\": 680,")
                .Append("\"position\": \"center\",")
                .Append("\"min_width\": 1280,")
                .Append("\"min_height\": 680,")
                .Append("\"max_width\": 0,")
                .Append("\"max_height\": 0,")
                .Append("\"show_in_taskbar\":true")
                .Append("},\"webkit\":{ \"plugin\": true }")
                .Append("}");

            File.WriteAllText(this.ClientPath + @"\package.json", sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 创建压缩包
        /// </summary>
        private void CreateZip()
        {
            string zipPath = PATH + this.Name + ".zip";
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipAgent.Compression(zipPath, Directory.GetFiles(this.ClientPath));
        }

        /// <summary>
        /// 创建合并引用程序（创建之前应修改nw.exe的图标）
        /// </summary>
        private void CreateExe()
        {
            string zipPath = PATH + this.Name + ".zip";
            List<byte> data = File.ReadAllBytes(PATH + "nw.exe").ToList();
            data.AddRange(File.ReadAllBytes(zipPath));
            File.WriteAllBytes(PATH + this.Name + ".exe", data.ToArray());
            File.Delete(zipPath);
        }

        /// <summary>
        /// 创建图标文件
        /// </summary>
        private void CreateIcon()
        {
            string exe = Application.StartupPath + @"\png2ico.exe";
            Process process = new Process();
            process.StartInfo.FileName = exe;
            process.StartInfo.Arguments = string.Format("-s 16 32bpp -2 32 32bpp -s 48 32bpp -s 64 32bpp -s 96 32bpp -s 128 32bpp -s 256 32bpp -i \"{0}\" -o \"{0}\" -noconfirm", this.ClientPath);
            process.Start();
            process.WaitForExit();
        }

        public void Dispose()
        {

        }
    }
}
