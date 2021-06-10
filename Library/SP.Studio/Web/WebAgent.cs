using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.PageBase;
using SP.Studio.Xml;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace SP.Studio.Web
{
    public static class WebAgent
    {
        #region ============== 取值 =============


        /// <summary>
        /// 有过滤HTML字符
        /// </summary>
        public static string QS(string key)
        {
            string value = HttpContext.Current.Request.QueryString[key] + "";
            return HttpUtility.HtmlEncode(value);
        }

        public static string QF(string key)
        {
            return HttpContext.Current.QF(key);
        }

        public static string QF(this HttpContext context, string key)
        {
            return context.Request.Form[key] ?? "";
        }

        public static string GetParam(string key)
        {
            return HttpContext.Current.Request[key] + "";
        }

        public static int GetParam(string key, int def)
        {
            int value;
            return int.TryParse(GetParam(key), out value) ? value : def;
        }

        public static T GetParam<T>(string key, T def)
        {
            string value = GetParam(key);
            if (typeof(T) == typeof(string) && string.IsNullOrEmpty(value)) return def;
            if (!WebAgent.IsType<T>(value)) return def;
            return (T)SP.Studio.Core.ObjectExtensions.GetValue(value, typeof(T));
        }

        public static int QS(string key, int def)
        {
            int value;
            return int.TryParse(QS(key), out value) ? value : def;
        }

        public static int QF(string key, int def)
        {
            int value;
            return int.TryParse(QF(key), out value) ? value : def;
        }

        public static T QF<T>(string key, T t)
        {
            string value = QF(key);
            if (!WebAgent.IsType<T>(value)) return t;
            return (T)SP.Studio.Core.ObjectExtensions.GetValue(value, typeof(T));
        }

        /// <summary>
        /// 通过正则表达式获取数组内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="regex"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<T> QF<T>(Regex regex, T t)
        {
            foreach (string key in HttpContext.Current.Request.Form.AllKeys)
            {
                if (!regex.IsMatch(key)) continue;
                yield return QF(key, t);
            }
        }

        public static T QF<T>(this HttpContext context, string key, T t)
        {
            string value = context.QF(key);
            if (!WebAgent.IsType<T>(value)) return t;
            return (T)SP.Studio.Core.ObjectExtensions.GetValue(value, typeof(T));
        }

        /// <summary>
        /// 返回指定类型的值，如果非指定类型则返回null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Object QF(string key, Type type)
        {
            string value = QF(key);
            if (!WebAgent.IsType(value, type)) return null;
            return SP.Studio.Core.ObjectExtensions.GetValue(value, type);
        }

        public static string QC(string key)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request.Cookies[key] == null) return string.Empty;
            return HttpContext.Current.Request.Cookies[key].Value;
        }

        public static T QC<T>(string key, T def)
        {
            string value = QC(key);
            if (!WebAgent.IsType<T>(value)) return def;
            return (T)SP.Studio.Core.ObjectExtensions.GetValue(value, typeof(T));
        }

        public static string QC(string coll, string key)
        {
            if (HttpContext.Current.Request.Cookies[coll] == null) return null;
            return HttpContext.Current.Request.Cookies[coll].Values[key];
        }

        public static int QC(string coll, string key, int def)
        {
            string qc = QC(coll, key);
            int value;
            return int.TryParse(qc, out value) ? value : def;
        }

        #endregion

        #region ==========  页面交互操作  ============

        public static void FaidAndBack(string msg = null, string url = null)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write("<script language=\"javascript\">");
            if (!string.IsNullOrEmpty(msg))
            {
                HttpContext.Current.Response.Write(string.Format(" alert(\"{0}\");", msg.Replace("\"", "\\\"").Replace("\n", "\\n")));
            }
            if (string.IsNullOrEmpty(url))
            {
                HttpContext.Current.Response.Write("history.back();");
            }
            else
            {
                HttpContext.Current.Response.Write(string.Format("location.href=\"{0}\";", url));
            }
            HttpContext.Current.Response.Write("</script>");

            HttpContext.Current.Response.End();
        }

        public static void FaidAndClose(string msg = null)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write("<script language=\"javascript\">");
            if (!string.IsNullOrEmpty(msg))
            {
                HttpContext.Current.Response.Write(string.Format(" alert(\"{0}\");", msg.Replace("\"", "\\\"")));
            }
            HttpContext.Current.Response.Write("window.close();");
            HttpContext.Current.Response.Write("</script>");
            HttpContext.Current.Response.End();
        }

        public static void Alert(string msg)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(String.Format("<script language='javascript'>alert(\"{0}\");</script>", msg));
        }

        /// <summary>
        /// 跳转到指定Url
        /// </summary>
        public static void SuccAndGo(string goUrl)
        {
            HttpContext.Current.Response.Clear();
            if (string.IsNullOrEmpty(goUrl)) goUrl = HttpContext.Current.Request.UrlReferrer.ToString();
            HttpContext.Current.Response.Write(String.Format("<script language='javascript'>document.location.href='{0}';</script>", goUrl));
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 跳转到指定Url
        /// </summary>
        /// <param name="goUrl">如果为null在自动获取来路地址</param>
        public static void SuccAndGo(string msg, string goUrl)
        {
            if (string.IsNullOrEmpty(goUrl)) goUrl = HttpContext.Current.Request.UrlReferrer.ToString();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(String.Format("<script language='javascript'> alert(\"{0}\"); document.location.href='{1}';</script>", msg, goUrl));
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 弹出提示框，并且给出双URL选择
        /// </summary>
        public static void SuccAndGo(string msg, string url1, string url2)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(String.Format("<script language='javascript'> document.location.href = confirm(\"{0}\") ? \"{1}\" : \"{2}\"; </script>", msg, url1, url2));
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 弹出信息并且刷新
        /// </summary>
        /// <param name="msg">提示的信息 如果为null则不作提示</param>
        /// <param name="parentReload">是否刷新父级页面</param>
        public static void SuccAndGo(string msg, bool parentReload)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(String.Format("<script language='javascript'> {0} {1}.location.reload();</script>",
                string.IsNullOrEmpty(msg) ? "" : string.Format("alert(\"{0}\");", msg),
                parentReload ? "parent" : "self"));
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 输出一段js，并且终止页面执行
        /// </summary>
        /// <param name="js"></param>
        public static void WriteJS(string js)
        {
            HttpContext.Current.Response.Clear();
            StringBuilder sb =
                new StringBuilder("<!DOCTYPE html><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>")
                 .AppendFormat("<script language=\"javascript\"> {0} </script>", js)
                 .Append("</head></html>");
            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }


        #endregion

        #region =============  字符串验证  =============

        /// <summary>
        /// 是否是电子邮件
        /// </summary>
        public static bool IsEmail(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return Regex.IsMatch(input, @"^[\w\.\-]+@[\w\.\-]+\.[a-z]{2,3}$", RegexOptions.IgnoreCase);
        }

        public static bool IsQQ(string input)
        {
            return Regex.IsMatch(input, @"[1-9]\d{4,12}");
        }

        public static bool IsMobile(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return Regex.IsMatch(input, @"^1[0-9]{10}$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否是身份证号码
        /// </summary>
        public static bool IsIDCard(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (!Regex.IsMatch(input, @"^\d{17}[0-9x]$", RegexOptions.IgnoreCase)) return false;

            long n = 0;
            if (long.TryParse(input.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(input.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(input.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = input.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = input.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != input.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }
            return true;
        }

        /// <summary>
        /// 银行卡规则判断
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsBankCard(string input)
        {
            if (!Regex.IsMatch(input, @"^\d{6,}$")) return false;
            int[] cardArr = new int[input.Length];
            for (int i = 0; i < cardArr.Length; i++)
            {
                cardArr[i] = int.Parse(input[i].ToString());
            }

            for (int i = cardArr.Length - 2; i >= 0; i -= 2)
            {
                cardArr[i] <<= 1;
                cardArr[i] = cardArr[i] / 10 + cardArr[i] % 10;
            }

            int sum = 0;
            for (int i = 0; i < cardArr.Length; i++)
            {
                sum += cardArr[i];
            }
            return sum % 10 == 0;
        }


        private static Dictionary<string, string> _bankCardData;
        private static Dictionary<string, string> _bankCard
        {
            get
            {
                if (_bankCardData != null) return _bankCardData;

                XElement root = XElement.Parse((string)new ResourceManager(typeof(SP.Studio.Files.LocalData)).GetObject("Bank"));
                _bankCardData = root.Elements().ToDictionary(t => t.GetAttributeValue("code"), t => t.GetAttributeValue("bank"));
                return _bankCardData;
            }
        }


        /// <summary>
        /// 获取银行卡的开户行（通过支付宝的接口）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetBankCard(string input)
        {
            if (input.Length < 6) return null;
            string code = input.Substring(0, 6);
            if (_bankCard.ContainsKey(code)) return _bankCard[code];
            string url = string.Format("https://ccdcapi.alipay.com/validateAndCacheCardInfo.json?_input_charset=utf-8&cardNo={0}&cardBinCheck=true", input);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            Regex regex = new Regex(@"""bank"":""(?<Bank>[A-Z]+)""");
            if (regex.IsMatch(result)) return regex.Match(result).Groups["Bank"].Value;
            return null;
        }

        /// <summary>
        /// 是否全部是中文
        /// </summary>
        public static bool IsChinese(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return Regex.IsMatch(input, @"^[\u4e00-\u9fa5]+$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否包含中文
        /// </summary>
        public static bool HasChinese(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return Regex.IsMatch(input, @"[\u4e00-\u9fa5]+", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 验证用户名是否可以注册（默认规则，字母数字横杠下划线加中文）
        /// </summary>
        /// <returns></returns>
        public static bool IsUserName(string userName)
        {
            string Illegal = @"<>\/ *?";

            if (userName.Length < 2 || userName.Length > 16) return false;
            foreach (char c in Illegal)
            {
                if (userName.Contains(c)) return false;
            }

            return true;
        }

        /// <summary>
        /// 验证用户名(只允许数字、英文）
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IsUserNameByEnglish(string userName, int min = 4, int max = 16)
        {
            return Regex.IsMatch(userName, @"^[0-9a-z\-_]{" + min + "," + max + "}$", RegexOptions.IgnoreCase);
        }

        #endregion

        #region ==========  表单判断  ============

        /// <summary>
        /// 不允许为空
        /// </summary>
        public static void IsEmpty(string value, string msg)
        {
            if (string.IsNullOrEmpty(value)) FaidAndBack(msg);
        }

        /// <summary>
        /// 非邮件格式则提示后退
        /// </summary>
        public static void IsEmail(string value, string msg)
        {
            if (!IsEmail(value)) WebAgent.FaidAndBack(msg);
        }

        /// <summary>
        /// 非手机号码格式则提示后退
        /// </summary>
        public static void IsMobile(string value, string msg)
        {
            if (!IsMobile(value)) WebAgent.FaidAndBack(msg);
        }

        #endregion

        /// <summary>
        /// 301重定向
        /// </summary>
        /// <param name="url"></param>
        public static void Redirect(string url)
        {
            HttpContext.Current.Response.StatusCode = 301;
            HttpContext.Current.Response.Status = "301 Moved Permanently";
            HttpContext.Current.Response.AddHeader("Location", url);
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 把字符串转化成为数字数组
        /// </summary>
        /// <param name="str">用逗号隔开的数字</param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static T[] GetArray<T>(string str, char split = ',')
        {
            if (str == null) str = string.Empty;
            str = str.Replace(" ", string.Empty);
            string regex = null;
            T[] result = new T[] { };
            switch (typeof(T).Name)
            {
                case "Int32":
                case "Byte":
                    regex = string.Format(@"(\d+{0})?\d$", split);
                    if (Regex.IsMatch(str, regex, RegexOptions.IgnoreCase))
                    {
                        result = str.Split(split).Where(t => WebAgent.IsType<int>(t)).ToList().ConvertAll(t => (T)Convert.ChangeType(t, typeof(T))).ToArray();
                    }
                    break;
                case "Guid":
                    regex = @"([0-9a-f]{8}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{12}" + split + @")?([0-9a-f]{8}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{12})$";
                    if (Regex.IsMatch(str, regex, RegexOptions.IgnoreCase))
                    {
                        result = str.Split(split).ToList().ConvertAll(t => (T)((object)Guid.Parse(t))).ToArray();
                    }
                    break;
                case "Decimal":
                    regex = string.Format(@"([0-9\.]+{0})?\d+$", split);
                    if (Regex.IsMatch(str, regex, RegexOptions.IgnoreCase))
                    {
                        result = str.Split(split).ToList().ConvertAll(t => (T)Convert.ChangeType(t, typeof(T))).ToArray();
                    }
                    break;
                case "Double":
                    result = str.Split(split).Where(t => WebAgent.IsType<T>(t)).Select(t => (T)Convert.ChangeType(t, typeof(T))).ToArray();
                    break;
                case "String":
                    result = str.Split(split).ToList().FindAll(t => !string.IsNullOrEmpty(t.Trim())).ConvertAll(t => (T)((object)t.Trim())).ToArray();
                    break;
                case "DateTime":
                    result = str.Split(split).ToList().FindAll(t => WebAgent.IsType<DateTime>(t)).ConvertAll(t => (T)((object)DateTime.Parse(t))).ToArray();
                    break;
                default:
                    if (typeof(T).IsEnum)
                    {
                        result = str.Split(split).Where(t => Enum.IsDefined(typeof(T), t)).Select(t => (T)Enum.Parse(typeof(T), t)).ToArray();
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// 生成一个随机数字。 包括min和max
        /// </summary>
        public static int GetRandom(int minValue = 0, int maxValue = int.MaxValue)
        {
            return new Random().Next(minValue, maxValue == int.MaxValue ? maxValue : maxValue + 1);
        }

        /// <summary>
        /// 从QueryString字符串中获取值
        /// </summary>
        public static string GetValue(string queryString, string key)
        {
            HttpRequest request = new HttpRequest(null, "http://localhost", queryString);
            return request[key];
        }

        /// <summary>
        /// 根据当前地址一个带参数URL地址。
        /// 效果：如果当前参数地址包含了Key则替换，如果没有则新增
        /// </summary>
        /// <param name="removeKey">需要移除的URL参数</param>
        public static string GetLink(string key, string value = null, string page = null, params string[] removeKey)
        {
            List<string> query = new List<string>();
            var removeKeyList = removeKey == null ? new List<string>() : removeKey.ToList();
            foreach (var q in HttpContext.Current.Request.QueryString.AllKeys)
            {
                if (string.IsNullOrEmpty(q)) continue;
                if (q.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(value))
                        query.Add(string.Format("{0}={1}", key, HttpContext.Current.Server.UrlEncode(value)));
                }
                else
                {
                    if (!removeKeyList.Exists(t => t.Equals(q, StringComparison.CurrentCultureIgnoreCase)))
                        query.Add(string.Format("{0}={1}", q, HttpContext.Current.Server.UrlEncode(WebAgent.QS(q))));
                }
            }
            if (!query.Exists(t => t.StartsWith(key + "=")) && !string.IsNullOrEmpty(value)) query.Add(string.Format("{0}={1}", key, HttpContext.Current.Server.UrlEncode(value)));
            return page + "?" + query.Join('&');
        }

        /// <summary>
        /// 获取Url的文件名 (去除路径和参数)
        /// </summary>
        /// <param name="url">要查询的Url，如果为空则为当前所在的网址</param>
        /// <returns></returns>
        public static string GetPage(string url = null)
        {
            if (string.IsNullOrEmpty(url)) url = HttpContext.Current.Request.RawUrl;
            url = url.Substring(url.LastIndexOf('/') + 1);
            if (url.Contains('?')) url = url.Substring(0, url.IndexOf('?'));
            return url;
        }

        /// <summary>
        /// 获取域名的顶级域
        /// </summary>
        public static string GetDomain(this Uri uri)
        {
            string[] domains = new string[] { ".com.cn", ".net.cn", ".org.cn", ".com", ".net", ".cn", ".cc", ".me", ".tw", ".hk" };
            string domain = uri.Host;
            if (domain.Contains("localhost")) return domain;
            foreach (string name in domains)
            {
                if (domain.EndsWith(name))
                {
                    domain = domain.Replace(name, "");
                    domain = domain.Split('.')[domain.Split('.').Length - 1] + name;
                    return domain;
                }
            }
            return domain;
        }

        public static string GetDomain(this string url)
        {
            Uri uri = new Uri(url);
            return uri.GetDomain();
        }

        /// <summary>
        /// 获取当前的域名（支持反代过来的域名）
        /// </summary>
        /// <returns></returns>
        public static string GetDomain()
        {
            HttpContext context = HttpContext.Current;
            if (context == null) return string.Empty;

            if (context.Request.UrlReferrer != null)
            {
                return context.Request.UrlReferrer.Authority;
            }
            return context.Request.Url.Authority;
        }

        /// <summary>
        /// 给路径加上当前域名
        /// </summary>
        public static string Domain(string path)
        {
            if (path.StartsWith("http")) return path;
            return string.Concat("http://", HttpContext.Current.Request.Url.Authority, path.StartsWith("/") ? "" : "/", path);
        }

        /// <summary>
        /// 检查字符串是否是已固定的格式
        /// </summary>
        public static bool IsType<T>(string s)
        {
            return IsType(s, typeof(T));
        }

        public static bool IsType(string s, Type type)
        {
            bool isType = false;
            switch (type.Name)
            {
                case "Int32":
                    int int32;
                    isType = int.TryParse(s, out int32);
                    break;
                case "Int16":
                    short int16;
                    isType = short.TryParse(s, out int16);
                    break;
                case "Int64":
                    long int64;
                    isType = long.TryParse(s, out int64);
                    break;
                case "Guid":
                    Guid guid;
                    isType = Guid.TryParse(s, out guid);
                    break;
                case "DateTime":
                    DateTime dateTime;
                    isType = DateTime.TryParse(s, out dateTime);
                    break;
                case "Decimal":
                    decimal money;
                    isType = Decimal.TryParse(s, out money);
                    break;
                case "Double":
                    double doubleValue;
                    isType = Double.TryParse(s, out doubleValue);
                    break;
                case "String":
                    isType = true;
                    break;
                case "Boolean":
                    isType = Regex.IsMatch(s, "1|0|true|false", RegexOptions.IgnoreCase);
                    break;
                case "Byte":
                    byte byteValue;
                    isType = byte.TryParse(s, out byteValue);
                    break;
                default:
                    if (type.IsEnum)
                    {
                        isType = Enum.IsDefined(type, s);
                    }
                    else
                    {
                        throw new Exception("WebAgent.IsType 方法暂时未能检测该种类型" + type.FullName);
                    }
                    break;
            }
            return isType;
        }

        /// <summary>
        /// 判断一个字符串包含的布尔信息
        /// </summary>
        public static bool Boolean(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value == "1" || value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// 截取文本
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length">中文字符的长度（英文的话等于×2）</param>
        /// <returns></returns>
        public static string Left(string str, int length)
        {
            if (string.IsNullOrEmpty(str) || length == 0) return str;
            StringBuilder sb = new StringBuilder();
            length *= 2;
            int index = 0;
            foreach (char c in str)
            {
                int count = Encoding.Default.GetByteCount(c.ToString());
                index += count;
                sb.Append(c.ToString());
                if (index >= length - 4) { sb.Append("..."); break; }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取左侧开始的一部分（只计算字符，不考虑半角和全角）
        /// </summary>
        public static string LeftString(string str, int length)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return str.Substring(0, str.Length > length ? length : str.Length);
        }

        /// <summary>
        /// 隐藏字符串的某一部分
        /// </summary>
        public static string Hidden(string str, int index, int len = 0, char show = '*')
        {
            string tmp = string.Empty;
            int length = str.Length;
            if (index >= length) return str;
            if (len == 0) len = length - index;
            tmp = str.Substring(0, index);
            if (index + len >= length) return tmp + show.ToString().PadLeft(index + len - str.Length, show);
            return tmp + show.ToString().PadLeft(len, show) + str.Substring(index + len);
        }

        /// <summary>
        /// 隐藏电子邮件地址的部分
        /// </summary>
        public static string HiddenEmail(string email)
        {
            if (!WebAgent.IsEmail(email)) return email;

            string name = email.Substring(0, email.IndexOf('@'));
            return string.Concat(Hidden(name, 0, 4), email.Substring(email.IndexOf('@')));
        }

        /// <summary>
        /// 隐藏IP
        /// </summary>
        /// <param name="count">要隐藏的位数</param>
        /// <returns></returns>
        public static string HiddenIP(string ip, int count = 2)
        {
            string[] ips = ip.Split('.');
            for (var i = 0; i < ips.Length; i++)
            {
                if (ips.Length - i <= count) ips[i] = "*";
            }
            return string.Join(".", ips);
        }

        /// <summary>
        /// 隐藏用户名（显示第一位和最后一位）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string HiddenName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "***";
            if (name.Length < 4) return string.Format("{0}**", name.FirstOrDefault());
            return string.Format("{0}***{1}", name.FirstOrDefault(), name.LastOrDefault());
        }

        /// <summary>
        /// 隐藏手机号码（中间四位加密）
        /// </summary>
        public static string HiddenMobile(string mobile)
        {
            if (!WebAgent.IsMobile(mobile)) return mobile;

            return Hidden(mobile, 3, 4);
        }

        /// <summary>
        /// 隐藏QQ号码
        /// </summary>
        /// <param name="qq"></param>
        /// <returns></returns>
        public static string HiddenQQ(string qq)
        {
            if (!WebAgent.IsQQ(qq)) return qq;
            return Hidden(qq, 0, 4);
        }

        /// <summary>
        /// 与js兼容的URL编码
        /// </summary>
        public static string UrlEncode(string value)
        {
            StringBuilder result = new StringBuilder();
            string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 获取当前时间与1970-1-1之间的秒数
        /// </summary>
        public static long GetTimeStamp(DateTime? time = null)
        {
            if (time == null) time = DateTime.UtcNow;
            TimeSpan ts = time.Value - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// 获取当前时间与1970-1-1之间的毫秒数
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimeStamps(DateTime? time = null)
        {
            if (time == null) time = DateTime.Now;

            TimeZoneInfo local = TimeZoneInfo.Local;


            TimeSpan ts = time.Value - new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(local.BaseUtcOffset.TotalMilliseconds);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        /// <summary>
        ///  转化UTC时间为PST时间 
        ///  PST：(=PacificStandardTime)太平洋标准时间 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToPST(this DateTime dateTime)
        {
            return System.TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, "Pacific Standard Time");

        }

        /// <summary>
        /// 根据时间戳获取到时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime GetTimeStamp(double timeStamp)
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return time.AddSeconds(timeStamp);
        }

        /// <summary>
        /// 获取当前是第几周
        /// </summary>
        public static int GetWeekOfYear(DateTime dt)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            Calendar cal = ci.Calendar;
            CalendarWeekRule cwr = ci.DateTimeFormat.CalendarWeekRule;
            DayOfWeek dow = ci.DateTimeFormat.FirstDayOfWeek;
            return cal.GetWeekOfYear(dt, cwr, dow);
        }

        /// <summary>
        /// 获取用中文表示的时间差
        /// </summary>
        /// <param name="time"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static string GetTimeDiff(DateTime time, DateTime? now = null)
        {
            if (now == null) now = DateTime.Now;
            TimeSpan span = (now.Value - time);
            var day = (int)span.TotalDays;
            if (day > 365) return string.Concat(day / 365, "年前");
            if (day > 30) return string.Concat(day / 30, "月前");
            if (day > 7) return string.Concat(day / 7, "周前");
            if (day > 1) return string.Concat((int)day, "天前");
            var second = (int)span.TotalSeconds;
            if (second > 3600) return string.Concat(second / 3600, "小时前");
            if (second > 60) return string.Concat(second / 60, "分钟前");
            return string.Concat(Math.Max(0, (int)second), "秒之前");
        }

        /// <summary>
        /// 获取中文表示的时间差
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetTimeSpan(TimeSpan time)
        {
            if (time.TotalSeconds < 0) return null;
            var day = (int)time.TotalDays;
            if (day > 365) return string.Concat(day / 365, "年");
            if (day > 30) return string.Concat(day / 30, "月");
            if (day > 7) return string.Concat(day / 7, "周");
            if (day > 1) return string.Concat((int)day, "天");
            var second = (int)time.TotalSeconds;
            if (second > 3600) return string.Concat(second / 3600, "小时");
            if (second > 60) return string.Concat(second / 60, "分钟");
            return string.Concat(Math.Max(0, (int)second), "秒");
        }

        /// <summary>
        /// 数字的位合并
        /// </summary>
        /// <returns>十进制的数字</returns>
        public static int BitAppend(params int[] nums)
        {
            if (nums.Length == 0) return 0;
            if (nums.Length == 1) return nums[0];

            var bits = nums.ToList().ConvertAll(t => Convert.ToString(t, 2));
            var length = bits.ConvertAll(t => t.Length).Max();
            bits = bits.ConvertAll(t => t.PadLeft(length, '0'));
            char[] arr = new char[length];
            for (var index = 0; index < arr.Length; index++)
            {
                arr[index] = bits.ConvertAll(t => t[index]).Contains('1') ? '1' : '0';
            }
            return Convert.ToInt32(new String(arr), 2);
        }


        /// <summary>
        /// 获取农历的日期
        /// </summary>
        /// <param name="year">是否返回年份</param>
        public static string GetChineseDate(DateTime date, bool year = false)
        {
            ChineseLunisolarCalendar chineseDate = new ChineseLunisolarCalendar();

            int lYear = chineseDate.GetYear(DateTime.Now);
            int lMonth = chineseDate.GetMonth(DateTime.Now);
            int lDay = chineseDate.GetDayOfMonth(DateTime.Now);
            int leapMonth = chineseDate.GetLeapMonth(lYear);//获取第几个月是闰月,等于0表示本年无闰月  

            string calendarDate = null;
            //如果今年有闰月  
            if (leapMonth > 0)
            {
                //闰月数等于当前月份  
                if (lMonth == leapMonth)
                {
                    calendarDate = string.Format("闰{0}月{1}日", lMonth - 1, lDay);
                }
                else if (lMonth > leapMonth)//  
                {
                    calendarDate = string.Format("{0}月{1}日", lMonth - 1, lDay);
                }
                else
                {
                    calendarDate = string.Format("{0}月{1}日", lMonth, lDay);
                }
            }
            else
            {
                calendarDate = string.Format("{0}月{1}日", lMonth, lDay);
            }
            if (year) calendarDate = string.Concat(lYear, "年", calendarDate);
            return calendarDate;

        }


        /// <summary>
        /// 返回文件的大小
        /// </summary>
        public static string GetSize(long size)
        {
            if (size < 1024) return size + "byte";
            if (size < (long)1048576) return Math.Round((decimal)size / 1024M, 2) + "K";
            if (size < (long)1073741824) return Math.Round((decimal)size / 1048576M, 2) + "M";
            return Math.Round((decimal)size / 1073741824M, 2) + "G";
        }

        /// <summary>
        /// 根据邮箱地址获取对应的登陆网址
        /// </summary>
        public static string GetMailLoginUrl(string email)
        {
            string url = string.Empty;
            if (!email.Contains('@')) return url;
            string domain = email.Substring(email.IndexOf('@') + 1).ToLower();
            switch (domain)
            {
                case "gmail.com":
                    url = "https://mail.google.com/";
                    break;
                case "qq.com":
                case "163.com":
                case "sohu.com":
                case "yahoo.com.cn":
                case "yahoo.com":
                case "yahoo.cn":
                case "21cn.com":
                case "139.com":
                case "263.net":
                    url = "http://mail." + domain;
                    break;
                case "vip.qq.com":
                    url = "http://mail.qq.com";
                    break;
            }
            return url;
        }

        /// <summary>
        /// 获取页面返回的日志信息
        /// </summary>
        public static string GetPostLog(params string[] contents)
        {
            XElement root = new XElement("root");

            root.Add(new XElement("datetime", DateTime.Now));
            root.Add(new XElement("ip", IPAgent.IP));
            if (HttpContext.Current != null)
            {
                root.Add(new XElement("url", HttpContext.Current.Request.RawUrl));
                XElement header = new XElement("header");
                foreach (string key in HttpContext.Current.Request.Headers.AllKeys)
                {
                    header.Add(new XElement(key, HttpContext.Current.Request.Headers[key]));
                }
                root.Add(header);
                if (contents.Length % 2 == 0)
                {
                    for (int i = 0; i < contents.Length; i += 2)
                    {
                        root.Add(new XElement(contents[i], contents[i + 1]));
                    }
                }

                foreach (string key in HttpContext.Current.Request.Form.AllKeys)
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    root.Add(new XElement(key.Replace("$", "_"), HttpContext.Current.Request.Form[key]));
                }
            }
            return root.ToString();
        }

        /// <summary>
        /// 播放视频的地址
        /// </summary>
        /// <param name="url"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static string ShowVideo(string url, int width = 0, int height = 0)
        {
            return null;
        }

        /// <summary>
        /// 地球半径
        /// </summary>
        private const double EARTH_RADIUS = 6378.137D;
        private static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }
        /// <summary>
        /// 计算2个经纬度之间的地理距离
        /// </summary>
        /// <param name="lat1">经度1</param>
        /// <param name="lng1">纬度1</param>
        /// <param name="lat2">经度2</param>
        /// <param name="lng2">纬度2</param>
        /// <returns></returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        /// <summary>
        /// 把十进制的数字转化成为任意进制的数字
        /// </summary>
        /// <param name="id">十进制值</param>
        /// <param name="hex">进制</param>
        /// <returns></returns>
        public static int[] NumberToHex(int id, int hex)
        {
            List<int> list = new List<int>();
            int flag = 0;
            while (true)
            {
                if (Math.Pow(hex, flag) > id) break;
                flag++;
            }
            while (flag > 0)
            {
                int pow = (int)Math.Pow(hex, flag - 1);
                int index = id / pow;
                id -= index * pow;
                flag--;
                list.Add(index);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 短网址的转换因子
        /// </summary>
        private const string shortString = "0123456789ABCDEFGHIJKLMNOPQRSTURWXYZabcdefghijklmnopqrstuvwxyz";
        /// <summary>
        /// 把数字转化成为短字符串
        /// </summary>
        /// <param name="id">为正整数</param>
        /// <returns></returns>
        public static string NumberToShort(int id)
        {
            string _str = string.Empty;
            foreach (int index in NumberToHex(id, shortString.Length))
            {
                _str += shortString[index];
            }
            return _str;
        }

        /// <summary>
        /// 把短字符串转成数字
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int ShorttoNumber(string id)
        {
            int value = 0;
            int index = id.Length;
            foreach (char c in id)
            {
                value += shortString.IndexOf(c) * (int)Math.Pow(shortString.Length, index - 1);
                index--;
            }
            return value;
        }

        /// <summary>
        /// Guid转化成为数字
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static int GetNumber(Guid guid)
        {
            string str = guid.ToString();
            str = str.Substring(str.LastIndexOf('-') + 1);
            long result = Convert.ToInt64(str, 16);

            return (int)(result % int.MaxValue);
        }

        /// <summary>
        /// 把任意字符串转成guid
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Guid GetGuid(string str)
        {
            if (string.IsNullOrEmpty(str)) return Guid.Empty;
            return Guid.Parse(SP.Studio.Security.MD5.toMD5(str));
        }


        /// <summary>
        /// 检测当前访问是否是手机设备
        /// </summary>
        /// <returns></returns>
        public static bool IsMobile()
        {
            if (HttpContext.Current == null) return false;
            string userAgent = HttpContext.Current.Request.UserAgent;
            if (string.IsNullOrEmpty(userAgent)) return false;
            return Regex.IsMatch(userAgent, "Mobile|iPad|iPhone|Android", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否是安卓设备
        /// </summary>
        /// <returns></returns>
        public static bool IsAndroid()
        {
            if (HttpContext.Current == null) return false;
            string userAgent = HttpContext.Current.Request.UserAgent;
            return Regex.IsMatch(userAgent, "Android", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 当前是否是苹果设备
        /// </summary>
        /// <returns></returns>
        public static bool IsIOS()
        {
            if (HttpContext.Current == null) return false;
            string userAgent = HttpContext.Current.Request.UserAgent;
            return Regex.IsMatch(userAgent, "iPhone|iPad", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否在微信内访问
        /// </summary>
        public static bool IsWechat()
        {
            if (HttpContext.Current == null) return false;
            string userAgent = HttpContext.Current.Request.UserAgent;
            return Regex.IsMatch(userAgent, "MicroMessenger", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 判断是否是域名格式（支持端口)
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static bool IsDomain(string domain)
        {
            return Regex.IsMatch(domain, @"^\w[\.\w\-_\:]+$");
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string StringToBase64(string source)
        {
            byte[] bytedata = Encoding.ASCII.GetBytes(source);
            return Convert.ToBase64String(bytedata, 0, bytedata.Length);
        }

        public static string ByteToBase64(byte[] bytedata)
        {
            return Convert.ToBase64String(bytedata, 0, bytedata.Length);
        }

        /// <summary>
        /// base64编码还原成字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Base64ToString(string source)
        {
            return Base64ToString(source, Encoding.Default);
        }

        public static string Base64ToString(string source, Encoding encoding)
        {
            return encoding.GetString(Convert.FromBase64String(source));
        }

        /// <summary>
        /// 字符串转Base64编码
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] Base64ToByte(string source)
        {
            return Convert.FromBase64String(source);
        }


        private const string DEBUG_INPUTSTREAM = "DEBUG_INPUTSTREAM";
        /// <summary>
        /// 获取inputstream内容
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static byte[] GetInputSteam(HttpContext context)
        {
            if (!new string[] { "POST", "PUT" }.Contains(context.Request.HttpMethod)) return new byte[] { };
            if (context.Items.Contains(DEBUG_INPUTSTREAM)) return (byte[])context.Items[DEBUG_INPUTSTREAM];
            byte[] data = context.Request.BinaryRead(context.Request.TotalBytes);
            context.Items.Add(DEBUG_INPUTSTREAM, data);
            return data;
        }

        /// <summary>
        /// 把文字转化成为二维码（从百度接口获取）
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetQRCode(string content, int width = 220, int height = 220)
        {
            string url = string.Format("//qrcode.a8.to/chart?cht=qr&chs={0}x{1}&chl={2}", width, height, HttpUtility.UrlEncode(content));
            return url;
        }

        /// <summary>
        /// 获取当前所处的平台
        /// </summary>
        /// <returns></returns>
        public static PlatformType GetPlatformType()
        {
            if (!WebAgent.IsMobile())
            {
                return PlatformType.PC;
            }
            PlatformType type = PlatformType.Wap;
            if (WebAgent.IsWechat())
            {
                type = type | PlatformType.Wechat;
            }
            if (WebAgent.IsAndroid())
            {
                type = type | PlatformType.Android;
            }
            if (WebAgent.IsIOS())
            {
                type = type | PlatformType.IOS;
            }
            return type;
        }
    }
}
