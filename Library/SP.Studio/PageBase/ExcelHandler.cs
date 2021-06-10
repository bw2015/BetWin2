using System;
using System.Web;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data;
using System.Text;

using SP.Studio.Data;
using SP.Studio.Core;
using SP.Studio.Web;

namespace SP.Studio.PageBase
{
    /// <summary>
    /// Excel 模拟器的服务端
    /// </summary>
    public abstract class ExcelHandler : DbAgent, IHttpHandler
    {
        /// <summary>
        /// 当前要处理的表格名称
        /// </summary>
        protected virtual string TableName
        {
            get
            {
                return WebAgent.GetParam("tableName");
            }
        }

        /// <summary>
        /// 当前动作
        /// </summary>
        protected virtual ActionType Action
        {
            get
            {
                return WebAgent.GetParam("ac").ToLower().ToEnum<ActionType>();
            }
        }

        /// <summary>
        /// 要查询的字段
        /// </summary>
        protected virtual string[] Fields
        {
            get
            {
                return WebAgent.GetParam("Fields").Split(',');
            }
        }

        /// <summary>
        /// 要修改的字段名
        /// </summary>
        protected virtual string Field
        {
            get
            {
                return WebAgent.GetParam("Field");
            }
        }

        /// <summary>
        /// 要修改的值
        /// </summary>
        protected virtual string Value
        {
            get
            {
                return WebAgent.GetParam("Value");
            }
        }

        /// <summary>
        /// 主键名称
        /// </summary>
        protected virtual string IndexKey
        {
            get
            {
                return WebAgent.GetParam("IndexKey");
            }
        }

        /// <summary>
        /// 主键值
        /// </summary>
        protected virtual string Index
        {
            get
            {
                return WebAgent.GetParam("Index");
            }
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        protected virtual string Condition
        {
            get
            {
                return WebAgent.GetParam("Condition");
            }
        }

        /// <summary>
        /// 当前查询的页码
        /// </summary>
        protected virtual int PageIndex
        {
            get
            {
                return WebAgent.GetParam("PageIndex", 1);
            }
        }

        /// <summary>
        /// 每页大小
        /// </summary>
        protected virtual int PageSize
        {
            get
            {
                return WebAgent.GetParam("PageSize", 20);
            }
        }

        /// <summary>
        /// 排序方式
        /// </summary>
        protected virtual string Sort
        {
            get
            {
                return WebAgent.GetParam("Sort");
            }
        }

        /// <summary>
        /// 抛出错误
        /// </summary>
        protected virtual void ShowError(HttpContext context, string msg, params object[] args)
        {
            context.Response.Write(string.Concat("{ \"success\" : 0, \"msg\" : \"", HttpUtility.JavaScriptStringEncode(string.Format(msg, args)), "\" }"));
        }

        /// <summary>
        /// 显示成功信息
        /// </summary>
        protected virtual void ShowSuccess(HttpContext context, string msg, params object[] args)
        {
            context.Response.Write(string.Concat("{ \"success\" : 1, \"msg\" : \"", HttpUtility.JavaScriptStringEncode(string.Format(msg, args)), "\" }"));
        }

        /// <summary>
        /// 动作类型
        /// </summary>
        public enum ActionType
        {
            none,
            /// <summary>
            /// 
            /// </summary>
            list,
            /// <summary>
            /// 更新
            /// </summary>
            update,
            /// <summary>
            /// 删除
            /// </summary>
            delete,
            /// <summary>
            /// 显示属性的类型
            /// </summary>
            type
        }

        /// <summary>
        /// 当前的数据库操作对象
        /// </summary>
        protected virtual DbExecutor db
        {
            get
            {
                return DataExtension.GetDbExecutor();
            }
        }

        /// <summary>
        /// 要显示的字段名称
        /// </summary>
        protected virtual string GetFieldName(string columnName)
        {
            return columnName;
        }

        /// <summary>
        /// 显示值
        /// </summary>
        protected virtual string GetFieldValue(object value)
        {
            return value.ToString().Replace("\"", "\\\"");
        }

        /// <summary>
        /// 输出json格式的列表
        /// </summary>
        protected virtual void GetList(HttpContext context)
        {

            using (IDbOperation op = new SqlDbOperation(db))
            {
                int recordCount;
                DataSet ds = op.GetList(this.PageIndex, this.PageSize, this.TableName, string.Join(",", this.Fields), this.Condition, this.Sort, out recordCount);

                StringBuilder sb = new StringBuilder("{ \"RecordCount\" : " + recordCount);
                sb.AppendFormat(", \"Rows\" : [");

                List<string> rows = new List<string>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    List<string> json = new List<string>();
                    Object obj = this.GetObject(dr);
                    if (obj == null)
                    {
                        foreach (DataColumn column in dr.Table.Columns)
                        {
                            string name = column.ColumnName;
                            string value = dr[column].ToString();
                            json.Add(string.Format("\"{0}\" : \"{1}\"", this.GetFieldName(name), this.GetFieldValue(value)));
                        }
                        rows.Add(string.Concat("{", string.Join(",", json), "}"));
                    }
                    else
                    {
                        rows.Add(obj.ToJson());
                    }
                }

                sb.Append(string.Join(",", rows));

                sb.Append("] }");

                context.Response.Write(sb);
            }
        }

