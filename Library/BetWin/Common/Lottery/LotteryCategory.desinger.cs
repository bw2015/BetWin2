using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 彩种所属分类
    /// </summary>
    [Table(Name = "lot_Category")]
    public partial class LotteryCate
    {
        [Column(Name = "CateID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        [Column(Name = "CateName")]
        public string Name { get; set; }

        [Column(Name = "Sort")]
        public short Sort { get; set; }

        [Column(Name = "Code")]
        public string Code { get; set; }
    }
}
