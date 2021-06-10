using ipdb;
using SP.Studio.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SP.Studio.Web
{
    /// <summary>
    /// 纯真数据库操作类
    /// 2009.5.25 YM
    /// </summary>
    public class IPAgent
    {
        public static string IP
        {
            get
            {
                return GetIP(HttpContext.Current);
            }
        }

        /// <summary>
        /// 从传入的http对象中获取IP地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetIP(HttpContext context)
        {
            if (context == null) return "0.0.0.0";  // 非Web程序不获取IP
            string ip = context.Request.Headers["X-Real-IP"] ?? context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ip))
            {
                if (ip.Contains(":")) ip = ip.Substring(0, ip.IndexOf(':'));
                if (ip.Contains(",")) ip = ip.Substring(0, ip.IndexOf(','));
                if (!regex.IsMatch(ip)) ip = "127.0.0.1"; //不正确的IP可能是ipV6
            }
            else
            {
                ip = HttpContext.Current.Request.UserHostAddress;
            }
            return ip;
        }

        /// <summary>
        /// 把数字形式IP转化成为IPv4格式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetIP(long ip)
        {
            if (ip == 0) return string.Empty;
            string[] iplist = new string[4];

            for (int index = 0; index < iplist.Length; index++)
            {
                long pow = (int)Math.Pow(256, iplist.Length - index - 1);
                long num = ip / pow;
                iplist[index] = num.ToString();
                ip -= num * pow;
            }
            return string.Join(".", iplist);
        }

        public static long GetIP(string ip)
        {
            if (!regex.IsMatch(ip)) return 0;
            long[] num = ip.Split('.').Select(t => long.Parse(t)).ToArray();
            for (int i = 0; i < num.Length; i++)
            {
                num[i] *= (long)Math.Pow((long)256, (long)(num.Length - i - 1));
            }
            return num.Sum();
        }

        /// <summary>
        /// 判断来路是否属于本地IP
        /// </summary>
        /// <returns></returns>
        public static bool IsLocal()
        {
            if (HttpContext.Current == null) return false;
            string address = GetAddress(IP);
            return Regex.IsMatch(address, "本机|局域网");
        }

        /// <summary>
        /// 判断当前IP是否属于中国（不包含港澳台）
        /// </summary>
        public static bool IsChina(string ip)
        {
            CityInfo info = GetAddress(ip);
            return info.getCountryName() == "中国" && !new[] { "香港", "台湾", "澳门" }.Contains(info.getCityName());
        }

        public static readonly Regex regex = new Regex(@"(((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))\.){3}((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))");


        private static Dictionary<string, CityInfo> ipAddressCache = new Dictionary<string, CityInfo>();

        /// <summary>
        /// 获取IP的物理地址并且保存进入缓存
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static CityInfo GetAddress(string ip, bool isCache)
        {
            if (!regex.IsMatch(ip)) return new CityInfo(new string[] { ip });
            CityInfo info;
            if (ipAddressCache.ContainsKey(ip))
            {
                info = ipAddressCache[ip];
            }
            else
            {
                lock (ipAddressCache)
                {
                    string file;
                    if (HttpContext.Current != null)
                    {
                        file = HttpContext.Current.Server.MapPath("~/bin/ipipfree.ipdb");
                    }
                    else
                    {
                        file = System.Environment.CurrentDirectory + @"\ipipfree.ipdb";
                    }
                    City db = new City(file);
                    info = db.findInfo(ip, "CN");
                    if (!ipAddressCache.ContainsKey(ip)) ipAddressCache.Add(ip, info);
                }
            }
            return info;
        }

        public static CityInfo GetAddress(string ip)
        {
            return GetAddress(ip, true);
        }

        public static string GetAddress()
        {
            return GetAddress(IP);
        }
    }
}
