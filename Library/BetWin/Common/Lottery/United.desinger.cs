/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    ///  彩票合買
    /// </summary>
    [Table(Name = "lot_United")]
    public partial class United
    {


        [Column(Name = "UnitedID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 合買的發起者
        /// </summary>
        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 合买标题
        /// </summary>
        [Column(Name = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// 總金額
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 彩票類型
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Lottery.LotteryType Type { get; set; }

        /// <summary>
        /// 期號
        /// </summary>
        [Column(Name = "Index")]
        public string Index { get; set; }

        /// <summary>
        /// 总份数
        /// </summary>
        [Column(Name = "Total")]
        public int Total { get; set; }

        /// <summary>
        /// 已购买份额
        /// </summary>
        [Column(Name = "Buyed")]
        public int Buyed { get; set; }

        /// <summary>
        /// 發布時間
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 封单时间
        /// </summary>
        [Column(Name = "CloseAt")]
        public DateTime CloseAt { get; set; }

        /// <summary>
        ///  合買狀態 0：正常 1：已開獎，2、撤单
        /// </summary>
        [Column(Name = "Status")]
        public UnitedStatus Status { get; set; }

        /// <summary>
        /// 中獎金額
        /// </summary>
        [Column(Name = "Reward")]
        public Decimal Reward { get; set; }

        /// <summary>
        /// 公开选项 Public , Protected, Private
        /// </summary>
        [Column(Name = "Public")]
        public PublicType Public { get; set; }

        /// <summary>
        /// 佣金比例
        /// </summary>
        [Column(Name = "Commission")]
        public Decimal Commission { get; set; }

        /// <summary>
        /// 投注号码
        /// </summary>
        [Column(Name = "Number")]
        public string Number { get; set; }

        /// <summary>
        /// 保底份额
        /// </summary>
        [Column(Name = "Package")]
        public int Package { get; set; }

        /// <summary>
        /// 合买发起者的点位
        /// </summary>
        [Column(Name = "Rebate")]
        public int Rebate { get; set; }

    }
}
