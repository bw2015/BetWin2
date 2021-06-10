using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Resources;
using System.Net;
using System.Xml.Linq;

using SP.Studio.Core;
using SP.Studio.Xml;
using SP.Studio.Web;

using BW.Common.Lottery;
using BW.Common.Lottery.Limited;
using BW.Common.Users;
using BW.Framework;

namespace BW
{
    /// <summary>
    /// 全局通用的工具类
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 静态构造 初始化系统相关的全局缓存
        /// </summary>
        static Utils()
        {
            XElement holiday = XElement.Parse((string)new ResourceManager(typeof(BW.Resources.Res)).GetObject("Holiday"));
            foreach (XElement item in holiday.Elements())
            {
                if (Enum.GetNames(typeof(HolidayType)).Contains(item.Name.ToString()))
                {
                    Holiday.Add(item.Name.ToString().ToEnum<HolidayType>(),
                        item.Elements().Select(t => t.GetAttributeValue("date", DateTime.MinValue)).ToArray());
                }
            }
        }

        /// <summary>
        /// 资金模式的标准单位（1元的拆分单位）
        /// </summary>
        internal const int LOTTERYMODE_UNIT = 10000;

        /// <summary>
        /// 公共假期的时间
        /// </summary>
        internal readonly static Dictionary<HolidayType, DateTime[]> Holiday = new Dictionary<HolidayType, DateTime[]>();

