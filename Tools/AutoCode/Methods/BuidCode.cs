using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;
using System.Configuration;
using System.Data;
using SP.Studio.Data;
using SP.Studio.Model;

namespace AutoCode.Methods
{
    /// <summary>
    /// 创建管理员权限文件
    /// </summary>
    public class BuidCode : DbAgent
    {
        private static string DbConnection
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            }
        }

        public BuidCode()
            : base(DbConnection, DatabaseType.SqlServer, DataConnectionMode.Instance)
        {

        }

        private void UpdateSite(int siteId, string xml)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE Site SET SysConfig = @Config WHERE SiteID = @SiteID",
                    NewParam("@Config", xml),
                    NewParam("@SiteID", siteId));
            }
        }

        private string GetSiteConfig(int siteId)
        {
            using (DbExecutor db = NewExecutor())
            {
                return (string)db.ExecuteScalar(CommandType.Text, "SELECT SysConfig FROM Site WHERE SiteID = @SiteID",
                     NewParam("@SiteID", siteId));
            }
        }

        public static void Run(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("没有指定路径");
                return;
            }

            XElement root = null;

            int siteId = 0;

            string source = args[0];
            string target = args[1];

            bool isFile = !int.TryParse(source, out siteId);

            if (isFile)
            {
                if (!File.Exists(source))
                {
                    Console.WriteLine("来源路径不存在");
                    return;
                }
                root = XElement.Parse(File.ReadAllText(source));
            }
            else
            {
                root = XElement.Parse(new BuidCode().GetSiteConfig(siteId));
            }


            SetID(root);

            if (isChange)
            {
                if (isFile)
                {
                    File.WriteAllText(source, root.ToString());
                }
                else
                {
                    new BuidCode().UpdateSite(siteId, root.ToString());
                }
            }

            Regex regex = new Regex(@"\\(?<Name>\w+)\.cs$", RegexOptions.IgnoreCase);
            string name = regex.Match(target).Groups["Name"].Value;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"using System;")
                .AppendLine("using System.Collections.Generic;")
            .Append("namespace BW{")
            .AppendFormat("public class {0}", name)
            .AppendLine("{");

            sb.AppendLine(@"public static readonly Dictionary<string, string> NAME = new Dictionary<string, string>(){ ${NAME} };");

            foreach (XElement item in root.Elements())
            {
                Build(item, sb);
            }
            sb.AppendLine(@"}}");
            string codeFile = sb.ToString().Replace("${NAME}", string.Join(",\n", _name.Select(t => string.Concat("{\"", t.Key, "\",\"", t.Value, "\"}"))));
            File.WriteAllText(target, codeFile, Encoding.UTF8);
            Console.WriteLine(codeFile);
        }

        static bool isChange = false;

        static void SetID(XElement element)
        {
            if (element.Attribute("ID") == null)
            {
                isChange = true;
                element.SetAttributeValue("ID", Guid.NewGuid().ToString("N"));
            }
            foreach (XElement child in element.Elements())
            {
                SetID(child);
            }
        }

        private static Dictionary<string, string> _name = new Dictionary<string, string>();

        static void Build(XElement element, StringBuilder sb)
        {
            string name = element.Attribute("name").Value;
            string value = element.Attribute("ID").Value;
            string tag = element.Name.ToString();
            switch (tag)
            {
                case "menu":
                    sb.AppendLine(" public class " + name + "   { ");
                    sb.AppendFormat("public const string Value = \"{0}\";", value).AppendLine();
                    foreach (XElement item in element.Elements())
                    {
                        Build(item, sb);
                    }
                    sb.AppendLine("}");
                    break;
                case "action":
                    sb.AppendFormat("public const string {0} = \"{1}\";", name, value);
                    break;
            }
            List<string> permissionName = new List<string>();
            while (element != null && element.Attribute("name") != null)
            {
                permissionName.Add(element.Attribute("name").Value);
                element = element.Parent;
            }
            if (!_name.ContainsKey(value)) _name.Add(value, string.Join(".", permissionName));
        }
    }
}
