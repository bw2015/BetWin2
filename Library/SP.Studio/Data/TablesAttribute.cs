using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;

namespace SP.Studio.Data
{
    public class TablesAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
