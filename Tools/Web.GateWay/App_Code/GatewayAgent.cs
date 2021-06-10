using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Configuration;
using SP.Studio.Data;
using SP.Studio.Text;
using SP.Studio.Core;
using SP.Studio.Net;

namespace Web.GateWay.App_Code
{
    public class GatewayAgent : AgentBase
    {

        /// <summary>
        /// 通过远程接口获取邀请码对应的域名列表
        /// </summary>
        private string[] inviteDomainAPI
        {
            get
            {
                string domain = ConfigurationManager.AppSettings["invite-domain"];
                if (string.IsNullOrEmpty(domain)) return null;

                return domain.Split(',');
            }
        }

        /// <summary>
        /// 通过远程接口获取站点的配置参数
        /// </summary>
        private string[] siteSettingAPI
        {
            get
            {
                string setting = ConfigurationManager.AppSettings["site-setting"];
                if (string.IsNullOrEmpty(setting)) return null;
                return setting.Split(',');
            }
        }

        /// <summary>
        /// 通过邀请ID获取域名列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<string> GetInviteDomain(string id)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetInviteDomain",
                    NewParam("@InviteID", id));

                List<string> list = new List<string>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add((string)dr[0]);
                }
                if (list.Count != 0) return list;

                if (inviteDomainAPI != null)
                {
                    foreach (string url in this.inviteDomainAPI)
                    {
                        try
                        {
                            string result = NetAgent.UploadData(url, "Code=" + id, Encoding.UTF8);
                            XElement root = XElement.Parse(result);

                            list = root.Elements().Select(t => t.Value).ToList();
                            if (list.Count != 0) return list;
                        }
                        catch
                        {

                        }
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 通过邀请ID获取站点
        /// </summary>
        /// <param name="inviteId"></param>
        /// <returns></returns>
        public int GetSiteID(string inviteId)
        {
            using (DbExecutor db = NewExecutor())
            {
                int? siteId = (int?)db.ExecuteScalar(CommandType.Text, "SELECT SiteID FROM usr_Invite WHERE InviteID = @InviteID",
                    NewParam("@InviteID", inviteId));
                if (siteId == null) return 0;
                return siteId.Value;
            }
        }

        /// <summary>
        /// 获取站点的参数设定
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetSiteSetting(int siteId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT SiteName,Setting FROM Site WHERE SiteID = @SiteID",
                    NewParam("@SiteID", siteId));
                if (ds.Tables[0].Rows.Count == 0)
                {
                    if (siteSettingAPI != null)
                    {
                        foreach (string url in this.siteSettingAPI)
                        {
                            try
                            {
                                string result = NetAgent.UploadData(url, string.Format("SiteID={0}&Property=APPAndroid,APPIOS", siteId), Encoding.UTF8);

                                if (string.IsNullOrEmpty(result)) continue;
                                XElement root = XElement.Parse(result);
                                foreach (XElement item in root.Elements())
                                {
                                    string key = item.Name.ToString();
                                    if (!dic.ContainsKey(key))
                                        dic.Add(key, item.Value);
                                }
                                return dic;
                            }
                            catch
                            {

                            }
                        }
                    }

                    return null;
                }
                else
                {

                    DataRow dr = ds.Tables[0].Rows[0];
                    dic.Add("SiteName", (string)dr["SiteName"]);
                    string setting = (string)dr["Setting"];
                    NameValueCollection request = HttpUtility.ParseQueryString(setting);
                    foreach (string key in request.AllKeys)
                    {
                        if (!dic.ContainsKey(key))
                            dic.Add(key, request[key]);
                    }
                }
            }

            return dic;
        }

        /// <summary>
        /// 获取站点的域名
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<string> GetDomain(int siteId)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT Domain FROM site_Domain WHERE SiteID = @SiteID ORDER BY Sort DESC",
                    NewParam("@SiteID", siteId));
                if (ds.Tables[0].Rows.Count == 0) return null;

                return ds.ToList<string>();
            }
        }

        /// <summary>
        /// 获取站点的主域名
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public string GetMainDomain(int siteId)
        {
            using (DbExecutor db = NewExecutor())
            {
                return (string)db.ExecuteScalar(CommandType.Text, "SELECT top 1 Domain FROM site_Domain WHERE SiteID = @SiteID AND IsSpeed = 0 ORDER BY Sort DESC",
                    NewParam("@SiteID", siteId));
            }
        }

        /// <summary>
        /// 获取站点的域名列表（带上http前缀）
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<string> GetMainDomainList(int siteId)
        {
            using (DbExecutor db = NewExecutor())
            {
                List<string> list = new List<string>();
                Regex regex = new Regex(":443$");
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT Domain FROM site_Domain WHERE SiteID = @SiteID AND IsSpeed = 0 ORDER BY Sort DESC",
                    NewParam("@SiteID", siteId));
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string domain = (string)dr["Domain"];
                    if (regex.IsMatch(domain))
                    {
                        domain = regex.Replace(domain, string.Empty);
                        domain = "https://" + domain;
                    }
                    else
                    {
                        domain = "http://" + domain;
                    }
                    list.Add(domain);
                }
                return list;
            }
        }

        /// <summary>
        /// 获取区域内容
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetContent(int siteId, string name)
        {
            using (DbExecutor db = NewExecutor())
            {
                object obj = db.ExecuteScalar(CommandType.Text, "SELECT TOP 1 Content FROM site_Region WHERE SiteID = @SiteID AND Name = @Name",
                    NewParam("@SiteID", siteId),
                    NewParam("@Name", name));
                return obj == null ? string.Empty : (string)obj;
            }
        }

        /// <summary>
        /// 获取数据库缓存数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public XElement GetCacheData(Guid id, out byte type)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT Data,Type FROM site_Cache WHERE CacheID = @ID",
                    NewParam("@ID", id));
                if (ds.Tables[0].Rows.Count == 0)
                {
                    type = 0;
                    return null;
                }

                type = (byte)ds.Tables[0].Rows[0]["Type"];
                return XElement.Parse((string)ds.Tables[0].Rows[0]["Data"]);
            }
        }

        /// <summary>
        /// 获取APP的版本
        /// </summary>
        /// <param name="siteid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetType(int siteId, string type)
        {
            if (type != "mobile" && type != "mobile3") return type;
            using (DbExecutor db = NewExecutor())
            {
                string setting = (string)db.ExecuteScalar(CommandType.Text, "SELECT Setting FROM [Site] WHERE SiteID = @SiteID",
                    NewParam("@SiteID", siteId));
                if (setting == null) return type;

                Regex regex = new Regex(@"APPVersion=(?<Version>[\d\.]+)");
                if (regex.IsMatch(setting))
                {
                    switch (regex.Match(setting).Groups["Version"].Value)
                    {
                        case "3.0":
                            type = "mobile3";
                            break;
                        default:
                            type = "mobile";
                            break;
                    }
                }
                return type;
            }
        }
    }
}