        /// <summary>
        /// 通过实体对象做转换
        /// </summary>
        protected virtual Object GetObject(DataRow dr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 修改字段值
        /// </summary>
        protected virtual void Update(HttpContext context)
        {
            string sql = string.Format("UPDATE [{0}] SET [{1}] = @Value WHERE [{2}] = @Index", this.TableName, this.Field, this.IndexKey);
            try
            {
                int rows = db.ExecuteNonQuery(CommandType.Text, sql,
                     NewParam("@Value", this.Value),
                     NewParam("@Index", this.Index));
                this.ShowSuccess(context, "更新成功，受影响行数{0}条", rows);
            }
            catch (Exception ex)
            {
                this.ShowError(context, ex.Message);
            }
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        protected virtual void Delete(HttpContext context)
        {
            string sql = string.Format("DELETE FROM [{0}] WHERE [{1}] = @Index", this.TableName, this.IndexKey);
            try
            {
                int rows = db.ExecuteNonQuery(CommandType.Text, sql,
                    NewParam("@Index", this.Index));
                this.ShowSuccess(context, "删除成功，受影响行数{0}条", rows);
            }
            catch (Exception ex)
            {
                this.ShowError(context, ex.Message);
            }
        }

        /// <summary>
        /// 显示字段的类型
        /// </summary>
        protected virtual void Type(HttpContext context)
        {

        }

        /// <summary>
        /// 开始之前要执行的方法
        /// </summary>
        protected abstract void OnRequest(HttpContext context);


        public virtual void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/json";
            this.OnRequest(context);

            switch (this.Action)
            {
                case ActionType.list:
                    this.GetList(context);
                    break;
                case ActionType.update:
                    this.Update(context);
                    break;
                case ActionType.delete:
                    this.Delete(context);
                    break;
                case ActionType.type:
                    this.Type(context);
                    break;
            }
        }

        public virtual bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// 类型的属性
        /// </summary>
        protected struct TypeProperty
        {
            public TypeProperty(PropertyInfo property)
            {
                this.Name = property.Name;
                this.Type = property.PropertyType.Name;
                this.FullName = property.PropertyType.FullName.ToString();
                this.IsEnum = property.PropertyType.IsEnum;
            }

            /// <summary>
            /// 属性名称
            /// </summary>
            public string Name;

            /// <summary>
            /// 类型名称和全名
            /// </summary>
            public string Type, FullName;

            /// <summary>
            /// 是否是枚举
            /// </summary>
            public bool IsEnum;

            public string ToJson()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.AppendFormat("\"Name\" : \"{0}\" ,", this.Name);
                sb.AppendFormat("\"Type\" : \"{0}\" ,", this.Type);
                sb.AppendFormat("\"FullName\" : \"{0}\" ,", this.FullName);
                sb.AppendFormat("\"IsEnum\" : {0}", this.IsEnum ? 1 : 0);
                sb.Append("}");
                return sb.ToString();
            }
        }

        /// <summary>
        /// 枚举
        /// </summary>
        protected struct EnumProperty
        {
            public EnumProperty(Type type)
            {
                list = new List<Tuple<int, string, string>>();

                foreach (dynamic e in Enum.GetValues(type))
                {
                    try
                    {
                        Enum en = (Enum)e;
                        int value = (int)e;

                        string name = en.ToString();
                        string description = en.GetDescription();
                        list.Add(new Tuple<int, string, string>(value, name, description));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + e);
                    }
                }
            }

            public List<Tuple<int, string, string>> list;

            public string ToJson()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                sb.Append(string.Join(",", list.ConvertAll(t =>
                {
                    return string.Concat("{ \"Name\" : \"", t.Item2, "\" , \"Value\" : ", t.Item1, ", \"Description\" : \"", t.Item3, "\" }");
                })));
                sb.Append("]");
                return sb.ToString();
            }
        }
    }
}
