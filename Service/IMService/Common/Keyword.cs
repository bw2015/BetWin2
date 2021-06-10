using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace IMService.Common
{
    /// <summary>
    /// 关键词
    /// </summary>
    public class Keyword
    {
        public Keyword(DataRow dr)
        {
            this.Key = ((string)dr["Keyword"]).Split(' ', ',', '|').Where(t => !string.IsNullOrEmpty(t));
            this.Content = (string)dr["Content"];
        }

        public IEnumerable<string> Key { get; set; }

        public string Content { get; set; }
    }
}
