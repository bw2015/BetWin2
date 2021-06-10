using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Xml.Serialization;
using System.Windows.Forms;

using AutoCode.Methods.Models;

namespace AutoCode.Methods
{
    public static class Model
    {


        /// <summary>
        /// 当前文件路径
        /// </summary>
        private static string file;

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        /// <returns></returns>
        private static string studio()
        {
            string studioFile = null;
            DirectoryInfo directoryInfo = new FileInfo(file).Directory;
            while (directoryInfo != null)
            {
                studioFile = directoryInfo.FullName + "\\Studio.ini";
                if (File.Exists(studioFile))
                {
                    return studioFile;
                }
                directoryInfo = directoryInfo.Parent;
            }
            return null;
        }

        /// <summary>
        /// 配置参数转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T ToObject<T>(this string str) where T : class, new()
        {
            Encoding uTF = Encoding.UTF8;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream(uTF.GetBytes(str));
            T result;
            try
            {
                result = (T)((object)xmlSerializer.Deserialize(memoryStream));
            }
            catch
            {
                result = default(T);
            }
            finally
            {
                memoryStream.Close();
            }
            return result;
        }

        private static StudioConfig GetStudioConfig()
        {
            StudioConfig studioConfig = File.ReadAllText(studio()).ToObject<StudioConfig>() ?? new StudioConfig();
            return studioConfig;
        }

