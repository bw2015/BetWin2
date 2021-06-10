using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace AutoCode.Methods
{
    public class BetWinClient
    {

        public static void Run(params string[] args)
        {
            string path = args[0];
            string file = path + @"\AutoCode.cs";
            Dictionary<string, int> indexLengthDic = GetIndexLength(args[1]);

            Regex className = new Regex(@"^\s+public partial class (?<Name>\w+) : IGateway", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex methodName = new Regex(@"^\s+private Dictionary<string, string> (?<Name>\w+)\(\)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"using System;using System.Timers;using BetWinClient.Gateway;")
                .AppendLine("namespace BetWinClient.Gateway{");
            List<string> Lottery = new List<string>();
            foreach (string f in Directory.GetFiles(path))
            {
                string code = File.ReadAllText(f, Encoding.UTF8);
                if (!className.IsMatch(code)) continue;

                string name = className.Match(code).Groups["Name"].Value;
                Lottery.Add(name);
                List<string> method = new List<string>();
                foreach (Match match in methodName.Matches(code))
                {
                    method.Add(match.Groups["Name"].Value);
                }

                sb.AppendFormat("partial class {0}", name)
                    .Append("{")
                    .AppendFormat("protected override int IndexLength => {0};", indexLengthDic.ContainsKey(name) ? indexLengthDic[name] : 0)
                    .AppendFormat("public {0}()", name)
                    .Append("{")
                    .AppendFormat("this.Run({0});", string.Join(",", method))
                    .Append("}  }");
            }
            sb.AppendLine("}");

            sb.Append("namespace BetWinClient{ ")
                .Append(" partial class Service1{")
                .Append("private void Start(){ ");
            Lottery.ForEach(t =>
            {
                sb.AppendFormat(" if (Utils.IsGame(\"{0}\"))", t)
                .Append("{")
                .AppendFormat("Timer {0} = new Timer(2500);", t.ToLower())
                .AppendFormat("{0}.Elapsed += (a, b) => new {1}();", t.ToLower(), t)
                .AppendFormat("{0}.Start();", t.ToLower())
                .Append("}")
                .AppendLine();
            });
            sb.Append("       }    } }");

            File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 获取彩期的长度
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Dictionary<string, int> GetIndexLength(string path)
        {
            Assembly ass = Assembly.LoadFile(path);
            Type type = ass.GetType("BW.Common.Lottery.LotteryType");
            Type lottery = ass.GetType("BW.Common.Lottery.LotteryAttribute");
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (FieldInfo t in type.GetFields())
            {
                dynamic info = t.GetCustomAttribute(lottery);
                if (info == null) continue;
                dic.Add(t.Name, (int)info.IndexLength);
            }

            return dic;
        }
    }
}
