using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using SP.Studio.Data;
using System.Data;
using System.Xml.Linq;
using SP.Studio.Xml;
using SP.Studio.Security;

namespace Web.GateWay.App_Code
{
    /// <summary>
    /// 接受采集结果，直接存储至数据库
    /// </summary>
    public class ResultAgent : AgentBase
    {
        /// <summary>
        /// 已经采集过的彩期 彩种,(彩期,号码)
        /// </summary>
        internal static Dictionary<string, SortedDictionary<string, int>> data = new Dictionary<string, SortedDictionary<string, int>>();

        /// <summary>
        /// 上次采集结果
        /// </summary>
        internal static Dictionary<string, string> _last = new Dictionary<string, string>();

        private const string CONFIGURL = "http://localhost:21000/handler/game/lottery/type";

        private static Dictionary<string, LotteryConfig> __config;
        /// <summary>
        /// 彩种的参数设定
        /// </summary>
        private static Dictionary<string, LotteryConfig> config
        {
            get
            {
                if (__config == null)
                {
                    try
                    {
                        XElement root = XElement.Load(CONFIGURL);
                        __config = new Dictionary<string, LotteryConfig>();
                        foreach (XElement item in root.Elements())
                        {
                            __config.Add(item.Name.ToString(), new LotteryConfig(item));
                        }
                    }
                    catch
                    {
                        __config = null;
                    }
                }
                return __config;
            }
        }

        /// <summary>
        /// 存储一条采集结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="number"></param>
        /// <param name="site">系统彩标识</param>
        /// <returns></returns>
        public bool Save(string type, string index, string number, string site = null)
        {
            if (!config.ContainsKey(type)) return false;

            LotteryConfig lotteryConfig = config[type];
            number = lotteryConfig.GetNumber(number);
            if (string.IsNullOrEmpty(number)) return false;

            int siteId = 0;
            if (!string.IsNullOrEmpty(site))
            {
                Regex regex = new Regex(@"^(?<SiteID>\d{4})(?<Key>[0-9A-F]{3})$");
                if (regex.IsMatch(site))
                {
                    siteId = int.Parse(regex.Match(site).Groups["SiteID"].Value);
                    string md5Key = regex.Match(site).Groups["Key"].Value;
                    if (MD5.toMD5(siteId + type).Substring(0, 3) != md5Key)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            string lottery = type;
            if (siteId != 0) lottery = siteId + "-" + type;

            lock (lottery)
            {
                try
                {
                    if (!data.ContainsKey(lottery)) data.Add(lottery, new SortedDictionary<string, int>());
                    SortedDictionary<string, int> dic = data[lottery];
                    if (!dic.ContainsKey(index)) dic.Add(index, 0);
                    if (dic[index] > 3) return false;
                    bool success = false;
                    using (DbExecutor db = NewExecutor())
                    {
                        success = db.ExecuteNonQuery(CommandType.StoredProcedure, "SaveResultNumber",
                             NewParam("@SiteID", siteId),
                             NewParam("@Type", lotteryConfig.Value),
                             NewParam("@Index", index),
                             NewParam("@Number", number),
                             NewParam("@ResultAt", DateTime.Now)) != 0;
                        dic[index]++;
                    }
                    if (dic[index] == 1)
                    {
                        this.CreateTrend(lotteryConfig, index, number, siteId);
                    }
                }
                catch (Exception ex)
                {
                    SystemAgent.Instance().AddError(ex, string.Format("[开奖错误]{0}第{1}期{2}", type, index, number));
                    return false;
                }
            }

            return true;
        }

        public bool CreateTrend(LotteryConfig lotteryConfig, string index, string number, int siteId = 0)
        {
            XElement root = null;
            using (DbExecutor db = NewExecutor())
            {
                try
                {
                    int count = (int)db.ExecuteScalar(CommandType.Text, "SELECT COUNT(0) FROM  lot_Trend WHERE [Type] = @Type AND SiteID = @SiteID AND [Index] = @Index",
                        NewParam("@Type", lotteryConfig.Value),
                        NewParam("@SiteID", siteId),
                        NewParam("@Index", index));
                    if (count != 0) return false;

                    string result = (string)db.ExecuteScalar(CommandType.Text, "SELECT TOP 1 [Result] FROM lot_Trend WHERE [Type] = @Type AND SiteID = @SiteID ORDER BY [Index] DESC",
                        NewParam("@Type", lotteryConfig.Value),
                        NewParam("@SiteID", siteId));
                    if (!string.IsNullOrEmpty(result))
                    {
                        root = XElement.Parse(result);
                    }
                    else
                    {
                        root = new XElement("root");
                    }


                    int numberIndex = 0;
                    XElement newRoot = new XElement("root");

                    string[] resultNumber = number.Split(',');
                    string[] ball = lotteryConfig.Number.Split(',');


                    foreach (string num in resultNumber)
                    {
                        foreach (string n in ball)
                        {
                            string name = string.Format("N{0}-{1}", numberIndex, n);
                            XElement item = root.Elements().Where(t => t.GetAttributeValue("name") == name).FirstOrDefault();
                            int value = 0;
                            if (item == null)
                            {
                                item = new XElement("item");
                                item.SetAttributeValue("name", name);
                            }
                            else
                            {
                                value = item.GetAttributeValue("value", 0);
                            }

                            item.SetAttributeValue("value", n == num ? 0 : value + 1);
                            newRoot.Add(item);
                        }
                        numberIndex++;
                    }

                    // 分布遗漏
                    foreach (string n in ball)
                    {
                        string name = string.Format("D{0}", n);
                        int value = 0;
                        XElement item = root.Elements().Where(t => t.GetAttributeValue("name") == name).FirstOrDefault();

                        if (item == null)
                        {
                            item = new XElement("item");
                            item.SetAttributeValue("name", name);
                        }
                        else
                        {
                            value = item.GetAttributeValue("value", 0);
                        }

                        item.SetAttributeValue("value", resultNumber.Contains(n) ? 0 : value + 1);
                        newRoot.Add(item);
                    }


                    db.ExecuteNonQuery(CommandType.Text, "INSERT INTO lot_Trend VALUES(@Type,@Index,@SiteID,@Number,@Result)",
                        NewParam("@Type", lotteryConfig.Value),
                        NewParam("@Index", index),
                        NewParam("@SiteID", siteId),
                        NewParam("@Number", number),
                        NewParam("@Result", newRoot.ToString()));
                    return true;
                }
                catch (Exception ex)
                {
                    SystemAgent.Instance().AddError(ex, string.Format("[走势生成错误]{0}第{1}期{2}", lotteryConfig.Value, index, number));
                    return false;
                }
            }
        }
    }
}