        public static DialogResult ErrorBox(string text)
        {
            return MessageBox.Show(text, "发生错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        /// <summary>
        /// 创建实体文件
        /// </summary>
        private static void Build()
        {
            string content = File.ReadAllText(file, Encoding.UTF8);

            string nameSpace = GetNameSpace(content);
            string tableName = GetTable(content);

            if (string.IsNullOrEmpty(tableName))
            {
                Console.Write("请输入表名（默认为{0}）：", tableName);
                string _tableName = Console.ReadLine();
                if (!string.IsNullOrEmpty(_tableName)) tableName = _tableName;
            }
            if (string.IsNullOrEmpty(tableName))
            {
                ErrorBox("未输入表名");
                return;
            }

            StudioConfig studio = GetStudioConfig();

            DataTable dataTable = SqlHelper.GetDataTable(studio, tableName);
            if (dataTable.Rows.Count == 0)
            {
                ErrorBox("表“" + tableName + "”不存在！");
                return;
            }

            string text = studio.ModelTemplate.Replace("${namespace}", nameSpace);
            string text2 = null;
            string text3 = null;
            string text4 = null;
            text = GetProperty(text, ref text2);

            int num = 0;
            StringBuilder stringBuilder = new StringBuilder();
            List<string> attributies = new List<string>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                if (num == 0)
                {
                    tableName = (string)dataRow["表名"];
                    string text5 = (string)dataRow["表说明"];
                    text5 = GetDescription(text5, ref text3, ref text4, ref attributies);
                    if (string.IsNullOrEmpty(text3))
                    {
                        text3 = tableName;
                    }
                    text = text.Replace("${tableName}", tableName).Replace("${className}", text3);
                    if (!string.IsNullOrEmpty(text5))
                    {
                        text5 = string.Format("/// <summary>\n\t/// {0}\n\t/// </summary>", text5);
                    }
                    text = text.Replace("${tableDescription}", text5).Replace("${tableAttribute}", attributies.Count == 0 ? "" : "," + string.Join(",", attributies));
                }

                attributies.Clear();
                string text6 = (string)dataRow["字段名"];
                bool flag = (string)dataRow["主键"] == "true";
                bool flag2 = (string)dataRow["标识"] == "true";
                string text7 = GetDescription((string)dataRow["字段说明"], ref text3, ref text4, ref attributies);
                text4 = GetType(text4, (string)dataRow["类型"]);
                if (string.IsNullOrEmpty(text3))
                {
                    text3 = text6;
                }
                if (!string.IsNullOrEmpty(text7))
                {
                    text7 = string.Format("/// <summary>\n\t\t/// {0}\n\t\t/// </summary>", text7);
                }
                string newValue = string.Format("[Column(Name = \"{0}\"{1}{2}){3}]",
                    text6, flag ? ", IsPrimaryKey = true" : "", flag2 ? ", IsDbGenerated = true" : "",
                    attributies.Count == 0 ? "" : "," + string.Join(",", attributies)
                    );
                stringBuilder.Append(text2.Replace("${type}", text4).Replace("${propertyName}", text3).Replace("${columnDescription}", text7).Replace("${columnAttribute}", newValue));
            }
            text = text.Replace("${property}", stringBuilder.ToString());

            Console.WriteLine(text);

            File.WriteAllText(file, text);
        }

        private static string GetNameSpace(string text)
        {
            Regex regex = new Regex("namespace.*?(?<NameSpace>[\\w\\.]+)");
            string result;
            if (!regex.IsMatch(text))
            {
                result = string.Empty;
            }
            else
            {
                result = regex.Match(text).Groups["NameSpace"].Value;
            }
            return result;
        }

        private static string GetProperty(string template, ref string property)
        {
            Regex regex = new Regex("#property foreach(?<PropertyInfo>(.|\\n)*)#property end");
            if (regex.IsMatch(template))
            {
                property = regex.Match(template).Groups["PropertyInfo"].Value;
                template = regex.Replace(template, "${property}");
            }
            return template;
        }

        private static string GetTable(string text)
        {
            Regex regex = new Regex("\\[Table\\(Name[\\s\\=]+\"(?<TableName>\\w+)\".*\\]");
            string result;
            if (!regex.IsMatch(text))
            {
                result = string.Empty;
            }
            else
            {
                result = regex.Match(text).Groups["TableName"].Value;
            }
            return result;
        }

        private static string GetType(string type, string columnType)
        {
            string result;
            if (!string.IsNullOrEmpty(type))
            {
                result = type;
            }
            else
            {
                switch (columnType)
                {
                    case "varchar":
                    case "nvarchar":
                    case "char":
                    case "nchar":
                    case "xml":
                        result = "string";
                        return result;
                    case "bigint":
                        result = "long";
                        return result;
                    case "int":
                        result = "int";
                        return result;
                    case "smallint":
                        result = "short";
                        return result;
                    case "tinyint":
                        result = "byte";
                        return result;
                    case "bit":
                        result = "bool";
                        return result;
                    case "smalldatetime":
                    case "datetime":
                    case "date":
                        result = typeof(DateTime).Name;
                        return result;
                    case "money":
                        result = typeof(decimal).Name;
                        return result;
                    case "uniqueidentifier":
                        result = "Guid";
                        return result;
                }
                result = columnType;
            }
            return result;
        }

        /// <summary>
        /// 获取字段的备注信息
        /// </summary>
        /// <param name="text"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private static string GetDescription(string text, ref string name, ref string type, ref List<string> attribute)
        {
            string text2;
            type = (text2 = null);
            name = text2;
            Regex regex = new Regex(@"\[(?<Name>[^\]]+)\]", RegexOptions.IgnoreCase);
            if (regex.IsMatch(text))
            {
                foreach (Match match in regex.Matches(text))
                {
                    string item = match.Groups["Name"].Value;

                    switch (item)
                    {
                        case "ICache":
                            attribute.Add(item);
                            break;
                        default:

                            if (item.StartsWith("Type="))
                            {
                                type = item.Substring(5);
                            }
                            else if (item.StartsWith("Name="))
                            {
                                name = item.Substring(5);
                            }
                            else
                            {
                                name = item;
                            }
                            break;
                    }
                }
            }
            return Regex.Replace(text, "\\[.*\\]", string.Empty).Replace("\n", "\t").Replace("\r", "\t");
        }

        public static void Run(string[] args)
        {
            file = args[0];
            if (!file.EndsWith(".cs"))
            {
                Console.WriteLine(file);
                Console.WriteLine("不支持文件");
                return;
            }

            Build();
        }
    }
}
