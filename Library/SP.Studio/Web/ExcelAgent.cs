using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;

using SP.Studio.Data;

namespace SP.Studio.Web
{
    /// <summary>
    /// Web中的Excel操作
    /// </summary>
    public class ExcelAgent : DbAgent
    {
        public ExcelAgent() { }

        public ExcelAgent(string file)
            : base(string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Extended Properties=\"Excel 8.0\";Data Source={0}", file),
            DatabaseType.Access)
        {
            Console.WriteLine(file);
        }

        /// <summary>
        /// 插入一条记录
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        public void Insert(string tableName, params object[] values)
        {
            using (DbExecutor db = NewExecutor())
            {
                IDbOperation operation = NewOperation(db);
                operation.Insert(tableName, values);
            }
        }

        /// <summary>
        /// 根据HTML创建一个Excel文件供下载
        /// </summary>
        public static void CreateExcel(string html, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName + ".xls");
            HttpContext.Current.Response.Charset = "UTF-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head><meta http-equiv=Content-Type content=\"text/html; charset=utf-8\"></head><body>{0}</body></html>", html);
            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        public DataTable GetTable(string tableName)
        {
            using (DbExecutor db = NewExecutor())
            {
                return db.GetDataSet(CommandType.Text, string.Format("SELECT * FROM [{0}A:Z]", tableName)).Tables[0];
            }
        }
    }
}