        internal static void ShowError(HttpContext context, HttpStatusCode statusCode, string error = null)
        {
            if (context.Request.Url.Authority.StartsWith("localhost")) return;

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "text/html";
            string status = Regex.Replace(statusCode.ToString(), "[A-Z]", t =>
            {
                return " " + t.Value;
            });

            if (string.IsNullOrEmpty(error))
            {
                error = status;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<!-- {0} -->", error);
            sb.AppendFormat("<html><head><title>{0} {1}</title><body><h1><center>{0} {1}</center></h1><hr />", (int)statusCode, status);
            sb.AppendFormat("<center>{0}/{1}</center>", typeof(Utils).Assembly.GetName().Name.ToLower(), typeof(Utils).Assembly.GetName().Version);
            sb.Append("</body></html>");
            context.Response.Write(sb);
            context.Response.End();
        }

        /// <summary>
        /// 获取用户在某个彩种中的最高奖金
        /// </summary>
        /// <param name="siteRebate">站点最高奖金</param>
        /// <param name="userRebate">用户奖金</param>
        /// <param name="lotteryRebate">彩种奖金</param>
        /// <returns></returns>
        internal static int GetRebate(int siteRebate, int userRebate, int lotteryRebate)
        {
            return (int)((double)userRebate * ((double)lotteryRebate / (double)siteRebate));
        }

        /// <summary>
        /// 获取用户实际可得的奖金
        /// </summary>
        /// <param name="reward"></param>
        /// <param name="userRebate"></param>
        /// <returns></returns>
        internal static decimal GetReward(decimal reward, int userRebate)
        {
            return reward * (decimal)userRebate / 2000M;
        }

        /// <summary>
        /// 获取聊天的类型和用户ID
        /// </summary>
        /// <param name="key"></param>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        internal static ChatLog.ChatType GetChatType(string key, out int id1, out int id2)
        {
            id1 = id2 = 0;
            Regex regex = new Regex(@"^(\d)-(\d+)-(\d+)$");
            if (!regex.IsMatch(key)) return ChatLog.ChatType.None;
            GroupCollection group = regex.Match(key).Groups;
            id1 = int.Parse(group[2].Value);
            id2 = int.Parse(group[3].Value);
            try
            {
                return (ChatLog.ChatType)byte.Parse(group[1].Value);
            }
            catch
            {
                throw new Exception(group[0].Value);
            }
        }

        #region =========== 彩期相关方法  ==============

        /// <summary>
        /// 获取当前的日期（自动排除节假日）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private static DateTime GetDate(LotteryType type, DateTime date, out bool isHoliday, int step = 1)
        {
            isHoliday = false;
            IEnumerable<DateTime> holiday = type.GetCategory().GetHolidayDate();
            if (holiday == null) return date;
            while (holiday.Contains(date))
            {
                isHoliday = true;
                date = date.AddDays(step);
            }
            return date;
        }

        /// <summary>
        /// 是否是节假日
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsHoliday(LotteryType type, DateTime date)
        {
            IEnumerable<DateTime> holiday = type.GetCategory().GetHolidayDate();
            if (holiday == null) return false;
            return holiday.Contains(date);
        }

        /// <summary>
        ///  一天总共的秒数
        /// </summary>
        private const int TOTALSECOND = 86400;

        /// <summary>
        /// 获取当前的彩期
        /// </summary>
        /// <param name="type"></param>
        /// <param name="time">下一期的开奖时间（秒）</param>
        /// <returns></returns>
        internal static string GetLotteryIndex(LotteryType type, out int time)
        {
            DateTime now = DateTime.Now.AddHours(type.GetCategory().TimeDifference);

            if (type.GetCategory().StartIndex != 0)
            {
                return getIndexByNumber(type, ResultIndexType.Index, out time);
            }
            if (type.GetCategory().StartTime)
            {
                return getIndexByStartTime(type, ResultIndexType.Index, out time);
            }
            time = 0;
            if (!SysSetting.GetSetting().LotteryTimeTemplate.ContainsKey(type)) return null;
            bool isHoliday;

            DateTime date = GetDate(type, now.Date, out isHoliday, -1);
            int openTime = isHoliday ? TOTALSECOND : now.GetSecond();

            List<TimeTemplate> list = SysSetting.GetSetting().LotteryTimeTemplate[type];
            TimeTemplate template = list.Where(t => t.Seconds <= openTime).LastOrDefault();
            if (template == null)
            {
                date = date.AddDays(-1);
                template = list.LastOrDefault();
            }

            DateTime nextDate = GetDate(type, date.Date, out isHoliday);
            TimeTemplate nextTemplate = isHoliday ? list.First() : list.Where(t => t.Seconds > template.Seconds).FirstOrDefault();
            if (nextTemplate == null)
            {
                nextDate = nextDate.AddDays(1);
                nextTemplate = list.FirstOrDefault();
            }

            time = (int)((TimeSpan)(nextDate.AddSeconds(nextTemplate.Seconds) - now)).TotalSeconds;

            if (template.Index == 0) return date.ToString("yyyyMMdd");

            return string.Concat(date.ToString("yyyyMMdd"), "-", template.LotteryIndex);
        }

        /// <summary>
        /// 当前可投注期
        /// </summary>
        /// <param name="type">彩种</param>
        /// <param name="time">剩余时间（秒）</param>
        /// <param name="datetime">当前的时间</param>
        /// <returns></returns>
        internal static string GetLotteryBetIndex(LotteryType type, out int time, DateTime? datetime = null)
        {
            if (datetime == null) datetime = DateTime.Now;
            DateTime now = datetime.Value.AddHours(type.GetCategory().TimeDifference);

            if (type.GetCategory().StartIndex != 0)
            {
                return getIndexByNumber(type, ResultIndexType.Bet, out time);
            }
            if (type.GetCategory().StartTime)
            {
                return getIndexByStartTime(type, ResultIndexType.Bet, out time);
            }

            time = 0;
            if (!SysSetting.GetSetting().LotteryTimeTemplate.ContainsKey(type)) return null;
            List<TimeTemplate> list = SysSetting.GetSetting().LotteryTimeTemplate[type];
            bool isHoliday;
            int stopTime = type.GetCategory().StopTime;
            DateTime date = GetDate(type, now.Date, out isHoliday);

            TimeTemplate template;
            if (isHoliday)
            {
                template = list.First();
            }
            else
            {
                int timeNow = (int)((TimeSpan)(now - date)).TotalSeconds + stopTime;
                template = list.Where(t => t.Seconds > timeNow).FirstOrDefault();
                if (template == null)
                {
                    template = list.FirstOrDefault();
                    date = date.AddDays(1);
                }
            }

            time = (int)((TimeSpan)(date.AddSeconds(template.Seconds) - now)).TotalSeconds - stopTime;

            return string.Concat(date.ToString("yyyyMMdd"), "-", template.LotteryIndex);
        }

        /// <summary>
        /// 获取官方设定的开奖时间
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static DateTime GetLotteryTime(LotteryType type, string index)
        {
            if (type.GetCategory().StartIndex != 0)
            {
                return getTimeByNumber(type, index);
            }
            if (type.GetCategory().StartTime)
            {
                return getTimeByStartTime(type, index);
            }

            Regex regex = new Regex(@"^(?<Year>\d{4})(?<Month>\d{2})(?<Date>\d{2})-(?<Index>\d{2,4})$");
            if (!regex.IsMatch(index)) return DateTime.MinValue;

            string dateString = string.Concat(regex.Match(index).Groups["Year"].Value, "-", regex.Match(index).Groups["Month"].Value, "-", regex.Match(index).Groups["Date"].Value);
            DateTime date = DateTime.Parse(dateString);
            int indexNo = int.Parse(regex.Match(index).Groups["Index"].Value);

            int second = SysSetting.GetSetting().LotteryTimeTemplate[type].Where(t => t.Index == indexNo).FirstOrDefault().Seconds;

            date = date.AddSeconds(second);

            return date;
        }

        /// <summary>
        /// 获取当前期之后的第N期的期号、开奖时间
        /// </summary>
        /// <param name="type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        internal static IEnumerable<ResultNumber> GetLotteryIndex(LotteryType type, int count)
        {
            DateTime datetimeNow = DateTime.Now.AddHours(type.GetCategory().TimeDifference);

            DateTime date = datetimeNow.Date;
            if (!SysSetting.GetSetting().LotteryTimeTemplate.ContainsKey(type)) yield break;
            List<TimeTemplate> list = SysSetting.GetSetting().LotteryTimeTemplate[type];
            int now = (int)datetimeNow.TimeOfDay.TotalSeconds;
            LotteryAttribute info = type.GetCategory();

            for (int day = 0; day < count / SysSetting.GetSetting().LotteryTimeTemplate[type].Count + 1; day++)
            {
                date = datetimeNow.Date.AddDays(day);
                int startIndex = 0;
                if (info.StartIndex != 0)
                {
                    startIndex = info.StartIndex + ((int)(date - info.StartDate).TotalDays) * SysSetting.GetSetting().LotteryTimeTemplate[type].Count;
                }
                foreach (TimeTemplate time in list)
                {
                    if (day > 0 || time.Seconds - type.GetCategory().StopTime > now)
                    {
                        yield return new ResultNumber()
                        {
                            ResultAt = date.AddSeconds(time.Seconds).AddHours(type.GetCategory().TimeDifference * -1),
                            Index = info.StartIndex != 0 ? (startIndex + time.Index).ToString() : string.Concat(date.ToString("yyyyMMdd"), "-", time.LotteryIndex),
                            Type = type
                        };
                        count--;
                        if (count == 0) yield break;
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前期之后N期的投注开始时间
        /// </summary>
        /// <param name="type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        internal static IEnumerable<ResultNumber> GetLotteryIndexStartTime(LotteryType type, int count)
        {
            DateTime datetimeNow = DateTime.Now.AddHours(type.GetCategory().TimeDifference);

            DateTime date = datetimeNow.Date;
            if (!SysSetting.GetSetting().LotteryTimeTemplate.ContainsKey(type)) yield break;
            List<TimeTemplate> list = SysSetting.GetSetting().LotteryTimeTemplate[type];
            int now = (int)datetimeNow.TimeOfDay.TotalSeconds;
            DateTime startAt = datetimeNow;
            LotteryAttribute info = type.GetCategory();

            for (int day = 0; day < count / SysSetting.GetSetting().LotteryTimeTemplate.Count + 1; day++)
            {
                date = datetimeNow.Date.AddDays(day);
                int startIndex = 0;
                if (info.StartIndex != 0)
                {
                    startIndex = info.StartIndex + ((int)(date - info.StartDate).TotalDays) * SysSetting.GetSetting().LotteryTimeTemplate[type].Count;
                }

                foreach (TimeTemplate time in list)
                {
                    if (day > 0 || time.Seconds - type.GetCategory().StopTime > now)
                    {
                        yield return new ResultNumber()
                        {
                            ResultAt = startAt,
                            Index = info.StartIndex != 0 ? (startIndex + time.Index).ToString() : string.Concat(date.ToString("yyyyMMdd"), "-", time.LotteryIndex),
                            Type = type
                        };
                        count--;
                        startAt = date.AddSeconds(time.Seconds).AddHours(type.GetCategory().TimeDifference * -1);
                        if (count == 0) yield break;
                    }
                }
            }
        }

        /// <summary>
        /// 当前是否是可投注期(未开奖不能投注）
        /// </summary>
        /// <param name="game"></param>
        /// <param name="betIndex">当前可投注期，如果不可投注返回null</param>
        /// <param name="siteId">如果是系统彩种则需要指定站点</param>
        /// <returns></returns>
        internal static bool IsBet(LotteryType game, out string betIndex, int siteId)
        {
            // 封单时间
            int stopTime = game.GetCategory().StopTime;
            // 距离开奖的时间
            int openTime = 0;
            // 剩余投注时间
            int betTime = 0;

            string openIndex = Utils.GetLotteryIndex(game, out openTime);
            string openNumber = SysSetting.GetSetting().GetResultNumber(game, openIndex, siteId);
            betIndex = null;

            // 当前期没有开奖
            if (string.IsNullOrEmpty(openNumber)) return false;

            // 开奖剩余时间小于封单时间
            if (openTime <= stopTime || openTime > 600) return false;

            betIndex = Utils.GetLotteryBetIndex(game, out betTime);
            return true;
        }

        #endregion

        #region =========== 自定义彩期方法  ===========

        /// <summary>
        /// 获取顺序期号的通用方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="indexType"></param>
        /// <param name="time">剩余时间（秒）</param>
        /// <returns></returns>
        private static string getIndexByNumber(LotteryType type, ResultIndexType indexType, out int time)
        {
            time = 0;
            string index = null;

            if (IsHoliday(type, DateTime.Now.Date)) return index;

            int startIndex = type.GetCategory().StartIndex;
            DateTime startDate = type.GetCategory().StartDate;

            if (startIndex == 0) return index;

            if (!SysSetting.GetSetting().LotteryTimeTemplate.ContainsKey(type)) return index;
            List<TimeTemplate> list = SysSetting.GetSetting().LotteryTimeTemplate[type];
            if (list.Count == 0) return index;

            TimeTemplate template;
            DateTime datetime = DateTime.Now;
            DateTime date = datetime.Date;
            template = list.FirstOrDefault();
            // 当前时间（秒）
            int now = DateTime.Now.GetSecond();
            int dayCount = ((TimeSpan)(DateTime.Now.Date - startDate)).Days * list.Count;
            // 当天的起始期号
            startIndex += dayCount;

            switch (indexType)
            {
                //当前开奖彩期
                case ResultIndexType.Index:
                    // 当前期等于昨日最后一期加小于当前时间的期数
                    template = list.Where(t => t.Seconds < now).LastOrDefault();
                    index = (startIndex + (template == null ? 0 : template.Index)).ToString();
                    // 下一期开奖的时间
                    template = list.Where(t => t.Seconds >= now).FirstOrDefault();
                    if (template == null)
                    {
                        date = date.AddDays(1);
                        template = list.FirstOrDefault();
                    }
                    time = (int)((TimeSpan)(date.AddSeconds(template.Seconds) - datetime)).TotalSeconds;
                    break;
                // 可投注期
                case ResultIndexType.Bet:
                    // 当前时间加上封单时间
                    now += type.GetCategory().StopTime;
                    // 获取大于当前时间的最小一期
                    template = list.Where(t => t.Seconds >= now).FirstOrDefault();
                    if (template != null)
                    {
                        // 如果当天还可以投注 则加上可投注期的序号
                        index = (startIndex + template.Index).ToString();
                    }
                    else
                    {
                        // 如果当天已经结束 则获取下一天的期号
                        template = list.FirstOrDefault();
                        date = date.AddDays(1);
                        index = (startIndex + list.Max(t => t.Index) + 1).ToString();
                    }
                    time = (int)((TimeSpan)(date.AddSeconds(template.Seconds) - datetime.AddSeconds(type.GetCategory().StopTime))).TotalSeconds;
                    break;
            }

            return index;
        }

        /// <summary>
        /// 自定义彩期的彩种
        /// </summary>
        /// <param name="type"></param>
        /// <param name="indexType"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private static string getIndexByStartTime(LotteryType type, ResultIndexType indexType, out int time)
        {
            time = 0;
            string index = null;
            if (!SysSetting.GetSetting().LotteryStartTime.ContainsKey(type)) return index;
            List<StartTime> list = SysSetting.GetSetting().LotteryStartTime[type];
            if (list.Count == 0) return index;

            StartTime template, nextTempalte;

            switch (indexType)
            {
                case ResultIndexType.Index:
                    template = list.Where(t => t.StartAt < DateTime.Now).LastOrDefault();
                    nextTempalte = list.Where(t => t.StartAt > DateTime.Now).FirstOrDefault();
                    if (template == null || nextTempalte == null) return index;
                    index = template.Index;
                    time = (int)(nextTempalte.StartAt - DateTime.Now).TotalSeconds;
                    break;
                case ResultIndexType.Bet:
                    int stopTime = type.GetCategory().StopTime;
                    template = list.Where(t => t.StartAt > DateTime.Now.AddSeconds(stopTime)).FirstOrDefault();
                    if (template == null) return index;
                    index = template.Index;
                    time = (int)(template.StartAt - DateTime.Now.AddSeconds(stopTime)).TotalSeconds;
                    break;
            }
            return index;
        }

        private const int x3StartIndex = 38;
        private static readonly DateTime x3StartDate = new DateTime(2016, 2, 14);
        /// <summary>
        /// 体彩P3 和 福彩3D的自定义彩期功能
        /// </summary>
        /// <param name="indexType"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        internal static string GetX3Index(ResultIndexType indexType, out int time)
        {
            time = 0;
            string index = null;
            LotteryType type = LotteryType.ticaiP3;

            if (!SysSetting.GetSetting().LotteryTimeTemplate.ContainsKey(type)) return index;
            List<TimeTemplate> list = SysSetting.GetSetting().LotteryTimeTemplate[type];

            if (list.Count == 0) return index;

            TimeTemplate template;
            DateTime date = DateTime.Now.Date;
            template = list.FirstOrDefault();
            // 当前的时间
            int openTime = DateTime.Now.GetSecond();

            switch (indexType)
            {
                case ResultIndexType.Index:
                    if (openTime < template.Seconds)
                    {
                        date = date.AddDays(-1);
                    }
                    index = date.Year.ToString() + ((date - x3StartDate).Days + x3StartIndex).ToString().PadLeft(3, '0');
                    time = (int)(date.AddSeconds(template.Seconds) - DateTime.Now).TotalSeconds;
                    break;
                case ResultIndexType.Bet:
                    int stopTime = type.GetCategory().StopTime;
                    int timeNow = openTime + stopTime;
                    if (timeNow > template.Seconds)
                    {
                        date = date.AddDays(1);
                    }
                    time = (int)(date.AddSeconds(template.Seconds) - DateTime.Now).TotalSeconds - stopTime;
                    index = date.Year.ToString() + ((date - x3StartDate).Days + x3StartIndex).ToString().PadLeft(3, '0');
                    break;
            }
            return index;
        }

        /// <summary>
        /// 序号类型彩种获取开奖时间
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static DateTime getTimeByNumber(LotteryType type, string index)
        {
            int indexNo;
            if (!int.TryParse(index, out indexNo)) return DateTime.MinValue;
            int count = SysSetting.GetSetting().LotteryTimeTemplate[type].Count;
            int startIndex = type.GetCategory().StartIndex;
            DateTime startDate = type.GetCategory().StartDate;
            if (indexNo < startIndex) return DateTime.MinValue;
            int day = (indexNo - startIndex) / count;
            int dayIndex = (indexNo - startIndex) % count;
            int second = 0;
            if (dayIndex == 0)
            {
                day--;
                second = SysSetting.GetSetting().LotteryTimeTemplate[type].LastOrDefault().Seconds;
            }
            else
            {
                second = SysSetting.GetSetting().LotteryTimeTemplate[type].Where(t => t.Index == dayIndex).FirstOrDefault().Seconds;
            }
            return startDate.AddDays(day).AddSeconds(second);
        }

        /// <summary>
        /// 获取自定义彩期的彩种的开奖时间
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static DateTime getTimeByStartTime(LotteryType type, string index)
        {
            if (!SysSetting.GetSetting().LotteryStartTime.ContainsKey(type)) return DateTime.MinValue;
            List<StartTime> list = SysSetting.GetSetting().LotteryStartTime[type];

            StartTime startTime = list.Find(t => t.Index == index);
            return startTime == null ? DateTime.MinValue : startTime.StartAt;
        }

        /// <summary>
        /// 获取体彩P3 和 福彩3D的开奖时间
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static DateTime GetX3Index(string index)
        {
            Regex regex = new Regex(@"^(?<Year>\d{4})(?<Index>\d{3})$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(index)) return DateTime.MinValue;
            LotteryType type = LotteryType.ticaiP3;

            if (!SysSetting.GetSetting().LotteryTimeTemplate.ContainsKey(type)) return DateTime.MinValue;
            List<TimeTemplate> list = SysSetting.GetSetting().LotteryTimeTemplate[type];

            if (list.Count == 0) return DateTime.MinValue;
            TimeTemplate template = list.FirstOrDefault();

            int indexNo = int.Parse(regex.Match(index).Groups["Index"].Value);
            return x3StartDate.AddDays(indexNo - x3StartIndex).AddSeconds(template.Seconds);
        }


        #endregion

        #region ============   资金相关方法  ===========

        /// <summary>
        /// 获取资金类型所属的分类
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MoneyLog.MoneyCategoryType GetCategory(this MoneyLog.MoneyType type)
        {
            if (!Enum.IsDefined(typeof(MoneyLog.MoneyType), type)) return MoneyLog.MoneyCategoryType.Other;
            MoneyLog.MoneyCategoryAttribute category = type.GetAttribute<MoneyLog.MoneyCategoryAttribute>();
            if (category == null) return MoneyLog.MoneyCategoryType.Other;
            return category.Type;
        }

        /// <summary>
        /// 获取分类下的所有的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MoneyLog.MoneyType> ToList(this MoneyLog.MoneyCategoryType type)
        {
            foreach (object obj in Enum.GetValues(typeof(MoneyLog)))
            {
                MoneyLog.MoneyType moneyType = (MoneyLog.MoneyType)obj;
                if (moneyType.GetCategory() == type)
                    yield return moneyType;
            }
        }

        #endregion

        #region ========== 工具方法 ==============

        /// <summary>
        /// 生成二维码的图片路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static string GetQRCode(string url, int width = 220, int height = 220)
        {
            return string.Format("http://pan.baidu.com/share/qrcode?w={0}&h={1}&url={2}", width, height, HttpUtility.UrlEncode(url));
        }

        public static int GetTableID(this int userId)
        {
            return userId % BetModule.TABLE_COUNT;
        }

        #endregion

    }

}
