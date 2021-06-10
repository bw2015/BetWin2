using BW.GateWay.Lottery;
using BW.GateWay.Lottery.SmartGame;
using SP.Studio.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 系统支持的彩种
    /// </summary>
    public enum LotteryType : byte
    {
        /// <summary>
        /// 重庆时时彩
        /// </summary>
        [Description("重庆时时彩"), Lottery(Cate = LotteryCategory.X5, IndexLength = 3, StopTime = 30, Holiday = new HolidayType[] { HolidayType.ChineseNewYear }, Wechat = true)]
        ChungKing = 1,
        /// <summary>
        /// 江西时时彩
        /// </summary>
        [Description("江西时时彩"), Lottery(Cate = LotteryCategory.X5, IndexLength = 3, StopTime = 60, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        Kiangsi = 2,
        /// <summary>
        /// 分分彩 1分钟开一期
        /// </summary>
        [Description("幸运分分彩"), Lottery(Cate = LotteryCategory.X5, SiteLottery = true, IndexLength = 4)]
        Minute = 3,
        /// <summary>
        /// 五分彩 5分钟开一期
        /// </summary>
        [Description("幸运五分彩"), Lottery(Cate = LotteryCategory.X5, SiteLottery = true, IndexLength = 3)]
        Minute5 = 4,
        /// <summary>
        /// 体彩P3
        /// </summary>
        [Description("体彩P3"), Lottery(Cate = LotteryCategory.X3, StopTime = 900, Holiday = new HolidayType[] { HolidayType.ChineseNewYear }, StartTime = true)]
        ticaiP3 = 5,
        /// <summary>
        /// 福彩3D
        /// </summary>
        [Description("福彩3D"), Lottery(Cate = LotteryCategory.X3, StopTime = 900, Holiday = new HolidayType[] { HolidayType.ChineseNewYear }, StartTime = true)]
        fucai3D = 6,
        /// <summary>
        /// 香港六合彩
        /// </summary>
        [Description("香港六合彩"), Lottery(Cate = LotteryCategory.M6, IndexLength = 3, StopTime = 900, StartTime = true)]
        MarkSix = 7,
        /// <summary>
        /// 北京赛车PK10
        /// </summary>
        [Description("北京赛车PK10"), Lottery(Cate = LotteryCategory.P10, StopTime = 15, Holiday = new HolidayType[] { HolidayType.ChineseNewYear }, StartIndex = 729391, BeginDate = "2019-2-11", Wechat = true)]
        PK10 = 8,
        /// <summary>
        /// 新疆时时彩（东6区）
        /// </summary>
        [Description("新疆时时彩"), Lottery(Cate = LotteryCategory.X5, IndexLength = 2, StopTime = 100, Holiday = new HolidayType[] { HolidayType.ChineseNewYear }, TimeDifference = -2, Wechat = true)]
        Sinkiang = 9,
        /// <summary>
        /// 秒秒彩（无固定奖期)
        /// </summary>
        [Description("秒秒彩"), Lottery(Cate = LotteryCategory.X5, BuildIndex = true, SiteLottery = true)]
        Second = 10,
        /// <summary>
        /// 山西11选5
        /// </summary>
        [Description("山西11选5"), Lottery(Cate = LotteryCategory.X11, IndexLength = 2, StopTime = 60, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        SX11x5 = 11,
        /// <summary>
        /// 东京1.5分彩（东9区）
        /// </summary>
        [Description("东京1.5分"), Lottery(Cate = LotteryCategory.X5, IndexLength = 3, StopTime = 15)]
        Japan15 = 12,
        /// <summary>
        /// 幸运45秒
        /// </summary>
        [Description("幸运45秒"), Lottery(Cate = LotteryCategory.X5, IndexLength = 4, StopTime = 5, SiteLottery = true)]
        Second45 = 13,
        /// <summary>
        /// 北京5分彩
        /// </summary>
        [Description("北京快乐8"), Lottery(Cate = LotteryCategory.X5, StopTime = 60, Holiday = new HolidayType[] { HolidayType.ChineseNewYear }, StartIndex = 873256, BeginDate = "2018-2-22")]
        BJKL8 = 14,
        /// <summary>
        /// 幸运两分彩
        /// </summary>
        [Description("幸运两分彩"), Lottery(Cate = LotteryCategory.X5, SiteLottery = true, IndexLength = 3)]
        Minute2 = 15,
        /// <summary>
        /// 山东11选5
        /// </summary>
        [Description("山东11选5"), Lottery(Cate = LotteryCategory.X11, IndexLength = 2, StopTime = 60, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        SD11x5 = 16,
        /// <summary>
        /// 江苏11选5
        /// </summary>
        [Description("江苏11选5"), Lottery(Cate = LotteryCategory.X11, IndexLength = 2, StopTime = 60, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        JS11x5 = 17,
        /// <summary>
        /// 广东11选5
        /// </summary>
        [Description("广东11选5"), Lottery(Cate = LotteryCategory.X11, IndexLength = 2, StopTime = 60, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        GD11x5 = 18,
        /// <summary>
        /// 江苏快三
        /// </summary>
        [Description("江苏快三"), Lottery(Cate = LotteryCategory.K3, IndexLength = 3, StopTime = 300, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        JSK3 = 19,
        /// <summary>
        /// 安徽快三
        /// </summary>
        [Description("安徽快三"), Lottery(Cate = LotteryCategory.K3, IndexLength = 3, StopTime = 300, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        AHK3 = 20,
        /// <summary>
        /// 湖北快三
        /// </summary>
        [Description("湖北快三"), Lottery(Cate = LotteryCategory.K3, IndexLength = 3, StopTime = 300, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        HBK3 = 21,
        /// <summary>
        /// 河北快三
        /// </summary>
        [Description("河北快三"), Lottery(Cate = LotteryCategory.K3, IndexLength = 3, StopTime = 300, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        HEBEIK3 = 22,
        /// <summary>
        /// 广西快三
        /// </summary>
        [Description("广西快三"), Lottery(Cate = LotteryCategory.K3, IndexLength = 3, StopTime = 300, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        GXK3 = 23,
        /// <summary>
        /// 吉林快三
        /// </summary>
        [Description("吉林快三"), Lottery(Cate = LotteryCategory.K3, IndexLength = 3, StopTime = 300, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        JLK3 = 24,
        /// <summary>
        /// 上海快三
        /// </summary>
        [Description("上海快三"), Lottery(Cate = LotteryCategory.K3, IndexLength = 2, StopTime = 300, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        SHK3 = 25,
        /// <summary>
        /// 幸运十分彩
        /// </summary>
        [Description("幸运十分彩"), Lottery(Cate = LotteryCategory.X5, SiteLottery = true, IndexLength = 3)]
        Minute10 = 26,
        /// <summary>
        /// 台湾宾果    107032886
        /// </summary>
        [Description("台湾宾果"), Lottery(Cate = LotteryCategory.X5, StopTime = 30, StartIndex = 107032683, BeginDate = "2018-06-11")]
        TWBingo = 27,
        /// <summary>
        /// 新加坡快乐彩
        /// </summary>
        [Description("新加坡快乐彩"), Lottery(Cate = LotteryCategory.X5, StopTime = 5, StartIndex = 2772969, BeginDate = "2017-7-23", SiteLottery = true)]
        SGKeno = 28,
        /// <summary>
        /// 腾讯分分彩
        /// </summary>
        [Description("腾讯分分彩"), Lottery(Cate = LotteryCategory.X5, StopTime = 10, IndexLength = 4, Revoke = 300)]
        Tencent = 29,
        /// <summary>
        /// 韩国1.5分
        /// </summary>
        [Description("韩国1.5分"), Lottery(Cate = LotteryCategory.X5, StopTime = 20, StartIndex = 1868420, BeginDate = "2017-6-5")]
        KRKeno = 30,
        /// <summary>
        /// 加拿大3.5分
        /// </summary>
        [Description("加拿大3.5分"), Lottery(Cate = LotteryCategory.X5, StopTime = 30, StartIndex = 2167807, BeginDate = "2017-7-22", SiteLottery = true)]
        CAKeno = 31,
        /// <summary>
        /// 香港赛马
        /// </summary>
        [Description("香港赛马"), Lottery(Cate = LotteryCategory.P10, SiteLottery = true, StopTime = 30, StartIndex = 855813, BeginDate = "2017-3-17", Wechat = true, Delay = 10)]
        HKSM = 32,
        /// <summary>
        /// 幸运1.5分
        /// </summary>
        [Description("幸运1.5分"), Lottery(Cate = LotteryCategory.X5, SiteLottery = true, StopTime = 15, StartIndex = 1770761, BeginDate = "2017-2-13")]
        Minute15 = 33,
        /// <summary>
        /// 东京1.5分
        /// </summary>
        [Description("东京1.5分"), Lottery(Cate = LotteryCategory.X5, SiteLottery = true, StopTime = 10, IndexLength = 3)]
        Tokyo15 = 34,
        /// <summary>
        /// VR金星1.5分
        /// </summary>
        [Description("VR金星1.5分"), Lottery(Cate = LotteryCategory.X5, StopTime = 5, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VRVenus = 35,
        /// <summary>
        /// VR火星1.5分
        /// </summary>
        [Description("VR重庆时时彩"), Lottery(Cate = LotteryCategory.X5, StopTime = 10, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VRMars = 36,
        /// <summary>
        /// VR3分
        /// </summary>
        [Description("VR3分彩"), Lottery(Cate = LotteryCategory.X5, StopTime = 10, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VR3 = 37,
        /// <summary>
        /// VR赛车
        /// </summary>
        [Description("VR赛车"), Lottery(Cate = LotteryCategory.P10, StopTime = 15, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VRRacing = 38,
        /// <summary>
        /// VR快艇
        /// </summary>
        [Description("VR快艇"), Lottery(Cate = LotteryCategory.P10, StopTime = 15, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VRBoat = 39,
        /// <summary>
        /// 幸运飞艇(马其他飞艇）
        /// </summary>
        [Description("幸运飞艇"), Lottery(Cate = LotteryCategory.P10, StopTime = 15, IndexLength = 3, TimeDifference = -5, Wechat = true)]
        Boat = 40,
        /// <summary>
        /// 美国强力球45秒
        /// </summary>
        [Description("强力球45秒"), Lottery(Cate = LotteryCategory.X5, StopTime = 5, IndexLength = 4, SiteLottery = true)]
        USBall45 = 41,
        /// <summary>
        /// 美国强力球1.5分
        /// </summary>
        [Description("强力球1.5分"), Lottery(Cate = LotteryCategory.X5, StopTime = 15, IndexLength = 3, SiteLottery = true)]
        USBall15 = 42,
        /// <summary>
        /// VR百家乐
        /// </summary>
        [Description("VR百家乐"), Lottery(Cate = LotteryCategory.BA, StopTime = 5, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VRBaccarat = 43,
        /// <summary>
        /// 贵州11选5
        /// </summary>
        [Description("贵州11选5"), Lottery(Cate = LotteryCategory.X11, StopTime = 30, IndexLength = 2, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        GZ11x5 = 44,
        /// <summary>
        /// 贵州快三
        /// </summary>
        [Description("贵州快三"), Lottery(Cate = LotteryCategory.K3, StopTime = 300, IndexLength = 3, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        GZK3 = 45,
        /// <summary>
        /// 天津时时彩
        /// </summary>
        [Description("天津时时彩"), Lottery(Cate = LotteryCategory.X5, StopTime = 180, IndexLength = 3, Holiday = new HolidayType[] { HolidayType.ChineseNewYear })]
        Tianjin = 46,
        /// <summary>
        /// 小游戏合集
        /// </summary>
        [Description("小游戏"), Lottery(Cate = LotteryCategory.SmartGame, BuildIndex = true, SiteLottery = true, FullRebate = true)]
        SmallGame = 47,
        /// <summary>
        /// VR赛马 每天9点开始，90秒一期，21个小时，840期。9点到第二天6点
        /// </summary>
        [Description("VR赛马"), Lottery(Cate = LotteryCategory.P10, StopTime = 15, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VRRace = 48,
        /// <summary>
        /// VR游泳
        /// </summary>
        [Description("VR游泳"), Lottery(Cate = LotteryCategory.P10, StopTime = 15, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VRSwim = 49,
        /// <summary>
        /// VR自行车
        /// </summary>
        [Description("VR自行车"), Lottery(Cate = LotteryCategory.X11, StopTime = 15, IndexLength = 3, TimeDifference = -6, NoTest = true)]
        VRBike = 50,
        /// <summary>
        /// 美国快三
        /// </summary>
        [Description("美国快三"), Lottery(Cate = LotteryCategory.K3, SiteLottery = true, StopTime = 5, IndexLength = 4, TimeDifference = -12)]
        USAK3 = 51,
        /// <summary>
        /// 冰岛快三
        /// Cate = LotteryCategory.X5, SiteLottery = true, StopTime = 10, IndexLength = 3
        /// </summary>
        [Description("冰岛快三"), Lottery(Cate = LotteryCategory.K3, SiteLottery = true, StopTime = 5, IndexLength = 4, TimeDifference = -10)]
        ICEK3 = 52,
        /// <summary>
        /// 北京快三
        /// </summary>
        [Description("北京快三"), Lottery(Cate = LotteryCategory.K3, SiteLottery = false, StopTime = 300, IndexLength = 2)]
        BJK3 = 53,
        /// <summary>
        /// PC28
        /// </summary>
        [Description("PC蛋蛋"), Lottery(Cate = LotteryCategory.P28, SiteLottery = false, StopTime = 60, StartIndex = 937695, BeginDate = "2019-2-24")]
        PC28 = 54,
        /// <summary>
        /// 系统内置彩种
        /// </summary>
        [Description("极速赛车"), Lottery(Cate = LotteryCategory.P10, SiteLottery = true, StopTime = 5, IndexLength = 4)]
        FastRace = 55,
        /// <summary>
        /// 河内五分彩
        /// </summary>
        [Description("河内五分彩"), Lottery(Cate = LotteryCategory.X5, StopTime = 30, IndexLength = 3)]
        Hanoi5 = 56,
        /// <summary>
        /// 河南快三
        /// </summary>
        [Description("河南快三"), Lottery(Cate = LotteryCategory.K3, StopTime = 300, IndexLength = 2)]
        HenanK3 = 57,
        /// <summary>
        /// 河内分分彩
        /// </summary>
        [Description("河内分分彩"), Lottery(Cate = LotteryCategory.X5, StopTime = 5, IndexLength = 4)]
        Hanoi1 = 58,
        /// <summary>
        /// 老重庆时时彩
        /// </summary>
        [Description("老重庆时时彩"), Lottery(Cate = LotteryCategory.X5, StopTime = 30, IndexLength = 3, SiteLottery = true)]
        ChungKing2 = 59
    }

    /// <summary>
    /// 彩票分类
    /// </summary>
    public enum LotteryCategory
    {
        /// <summary>
        /// 时时彩类型
        /// </summary>
        [Description("时时彩"), LotteryCategory(IsRepeat = true, Length = 5, Ball = "0,1,2,3,4,5,6,7,8,9")]
        X5 = 1,
        /// <summary>
        /// 11选5
        /// </summary>
        [Description("11选5"), LotteryCategory(IsRepeat = false, Length = 5, Ball = "01,02,03,04,05,06,07,08,09,10,11")]
        X11 = 2,
        /// <summary>
        /// 体彩P3、福彩3D
        /// </summary>
        [Description("3D"), LotteryCategory(IsRepeat = true, Length = 3, Ball = "0,1,2,3,4,5,6,7,8,9")]
        X3 = 3,
        /// <summary>
        /// 快三
        /// </summary>
        [Description("快三"), LotteryCategory(IsRepeat = true, Length = 3, Ball = "1,2,3,4,5,6")]
        K3 = 4,
        /// <summary>
        /// 北京PK10
        /// </summary>
        [Description("PK10"), LotteryCategory(IsRepeat = true, Length = 10, Ball = "01,02,03,04,05,06,07,08,09,10")]
        P10 = 5,
        /// <summary>
        /// 香港六合彩
        /// </summary>
        [Description("六合彩"), LotteryCategory(IsRepeat = false, Length = 7, Ball = "01,02,03,04,05,06,07,08,09,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49")]
        M6 = 6,
        /// <summary>
        /// 百家乐
        /// </summary>
        [Description("百家乐"), LotteryCategory(IsRepeat = true, Length = 4, Ball = "0,1,2,3,4,5,6,7,8,9")]
        BA = 7,
        /// <summary>
        /// 小游戏
        /// </summary>
        [Description("小游戏"), LotteryCategory(Open = true)]
        SmartGame = 8,
        /// <summary>
        /// PC28
        /// </summary>
        [Description("幸运28"), LotteryCategory(IsRepeat = true, Length = 4, Ball = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27")]
        P28 = 9
    }

    /// <summary>
    /// 资金模式（毫为最小单位）
    /// </summary>
    public enum LotteryMode : int
    {
        元 = 20000,
        一元 = 10000,
        五角 = 5000,
        角 = 2000,
        一角 = 1000,
        五分 = 500,
        分 = 200,
        一分 = 100,
        厘 = 20,
        毫 = 2
    }


    /// <summary>
    /// 获取彩期的类型
    /// </summary>
    internal enum ResultIndexType
    {
        /// <summary>
        /// 当前彩期
        /// </summary>
        Index,
        /// <summary>
        /// 当前可投注的彩期
        /// </summary>
        Bet
    }

    /// <summary>
    /// 公共假期（不开奖的日期）
    /// </summary>
    public enum HolidayType
    {
        /// <summary>
        /// 中国春节
        /// </summary>
        ChineseNewYear
    }

    /// <summary>
    /// 彩票的设置
    /// </summary>
    public class LotteryAttribute : Attribute
    {
        /// <summary>
        /// 是否自主创建彩期
        /// </summary>
        public bool BuildIndex { get; set; }

        /// <summary>
        /// 奖期长度
        /// </summary>
        public int IndexLength { get; set; }

        /// <summary>
        /// 自有彩种
        /// </summary>
        public bool SiteLottery { get; set; }

        /// <summary>
        /// 所属分类
        /// </summary>
        public LotteryCategory Cate { get; set; }

        /// <summary>
        /// 封单时间（秒）
        /// </summary>
        public int StopTime { get; set; }

        /// <summary>
        /// 不开奖的假期
        /// </summary>
        public HolidayType[] Holiday { get; set; }

        /// <summary>
        /// 序号奖期的开始日期前的最后一期
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// 序号奖期的开始日期
        /// </summary>
        public string BeginDate { get; set; }

        /// <summary>
        /// 是否支持微信投注
        /// </summary>
        public bool Wechat { get; set; }

        /// <summary>
        /// 延迟开奖（秒）
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// 与北京时间的时差
        /// </summary>
        public int TimeDifference { get; set; }

        /// <summary>
        /// 禁止虚拟号投注
        /// </summary>
        public bool NoTest { get; set; }

        public DateTime StartDate
        {
            get
            {
                if (string.IsNullOrEmpty(BeginDate)) return DateTime.MinValue;
                return DateTime.Parse(this.BeginDate);
            }
        }

        /// <summary>
        /// 自定义的彩期数据
        /// </summary>
        public bool StartTime { get; set; }

        /// <summary>
        /// 超过指定时间没有开奖则自动撤单（单位：秒）
        /// </summary>
        public int Revoke { get; set; }

        /// <summary>
        /// 按照2000奖金组派发奖金，无返点
        /// </summary>
        public bool FullRebate { get; set; }

        /// <summary>
        /// 假期日期
        /// </summary>
        public IEnumerable<DateTime> GetHolidayDate()
        {
            if (Holiday == null) yield break;
            foreach (DateTime[] date in Utils.Holiday.Where(t => this.Holiday.Contains(t.Key)).Select(t => t.Value))
            {
                foreach (DateTime time in date)
                    yield return time;
            }
        }

        /// <summary>
        /// 分类信息
        /// </summary>
        /// <returns></returns>
        public LotteryCategoryAttribute CategoryInfo
        {
            get
            {
                return this.Cate.GetAttribute<LotteryCategoryAttribute>();
            }
        }

        /// <summary>
        /// 创建一个随机号码
        /// </summary>
        /// <returns></returns>
        public string CreateNumber(IPlayer player = null)
        {
            string number = null;
            List<string> code = new List<string>();
            switch (this.Cate)
            {
                case LotteryCategory.X5:
                    while (code.Count < 5)
                    {
                        foreach (Match match in Regex.Matches(Guid.NewGuid().ToString(), @"\d"))
                        {
                            code.Add(match.Value);
                            if (code.Count == 5) break;
                        }
                    }
                    number = string.Join(",", code);
                    break;
                case LotteryCategory.K3:
                    while (code.Count < 3)
                    {
                        foreach (Match match in Regex.Matches(Guid.NewGuid().ToString(), @"\d"))
                        {
                            if (this.CategoryInfo.Ball.Contains(match.Value)) code.Add(match.Value);
                            if (code.Count == 3) break;
                        }
                    }
                    number = string.Join(",", code);
                    break;
                case LotteryCategory.P10:
                    number = string.Join(",", this.CategoryInfo.Ball.Split(',').OrderBy(t => Guid.NewGuid()));
                    break;
                // 小游戏 需根据玩法产生开奖结果
                case LotteryCategory.SmartGame:
                    if (player != null)
                    {
                        number = ((IGamePlayer)player).CreateRandomNumber();
                    }
                    break;
            }
            return number;
        }

        /// <summary>
        /// 默认转化成为分类信息
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static implicit operator LotteryCategoryAttribute(LotteryAttribute lottery)
        {
            return lottery.CategoryInfo;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Category\":\"{0}\",", this.Cate)
                .AppendFormat("\"Name\":\"{0}\",", this.Cate.GetDescription())
                .AppendFormat("\"SiteLottery\":{0}", this.SiteLottery ? 1 : 0)
                .Append("}");

            return sb.ToString();
        }

    }

    /// <summary>
    /// 自定义方法的委托
    /// </summary>
    /// <param name="type"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    internal delegate string LotteryFunction(ResultIndexType type, out int time);

    /// <summary>
    /// 自定义的获取开奖时间的委托
    /// </summary>
    /// <param name="index">奖期</param>
    /// <returns>预设的开奖时间</returns>
    internal delegate DateTime LotteryTimeFunction(string index);

    /// <summary>
    /// 分类属性
    /// </summary>
    public class LotteryCategoryAttribute : Attribute
    {
        /// <summary>
        /// 开奖球范围
        /// </summary>
        public string Ball { get; set; }

        /// <summary>
        /// 开奖号码长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 是否允许号码重复
        /// </summary>
        public bool IsRepeat { get; set; }

        /// <summary>
        /// 号码的投注范围
        /// </summary>
        public string[] Number
        {
            get
            {
                if (string.IsNullOrEmpty(this.Ball)) return null;
                return this.Ball.Split(',');
            }
        }


        /// <summary>
        /// 是否是即开型
        /// </summary>
        public bool Open { get; set; }

        /// <summary>
        /// 判断开奖号码是否符合规范
        /// </summary>
        /// <param name="result">用逗号隔开的数字</param>
        /// <returns></returns>
        public bool IsMatch(string result)
        {
            // 如果没有固定号码则不判断
            if (string.IsNullOrEmpty(this.Ball)) return true;

            string[] resultNumber = result.Split(',');

            //#1 如果不允许重复则过滤掉重复字符
            if (!this.IsRepeat) resultNumber = resultNumber.Distinct().ToArray();

            //#2 检查长度是否足够
            if (resultNumber.Length != this.Length) return false;

            //#3 检查号码在开奖范围内
            if (resultNumber.Where(t => !this.Number.Contains(t)).Count() != 0) return false;

            return true;
        }

        /// <summary>
        /// 判断投注号码是否符合规范
        /// </summary>
        /// <param name="number">用逗号隔开的投注号码，不允许重复</param>
        /// <param name="minLength">最低长度</param>
        /// <param name="maxLength">最多长度</param>
        /// <returns></returns>
        public bool IsMatch(string number, int minLength, int maxLength)
        {
            string[] resultNumber = number.Split(',');

            //#1 判断号码是否存在重复
            if (resultNumber.Distinct().Count() != resultNumber.Length) return false;

            //#2 判断号码是否在允许长度范围内
            if (resultNumber.Length < minLength || resultNumber.Length > maxLength) return false;

            //#3 检查号码是否在投注号码范围内
            if (resultNumber.Where(t => !this.Number.Contains(t)).Count() != 0) return false;

            return true;
        }


        /// <summary>
        /// 从采集接口采集过来的数据转化成为系统需要的数据格式
        /// 无逗号分隔修改成为有逗号分隔
        /// </summary>
        /// <returns></returns>
        public string GetResultNumber(string number)
        {
            string[] list = new string[this.Length];
            Regex regex = new Regex(string.Join("|", this.Number));
            int index = 0;
            foreach (Match match in regex.Matches(number))
            {
                list[index] = match.Value;
                index++;
                if (index == this.Length) break;
            }
            return string.Join(",", list);
        }
    }

    /// <summary>
    /// 彩种工具
    /// </summary>
    public static class LotteryUtils
    {
        /// <summary>
        /// 根据分类获取该分类下的全部彩种
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static LotteryType[] GetLottery(this LotteryCategory category)
        {
            List<LotteryType> list = new List<LotteryType>();
            foreach (byte value in Enum.GetValues(typeof(LotteryType)))
            {
                if (((LotteryType)value).GetAttribute<LotteryAttribute>().Cate == category)
                {
                    list.Add((LotteryType)value);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 获取彩种的分类属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static LotteryAttribute GetCategory(this LotteryType type)
        {
            return type.GetAttribute<LotteryAttribute>();
        }
    }



}
