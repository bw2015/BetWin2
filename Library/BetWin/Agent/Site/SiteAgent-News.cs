using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Sites;
using BW.Common.Admins;

using SP.Studio.Data;

namespace BW.Agent
{
    partial class SiteAgent
    {
        /// <summary>
        /// 新增或者修改栏目
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool SaveNewsColumn(NewsColumn column)
        {
            if (string.IsNullOrEmpty(column.Name))
            {
                base.Message("请输入栏目名");
                return false;
            }

            if (column.Sort < 0)
            {
                base.Message("排序值错误");
                return false;
            }

            column.SiteID = SiteInfo.ID;

            if (column.ID != 0)
            {
                if (column.Update() != 0)
                {
                    AdminInfo.Log(AdminLog.LogType.Site, "修改栏目{0},ID:{1}", column.Name, column.ID);
                    return true;
                }
            }
            else
            {
                if (column.Add(true))
                {
                    AdminInfo.Log(AdminLog.LogType.Site, "增加栏目{0},ID:{1}", column.Name, column.ID);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取栏目列表
        /// </summary>
        /// <returns></returns>
        public List<NewsColumn> GetNewsColumnList(NewsColumn.ContentType type)
        {
            return BDC.NewsColumn.Where(t => t.SiteID == SiteInfo.ID && t.Type == type).OrderByDescending(t => t.Sort).ToList();
        }

        /// <summary>
        /// 获取栏目信息
        /// </summary>
        /// <param name="colId"></param>
        /// <returns></returns>
        public NewsColumn GetNewsColumnInfo(int colId)
        {
            return BDC.NewsColumn.Where(t => t.SiteID == SiteInfo.ID && t.ID == colId).FirstOrDefault();
        }

        /// <summary>
        /// 删除栏目
        /// </summary>
        /// <param name="colId"></param>
        /// <returns></returns>
        public bool DeleteNewsColumn(int colId)
        {
            NewsColumn column = this.GetNewsColumnInfo(colId);
            if (column == null)
            {
                base.Message("编号错误");
                return false;
            }

            if (BDC.News.Where(t => t.SiteID == SiteInfo.ID && t.ColID == column.ID).Count() != 0)
            {
                base.Message("该栏目下存在内容");
                return false;
            }

            if (column.Delete() != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "删除栏目:{0} ID:{1}", column.Name, column.ID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 保存新闻信息
        /// </summary>
        /// <param name="news"></param>
        /// <returns></returns>
        public bool SaveNewsInfo(News news)
        {
            if (news.ColID == 0)
            {
                base.Message("请选择栏目");
                return false;
            }

            if (string.IsNullOrEmpty(news.Title))
            {
                base.Message("请输入标题");
                return false;
            }

            if (string.IsNullOrEmpty(news.Content))
            {
                base.Message("请输入内容");
                return false;
            }
            NewsColumn column = this.GetNewsColumnInfo(news.ColID);
            if (column == null)
            {
                base.Message("栏目错误");
                return false;
            }

            news.SiteID = SiteInfo.ID;
            news.AdminID = AdminInfo.ID;
            news.Type = column.Type;

            if (news.ID == 0)
            {
                news.CreateAt = DateTime.Now;
                if (news.Add())
                {
                    AdminInfo.Log(AdminLog.LogType.Site, "添加公告{0}", news.Title);
                    return true;
                }
            }
            else
            {
                if (news.Update() == 1)
                {
                    AdminInfo.Log(AdminLog.LogType.Site, "修改公告{0},ID:{1}", news.Title, news.ID);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取公告内容
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        public News GetNewsInfo(int newsId)
        {
            return BDC.News.Where(t => t.SiteID == SiteInfo.ID && t.ID == newsId).FirstOrDefault();
        }

        /// <summary>
        /// 获取需要弹出提醒的公告ID
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetNewsTip(NewsColumn.ContentType type)
        {
            if (UserInfo == null) return 0;

            IQueryable<int> newsRead = BDC.NewsRead.Where(p => p.UserID == UserInfo.ID).Select(p => p.NewsID);

            int? newsId = BDC.News.Where(t => t.SiteID == SiteInfo.ID && t.Type == type && t.Tip != 0 &&
                (t.Tip < 0 || (t.CreateAt.AddDays(t.Tip) > DateTime.Now && !newsRead.Contains(t.ID)))).Select(t => (int?)t.ID).FirstOrDefault();
            return newsId == null ? 0 : newsId.Value;
        }

        /// <summary>
        /// 获取公告内容（同一栏目中的前一篇后一篇）
        /// </summary>
        /// <param name="newsId"></param>
        /// <param name="next"></param>
        /// <param name="previous"></param>
        /// <returns></returns>
        public News GetNewsInfo(int newsId, out News next, out News previous)
        {
            next = previous = null;
            News news = this.GetNewsInfo(newsId);
            if (news == null) return null;

            next = BDC.News.Where(t => t.SiteID == SiteInfo.ID && t.ColID == news.ColID && t.ID > news.ID).OrderBy(t => t.ID).FirstOrDefault();
            previous = BDC.News.Where(t => t.SiteID == SiteInfo.ID && t.ColID == news.ColID && t.ID < news.ID).OrderBy(t => t.ID).FirstOrDefault();
            return news;
        }

        /// <summary>
        /// 标记已读
        /// </summary>
        /// <param name="newsId"></param>
        public void NewsRead(int newsId)
        {
            if (UserInfo == null) return;

            NewsRead read = new NewsRead() { NewsID = newsId, UserID = UserInfo.ID, ReadAt = DateTime.Now };
            if (read.Exists())
            {
                read.Update(null, t => t.ReadAt);
            }
            else
            {
                read.Add();
            }
        }

        /// <summary>
        /// 删除公告
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        public bool DeleteNews(int newsId)
        {
            News news = this.GetNewsInfo(newsId);
            if (news == null)
            {
                base.Message("编号错误");
                return false;
            }

            if (news.Delete() != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "删除公告{0},ID:{1}", news.Title, news.ID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取区域信息（会返回一个默認值）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Region GetRegionInfo(int id)
        {
            return BDC.Region.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).FirstOrDefault() ?? new Region();
        }

        /// <summary>
        /// 获取区域信息（会返回一个默認值）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Region GetRegionInfo(string name)
        {
            return BDC.Region.Where(t => t.SiteID == SiteInfo.ID && t.Name == name).FirstOrDefault() ?? new Region();
        }

        /// <summary>
        /// 获取当前站点的区域列表
        /// </summary>
        /// <returns></returns>
        public List<Region> GetRegionList()
        {
            return BDC.Region.Where(t => t.SiteID == SiteInfo.ID).OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// 保存区域设置信息
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public bool SaveRegionInfo(Region region)
        {
            if (string.IsNullOrEmpty(region.Content))
            {
                base.Message("未设置内容");
                return false;
            }
            if (string.IsNullOrEmpty(region.Name))
            {
                base.Message("未设置区域名字");
                return false;
            }

            if (BDC.Region.Where(t => t.SiteID == SiteInfo.ID && t.Name == region.Name && t.ID != region.ID).Count() != 0)
            {
                base.Message("区域名字已经存在");
                return false;
            }

            region.SiteID = SiteInfo.ID;
            region.CreateAt = DateTime.Now;
            bool success = false;
            if (region.ID == 0)
            {
                success = region.Add();
            }
            else
            {
                success = region.Update() != 0;
            }

            if (success)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "保存区域{0}[{1}]", region.Name, region.Title);
            }
            return success;
        }

        /// <summary>
        /// 刪除一個區域
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteRegion(int id)
        {
            Region region = this.GetRegionInfo(id);
            if (region.ID != id)
            {
                base.Message("编号错误");
                return false;
            }

            if (region.Delete() != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "删除区域{0}[{1}]", region.Name, region.Title);
                return true;
            }
            return false;
        }

    }
}
