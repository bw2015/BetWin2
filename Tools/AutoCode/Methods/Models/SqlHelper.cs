using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AutoCode.Methods.Models
{
    internal class SqlHelper
    {
        #region ===========  获取表结构的SQL  ============

        /// <summary>
        /// 获取表结构的SQL
        /// </summary>
        private const string sql = @"SELECT 
	表名       = case when a.colorder=1 then d.name else '' end, 
	表说明     = case when a.colorder=1 then isnull(f.value,'') else '' end, 
	字段序号   = a.colorder, 
	字段名     = a.name, 
	标识       = case when COLUMNPROPERTY( a.id,a.name,'IsIdentity')=1 then 'true'else 'false' end, 
	主键       = case when exists(SELECT 1 FROM sysobjects where xtype='PK' and parent_obj=a.id and name in ( 
	                 SELECT name FROM sysindexes WHERE indid in( SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid))) then 'true' else 'false' end, 
	类型       = b.name, 
	占用字节数 = a.length, 
	长度       = COLUMNPROPERTY(a.id,a.name,'PRECISION'), 
	小数位数   = isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0), 
	允许空     = case when a.isnullable=1 then 'true'else 'false' end, 
	默认值     = isnull(e.text,''), 
	字段说明   = isnull(g.[value],'') 
	FROM  
	syscolumns a 
	left join  
	systypes b  
	on  
	a.xusertype=b.xusertype 
	inner join  
	sysobjects d  
	on  
	a.id=d.id  and d.xtype='U' and  d.name<>'dtproperties' 
	left join  
	syscomments e  
	on  
	a.cdefault=e.id 
	left join  
	sys.extended_properties   g  
	on  
	a.id=G.major_id and a.colid=g.minor_id   
	left join  

	sys.extended_properties f 
	on  
	d.id=f.major_id and f.minor_id=0 
	where  
	d.name = @Table    
	order by  
	a.id,a.colorder";

        #endregion

        static SqlHelper()
        {

        }

        internal static DataTable GetDataTable(StudioConfig studioConfig, string tableName)
        {
            return GetDataTable(studioConfig.DbConnection, tableName);
        }

        internal static DataTable GetDataTable(string studioConfig, string tableName)
        {
            DataTable result;
            using (SqlConnection sqlConnection = new SqlConnection(studioConfig))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(new SqlCommand(sql, sqlConnection)
                {
                    Parameters =
                    {
                        new SqlParameter("@Table", tableName)
                    }
                });
                DataSet dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);
                sqlConnection.Close();
                result = dataSet.Tables[0];
            }
            return result;
        }

        /// <summary>
        /// 获取系统内所有的表名
        /// </summary>
        /// <param name="studioConfig"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetDataTable(string connection)
        {
            DataTable result;
            using (SqlConnection sqlConnection = new SqlConnection(connection))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(new SqlCommand("Select Name FROM SysObjects Where XType='U' ORDER BY [Name]", sqlConnection));
                DataSet dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);
                sqlConnection.Close();
                result = dataSet.Tables[0];
            }
            foreach (DataRow dr in result.Rows)
            {
                yield return (string)dr[0];
            }
        }
    }
}
