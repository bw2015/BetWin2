using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.Xml.Linq;
using System.Web.Security;
using System.Net;
using System.Text.RegularExpressions;

using System.Configuration;
namespace MY18
{
    class Program
    {
        /// <summary>
        /// 数据库文件的位置
        /// </summary>
        public static string mdb
        {
            get
            {
                return ConfigurationManager.AppSettings["mdb"];
            }
        }

        public static string key
        {
            get
            {
                return ConfigurationManager.AppSettings["key"];
            }
        }

        public static string gateway
        {
            get
            {
                return ConfigurationManager.AppSettings["gateway"];
            }
        }

        public static string type
        {
            get
            {
                return ConfigurationManager.AppSettings["type"] ?? "MY18";
            }
        }

        /// <summary>
        /// 已经成功的订单
        /// </summary>

        private static Dictionary<string, bool> success = new Dictionary<string, bool>();

        static void Main(string[] args)
        {
            string dbConnection = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Mode=Read;Persist Security Info=False;Jet OLEDB:Database Password=123456789;", mdb);

            while (true)
            {
                Console.Clear();
                List<Transfer> list = ReadData(dbConnection);
                Console.WriteLine("[{0}]开始采集", DateTime.Now);
                list.ForEach(t =>
                {
                    if (success.ContainsKey(t.SystemID)) return;
                    if (t.Date < DateTime.Now.AddHours(-1)) return;
                    UploadData(t);
                });

                System.Threading.Thread.Sleep(10 * 1000);
            }
        }

        /// <summary>
        /// 上传订单至网关接口
        /// </summary>
        /// <param name="order"></param>
        static void UploadData(Transfer order)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                    Console.Write("流水号：{0}  姓名：{1}   时间:{2}  金额：{3}  ", order.SystemID, order.Name, order.Date, order.Money.ToString("c"));

                    Console.WriteLine(gateway);
                    string result = Encoding.UTF8.GetString(wc.UploadData(gateway, "POST", Encoding.UTF8.GetBytes(order.ToString())));
                    if (result == "SUCCESS" || result.Contains("该流水号在系统中已经存在"))
                    {
                        if (!success.ContainsKey(order.SystemID)) success.Add(order.SystemID, true);
                    }
                    Console.WriteLine("结果：{0}", result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        static List<Transfer> ReadData(string dbConnection)
        {
            List<Transfer> list = new List<Transfer>();
            string sql = "SELECT TOP 32 * FROM neirong ORDER BY datet DESC";
            switch (type)
            {
                case "KHB":
                    sql = "SELECT TOP 32 * FROM payList ORDER BY payTime DESC";
                    break;
            }
            try
            {
                OleDbConnection conn = new OleDbConnection(dbConnection);
                conn.Open();
                OleDbCommand comm = new OleDbCommand(sql, conn);
                OleDbDataAdapter adapter = new OleDbDataAdapter(comm);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                adapter.Dispose();
                comm.Dispose();
                conn.Close();
                conn.Dispose();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new Transfer(dr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;

        }
    }

    /// <summary>
    /// 转账记录
    /// </summary>
    class Transfer
    {
        public Transfer(DataRow dr)
        {
            DataColumnCollection column = dr.Table.Columns;

            switch (Program.type)
            {
                case "MY18":
                    //MY18
                    if (column.Contains("Datet")) this.Date = DateTime.Parse((string)dr["Datet"]);
                    if (column.Contains("Tbid")) this.SystemID = (string)dr["Tbid"];
                    if (column.Contains("Jiaoyifang")) this.Name = (string)dr["Jiaoyifang"];
                    if (column.Contains("Jine")) this.Money = decimal.Parse(dr["Jine"].ToString());
                    if (column.Contains("caijikind")) this.Bank = (string)dr["caijikind"];
                    break;
                case "KHB":
                    //KHB
                    if (column.Contains("payTime")) this.Date = DateTime.Parse((string)dr["payTime"]);
                    if (column.Contains("numID")) this.SystemID = Regex.Replace(dr["numID"].ToString(), @"[^\d]", string.Empty);
                    if (column.Contains("otheInfo")) this.Name = (string)dr["otheInfo"];
                    if (string.IsNullOrEmpty(this.Name) && column.Contains("payUser")) this.Name = (string)dr["payUser"];
                    if (column.Contains("payMoney")) this.Money = decimal.Parse(dr["payMoney"].ToString());
                    if (column.Contains("source")) this.Bank = (string)dr["source"];
                    break;
            }

        }

        /// <summary>
        /// 转账时间
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 转账ID
        /// </summary>
        public string SystemID { get; set; }

        /// <summary>
        /// 转账方的姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 转账金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 银行
        /// </summary>
        public string Bank { get; set; }

        public override string ToString()
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("Date", this.Date.ToString());
            dic.Add("SystemID", this.SystemID);
            dic.Add("Name", this.Name);
            dic.Add("Money", this.Money.ToString("0.00"));
            dic.Add("Bank", this.Bank);
            string signstr = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&key=" + Program.key;
            dic.Add("sign", this.toMD5(signstr));
            return string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
        }

        public string toMD5(string text)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(text, "MD5");
        }
    }

}
