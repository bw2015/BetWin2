using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Framework;


namespace BW.Common
{
    /// <summary>
    /// 数据库操作对象
    /// </summary>
    public  sealed partial class BetDataContext : DataContext
    {
        private static MappingSource mapping = new AttributeMappingSource();

        public BetDataContext()
            : base(SysSetting.GetSetting().DbConnection, mapping)
        {

        }

    }
}
