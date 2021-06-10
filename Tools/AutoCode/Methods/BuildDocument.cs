using AutoCode.Methods.Models;
using SP.Studio.Array;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoCode.Methods
{
    /// <summary>
    /// 创建数据库文档
    /// </summary>
    public class BuildDocument
    {
        private static StringBuilder sb;
        public static void Run(string[] args)
        {
            sb = new StringBuilder();
            string connection = args.GetArgument("-conn", string.Empty);

            string style = args.GetArgument("-style", string.Empty);

            if (string.IsNullOrEmpty(connection))
            {
                Console.WriteLine("未指定数据库连接参数 -conn");
                return;
            }

            Regex regex = new Regex(@"database=(?<Name>.+?);");
            if (!regex.IsMatch(connection))
            {
                Console.WriteLine("连接字符串不含 database 关键词");
                return;
            }
            string database = regex.Match(connection).Groups["Name"].Value;

            sb.AppendFormat("<h1>{0}</h1>", database);
            sb.AppendFormat("<h2>更新时间{0}</h2>", DateTime.Now.ToString("yyyy年MM月dd日 dddd HH:mm:ss"));

            var tables = SqlHelper.GetDataTable(connection);
            int total = tables.Count();
            int tableIndex = 0;
            foreach (string tableName in tables)
            {
                tableIndex++;
                Console.CursorLeft = 0;
                Console.Write($"当前进度{tableIndex}/{total}");
                DataTable dt = SqlHelper.GetDataTable(connection, tableName);
                int index = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    if (index == 0)
                    {
                        sb.AppendFormat($"<h3>{dr["表名"]}</h3><h4>{dr["表说明"]}</h4>")
                            .Append("<table>")
                            .Append("<thead><tr><th>序号</th><th>字段名</th><th>标识</th><th>主键</th><th>类型</th><th>长度</th><th>默认值</th><th>说明</th></tr></thead>")
                            .Append("<tbody>");
                    }
                    sb.Append("<tr>")
                        .AppendFormat("<td>{0}</td>", dr["字段序号"])
                        .AppendFormat("<td>{0}</td>", dr["字段名"])
                        .AppendFormat("<td>{0}</td>", dr["标识"])
                        .AppendFormat("<td>{0}</td>", dr["主键"])
                        .AppendFormat("<td>{0}</td>", dr["类型"])
                        .AppendFormat("<td>{0}</td>", dr["长度"])
                        .AppendFormat("<td>{0}</td>", dr["默认值"])
                        .AppendFormat("<td>{0}</td>", dr["字段说明"])
                        .Append("</tr>");

                    index++;
                }
                sb.Append("</tbody></table>");
            }

            StringBuilder html = new StringBuilder();
            html.Append("<html><head><meta charset=\"utf-8\" />")
                .AppendFormat("<title>{0}</title>", database)
                .Append("<meta name=\"renderer\" content=\"webkit\">")
                .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=0\" />");

            if (!string.IsNullOrEmpty(style))
            {
                style = System.Windows.Forms.Application.StartupPath + @"\" + style;
                if (File.Exists(style))
                {
                    html.Append("<style>")
                        .Append(File.ReadAllText(style, Encoding.UTF8))
                        .Append("</style>");
                }
            }


            html.Append("</head><body>")
                .Append(sb.ToString())
                .Append("</body></html>");

            string file = System.Windows.Forms.Application.StartupPath + @"\" + database + ".html";

            File.WriteAllText(file, html.ToString(), Encoding.UTF8);
        }
    }
}
