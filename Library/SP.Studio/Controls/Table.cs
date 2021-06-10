using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Reflection;

using SP.Studio.Core;
using SP.Studio.Text;
using System.Drawing;

namespace SP.Studio.Controls
{
    /// <summary>
    /// 数据表格控件
    /// </summary>
    [DefaultProperty("Table"), ToolboxData("<{0}:Table Author=\"曹雁斌\" runat=\"server\" />")]
    public class Table : System.Web.UI.WebControls.Table
    {
        private PropertyInfo[] fields;

        private List<object> nullList = new List<object>();

        public object DataSource { private get; set; }

        private string _none = "没有记录";

        public string None
        {
            get
            {
                return _none;
            }
            set
            {
                _none = value;
            }
        }

        public void DataBind<T>(params Expression<Func<T, object>>[] funs) where T : class,new()
        {
            if (funs.Length == 0)
                this.fields = typeof(T).GetProperties();
            else
                this.fields = funs.ToList().ConvertAll(t => t.GetPropertyInfo()).ToArray();

            TableHeaderRow headRow = new TableHeaderRow() { TableSection = TableRowSection.TableHeader };

            foreach (PropertyInfo property in fields)
            {
                DescriptionAttribute desc = property.GetAttribute<DescriptionAttribute>();
                string name = desc == null ? property.Name : desc.Description;
                headRow.Cells.Add(new TableHeaderCell()
                {
                    Text = name
                });
            }
            this.Rows.Add(headRow);

            IList<T> list = ((IEnumerable<T>)DataSource).ToList();
            if (list.Count == 0)
            {
                TableRow noneRow = new TableRow() { TableSection = TableRowSection.TableBody };
                TableCell noneCell = new TableCell() { ColumnSpan = headRow.Cells.Count, CssClass = "none", Text = None };
                noneRow.Cells.Add(noneCell);
                this.Rows.Add(noneRow);
            }
            else
            {
                foreach (T t in list)
                {
                    TableRow row = new TableRow() { TableSection = TableRowSection.TableBody };
                    foreach (var exp in funs)
                    {
                        TableCell cell = new TableCell()
                        {
                            Text = exp.Compile().Invoke(t).ToString()
                        };
                        row.Cells.Add(cell);
                    }
                    this.Rows.Add(row);
                }
            }
        }

        /// <summary>
        /// 获取单元格对象
        /// </summary>
        public TableCell GetCell(int row, int cell)
        {
            return this.Rows[row].Cells[cell];
        }

        /// <summary>
        /// 获取一整列
        /// </summary>
        /// <param name="start"></param>
        /// <param name="cell"></param>
        public void AddClass(string className, Func<int, bool> r = null, Func<int, bool> c = null)
        {
            if (r == null) r = t => true;
            if (c == null) c = t => true;
            for (int rowIndex = 0; rowIndex < this.Rows.Count; rowIndex++)
            {
                int cellCount = this.Rows[rowIndex].Cells.Count;
                for (int colIndex = 0; colIndex < cellCount; colIndex++)
                {
                    if (r.Invoke(rowIndex) && (c.Invoke(colIndex) || c.Invoke(colIndex - cellCount)))
                    {
                        this.Rows[rowIndex].Cells[colIndex].CssClass = className;
                    }
                }
            }
        }

        public void AddColumn<T>(string header, Func<T, string> fun, int indexAt = -1) where T : class,new()
        {
            if (indexAt == -1) indexAt = this.Rows[0].Cells.Count;
            int index = 0;
            IList<T> list = ((IEnumerable<T>)DataSource).ToList();
            foreach (TableRow row in this.Rows)
            {
                if (index == 0)
                {
                    row.Cells.AddAt(indexAt, new TableHeaderCell()
                    {
                        Text = header
                    });
                }
                else if (list.Count > 0)
                {
                    row.Cells.AddAt(indexAt, new TableCell()
                    {
                        Text = fun.Invoke(list[index - 1])
                    });
                }
                index++;
            }
        }

        public void SetHeader(params string[] names)
        {
            this.SetHeader(0, names);
        }

        /// <summary>
        /// 设置表头单元格信息
        /// </summary>
        /// <param name="start"></param>
        /// <param name="names"></param>
        public void SetHeader(int start, params string[] names)
        {
            var nameIndex = 0;
            int count = this.Rows[0].Cells.Count;
            if (start < 0) start = count + start;
            for (var index = 0; index < count; index++)
            {
                if (index < start) continue;
                if (names.Length == nameIndex) return;
                string name = names[nameIndex];
                this.Rows[0].Cells[index].Text = name;
                nameIndex++;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            writer.Write("<script language=\"javascript\" type=\"text/javascript\">" +
                            "$import(\"UI.Table.js\");" +
                            "window.addEvent(\"domready\", function () {" +
                            string.Format("new UI.Table(\"{0}\");", this.ClientID) +
                            "});" +
                        "</script>");
        }
    }
}
