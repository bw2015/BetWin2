using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.Agent;
using BW.Common.Sites;

using SP.Studio.Core;
using SP.Studio.Model;
using BW.Framework;



namespace BW.Handler.admin
{
    /// <summary>
    /// 新闻公告管理
    /// </summary>
    public class news : IHandler
    {
        /// <summary>
        /// 保存公告栏目
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void savecolumn(HttpContext context)
        {
            NewsColumn column = context.Request.Form.Fill<NewsColumn>();

            this.ShowResult(context, SiteAgent.Instance().SaveNewsColumn(column), "保存成功");
        }

        /// <summary>
        /// 公告栏目
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void columnlist(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(SiteAgent.Instance().GetNewsColumnList(QF("Type").ToEnum<NewsColumn.ContentType>()), t => new
            {
                t.ID,
                t.Name,
                t.Sort
            }));
        }

        /// <summary>
        /// 更新栏目信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void updatecolumn(HttpContext context)
        {
            NewsColumn column = SiteAgent.Instance().GetNewsColumnInfo(QF("ID", 0));
            if (column == null)
            {
                context.Response.Write(false, "编号错误");
            }
            switch (QF("Name"))
            {
                case "Name":
                    column.Name = QF("Value");
                    break;
                case "Sort":
                    column.Sort = QF("Value", (short)-1);
                    break;
            }
            this.ShowResult(context, SiteAgent.Instance().SaveNewsColumn(column));
        }

        /// <summary>
        /// 删除栏目
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void deletecolumn(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteNewsColumn(QF("ID", 0)), "删除成功");
        }
        /// <summary>
        /// 保存文章内容
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void save(HttpContext context)
        {
            News news = SiteAgent.Instance().GetNewsInfo(QF("ID", 0)) ?? new News();
            news = context.Request.Form.Fill(news);
            news.Content = QF("Content");

            string cover = UserAgent.Instance().UploadImage(context.Request.Files["coverfile"], "upload");

            if (!string.IsNullOrEmpty(cover))
            {
                news.Cover = cover;
            }

            this.ShowResult(context, SiteAgent.Instance().SaveNewsInfo(news), "保存成功");
        }

        /// <summary>
        /// 公告列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void list(HttpContext context)
        {
            NewsColumn.ContentType type = QF("Type").ToEnum<NewsColumn.ContentType>();
            IQueryable<News> list = BDC.News.Where(t => t.SiteID == SiteInfo.ID && t.Type == type);
            if (QF("ColID", 0) != 0) list = list.Where(t => t.ColID == QF("ColID", 0));
            if (!string.IsNullOrEmpty(QF("Title"))) list = list.Where(t => t.Title.Contains(QF("Title")));

            Dictionary<int, string> column = SiteAgent.Instance().GetNewsColumnList(type).ToDictionary(t => t.ID, t => t.Name);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.Sort).ThenByDescending(t => t.ID), t => new
            {
                t.ID,
                ColName = column[t.ColID],
                t.Title,
                t.CreateAt,
                Admin = AdminAgent.Instance().GetAdminName(t.AdminID),
                t.Sort
            }));
        }

        /// <summary>
        /// 公告的内容
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void info(HttpContext context)
        {
            News news = SiteAgent.Instance().GetNewsInfo(QF("ID", 0)) ?? new News();

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                news.ID,
                news.Title,
                news.ColID,
                news.Content,
                news.CreateAt,
                news.Tip,
                news.CoverShow
            });
        }

        /// <summary>
        /// 删除公告
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void delete(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteNews(QF("ID", 0)), "删除成功");
        }

        /// <summary>
        /// 从编辑器里面上传图片文件
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void upload(HttpContext context)
        {
            string url = UserAgent.Instance().UploadImage(context.Request.Files["imgFile"], "upload");
            if (string.IsNullOrEmpty(url))
            {
                context.Response.Write("{\"error\" : 1,\"message\" : \"" + UserAgent.Instance().Message() + "\"}");
            }
            else
            {
                context.Response.Write("{\"error\" : 0,\"url\" : \"" + SysSetting.GetSetting().imgServer + url + "\"}");
            }
        }

        /// <summary>
        /// 更新文章的排序值
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.Value)]
        private void updatesort(HttpContext context)
        {
            News news = SiteAgent.Instance().GetNewsInfo(QF("ID", 0));
            if (news == null)
            {
                context.Response.Write(false, "编号错误");
            }

            short sort = QF("value", (short)-1);
            if (sort < 0)
            {
                context.Response.Write(false, "输入值错误");
            }

            news.Sort = sort;
            this.ShowResult(context, SiteAgent.Instance().SaveNewsInfo(news), "修改成功");
        }

        /// <summary>
        /// 自定义区域的信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.内容管理.Value)]
        private void regioninfo(HttpContext context)
        {
            Region region = SiteAgent.Instance().GetRegionInfo(QF("ID", 0));
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                region.ID,
                region.Name,
                region.Title,
                region.Content
            });
        }

        /// <summary>
        /// 保存自定义区域内容
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.内容管理.Value)]
        private void saveregioninfo(HttpContext context)
        {
            Region region = SiteAgent.Instance().GetRegionInfo(QF("ID", 0));
            region = context.Request.Form.Fill(region);

            this.ShowResult(context, SiteAgent.Instance().SaveRegionInfo(region), "保存成功");
        }

        /// <summary>
        /// 區域列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.内容管理.Value)]
        private void regionlist(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(SiteAgent.Instance().GetRegionList(), t => new
            {
                t.ID,
                t.Name,
                t.Title,
                t.CreateAt
            }));
        }


        /// <summary>
        /// 刪除區域
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.内容管理.Value)]
        private void deleteregion(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteRegion(QF("ID", 0)), "删除成功");
        }
    }
}
