using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SP.Studio.Web
{
    /// <summary>
    /// 民族
    /// </summary>
    public enum Nation
    {
        汉族,
        蒙古族, 回族, 藏族, 维吾尔族, 苗族, 彝族, 壮族, 布依族, 朝鲜族, 满族, 侗族, 瑶族, 白族, 土家族, 哈尼族, 哈萨克族,
        傣族, 黎族, 傈僳族, 佤族, 畲族, 高山族, 拉祜族, 水族, 东乡族, 纳西族, 景颇族, 柯尔克孜族, 土族, 达斡尔族, 仫佬族,
        羌族, 布朗族, 撒拉族, 毛南族, 仡佬族, 锡伯族, 阿昌族, 普米族, 塔吉克族, 怒族, 乌孜别克族, 俄罗斯族, 鄂温克族,
        德昂族, 保安族, 裕固族, 京族, 塔塔尔族, 独龙族, 鄂伦春族, 赫哲族, 门巴族, 珞巴族, 基诺族
    }

    /// <summary>
    /// 婚姻
    /// </summary>
    public enum Marry : byte
    {
        保密,
        未婚,
        已婚,
        离异,
        丧偶
    }

    /// <summary>
    /// 是否选择
    /// </summary>
    public enum BoolChoose
    {
        [Description("是")]
        Yes = 1,
        [Description("否")]
        No = 0
    }

    public enum Sex : byte
    {
        [Description("男")]
        Male,
        [Description("女")]
        Female
    }

    /// <summary>
    /// 年收入区间
    /// </summary>
    public enum Income : byte
    {
        [Description("保密")]
        Item0,
        [Description("小于3万")]
        Item1,
        [Description("3到5万")]
        Item2,
        [Description("5到8万")]
        Item3,
        [Description("8到12万")]
        Item4,
        [Description("12到20万")]
        Item5,
        [Description("20到30万")]
        Item6,
        [Description("30到50万")]
        Item7,
        [Description("50到100万")]
        Item8,
        [Description("100万以上")]
        Item9
    }

    /// <summary>
    /// 学历
    /// </summary>
    public enum Educational : byte
    {
        [Description("保密")]
        Secert,
        [Description("高中")]
        Senior,
        [Description("大专")]
        College,
        [Description("本科")]
        Undergraduate,
        [Description("硕士")]
        Master,
        [Description("博士")]
        Doctor
    }
}
