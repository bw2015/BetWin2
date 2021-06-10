using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

using System.Xml.Linq;
using SP.Studio.Xml;

namespace SP.Studio.Core
{
    /// <summary>
    /// 时间的扩展
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 把时间转换成为与1970-1-1 0：0：0 的秒值（PHP中经常用到）
        /// </summary>
        public static int To1970(this DateTime time)
        {
            return (int)((TimeSpan)(time - new DateTime(1970, 1, 1))).TotalSeconds;
        }

        /// <summary>
        /// 把距离1970-1-1 0:0: 的秒值转换成为时间日期
        /// </summary>
        public static DateTime ToDateTime(this int second)
        {
            return new DateTime(1970, 1, 1).AddSeconds((double)second);
        }

        /// <summary>
        /// 计算两个时间之间的天数量
        /// </summary>
        public static int GetDays(this DateTime datetime, DateTime? diff = null)
        {
            if (diff == null) diff = DateTime.Now;
            return ((TimeSpan)(datetime - diff)).Days;
        }

        /// <summary>
        /// 将时间通过command命令保存到系统时间
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static bool SetSystemTime(this DateTime datetime)
        {
            if (datetime.Year < 2000) return false;

            Process date = new Process();
            date.StartInfo.FileName = "cmd.exe";
            date.StartInfo.Arguments = "/c date " + datetime.ToShortDateString();
            date.StartInfo.CreateNoWindow = true;
            date.Start();

            Process time = new Process();
            time.StartInfo.FileName = "cmd.exe";
            time.StartInfo.Arguments = "/c time " + datetime.ToLongTimeString();
            time.StartInfo.CreateNoWindow = true;
            time.Start();

            return true;
        }

        private static List<Tuple<DateTime, bool>> _workday = null;

        /// <summary>
        /// 增加工作日
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime AddWorkDay(this DateTime datetime, int day)
        {
            if (_workday == null)
            {
                _workday = new List<Tuple<DateTime, bool>>();
                XElement root = XElement.Parse(SP.Studio.Files.LocalData.WorkDay);
                foreach (XElement item in root.Elements())
                {
                    _workday.Add(new Tuple<DateTime, bool>(item.GetAttributeValue("name", DateTime.MinValue), item.GetAttributeValue("value") == "1"));
                }
            }

            DateTime date = _workday.Where(t => t.Item2 && t.Item1 > datetime).Take(day).Select(t => t.Item1).LastOrDefault();

            return date.Add(datetime.TimeOfDay);
        }
    }
}
