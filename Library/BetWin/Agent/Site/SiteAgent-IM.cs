using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;

using BW.Common.Sites;
using BW.Common.Admins;
using SP.Studio.Data;
namespace BW.Agent
{
    /// <summary>
    /// 客服系统设置
    /// </summary>
    partial class SiteAgent
    {
        /// <summary>
        /// 更新常用语分类的名字
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool UpdateReplyCategory(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                base.Message("名称不能为空");
                return false;
            }
            ReplyCategory category = BDC.ReplyCategory.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).FirstOrDefault();
            if (category == null)
            {
                base.Message("编号错误");
                return false;
            }
            if (BDC.ReplyCategory.Where(t => t.SiteID == SiteInfo.ID && t.Name == name && t.ID != id).Count() != 0)
            {
                base.Message("该名称已经存在");
                return false;
            }
            category.Name = HttpUtility.HtmlEncode(name);
            if (category.Update(null, t => t.Name) != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "修改常用语分类名称为{0},ID={1}", name, id);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除常用语分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteReplyCategory(int id)
        {
            ReplyCategory category = BDC.ReplyCategory.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).FirstOrDefault();
            if (category == null)
            {
                base.Message("编号错误");
                return false;
            }
            if (category.Delete() != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "删除常用语分类名称为{0},ID={1}", category.Name, id);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 新增分类
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AddReplyCategory(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                base.Message("名称不能为空");
                return false;
            }
            if (BDC.ReplyCategory.Where(t => t.SiteID == SiteInfo.ID && t.Name == name).Count() != 0)
            {
                base.Message("该名称已经存在");
                return false;
            }
            return new ReplyCategory()
            {
                SiteID = SiteInfo.ID,
                Name = name
            }.Add();
        }

        /// <summary>
        /// 获取常用语信息
        /// </summary>
        /// <param name="replyId"></param>
        /// <returns></returns>
        public Reply GetReplyInfo(int replyId)
        {
            if (replyId == 0) return null;
            return BDC.Reply.Where(t => t.SiteID == SiteInfo.ID && t.ID == replyId).FirstOrDefault();
        }

        /// <summary>
        /// 保存常用语信息
        /// </summary>
        /// <param name="reply"></param>
        /// <returns></returns>
        public bool SaveReplyInfo(Reply reply)
        {
            if (string.IsNullOrEmpty(reply.Content))
            {
                base.Message("常用语内容为空");
                return false;
            }
            reply.SiteID = SiteInfo.ID;
            reply.AdminID = AdminInfo.ID;
            reply.CreateAt = DateTime.Now;
            bool success;
            if (reply.ID == 0)
            {
                success = reply.Add();
            }
            else
            {
                success = reply.Update(null, t => t.CateID, t => t.Content) != 0;
            }
            if (success)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "设置常用语{0}", reply.Content);
            }
            return success;
        }

        /// <summary>
        /// 删除常用语
        /// </summary>
        /// <param name="replyId"></param>
        /// <returns></returns>
        public bool DeleteReply(int replyId)
        {
            Reply reply = this.GetReplyInfo(replyId);
            if (reply == null)
            {
                base.Message("编号错误");
                return false;
            }

            if (reply.Delete() != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "删除常用语{0}", reply.Content);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取关键词设置
        /// </summary>
        /// <param name="keywordId"></param>
        /// <returns></returns>
        public ReplyKeyword GetReplyKeywordInfo(int keywordId)
        {
            if (keywordId == 0) return null;
            return BDC.ReplyKeyword.Where(t => t.SiteID == SiteInfo.ID && t.ID == keywordId).FirstOrDefault();
        }

        /// <summary>
        /// 保存关键词设置
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public bool SaveReplyKeywordInfo(ReplyKeyword keyword)
        {
            if (string.IsNullOrEmpty(keyword.Keyword))
            {
                base.Message("请输入关键词");
                return false;
            }
            if (string.IsNullOrEmpty(keyword.Content))
            {
                base.Message("请输入内容");
                return false;
            }

            keyword.CreateAt = DateTime.Now;
            keyword.SiteID = SiteInfo.ID;
            bool success = false;
            if (keyword.ID == 0)
            {
                success = keyword.Add();
            }
            else
            {
                success = keyword.Update(null, t => t.Keyword, t => t.Content) != 0;
            }
            if (success)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "设定关键词{0}:{1}", keyword.Keyword, keyword.Content);
            }
            return success;
        }

        /// <summary>
        /// 删除关键词
        /// </summary>
        /// <param name="keywordId"></param>
        /// <returns></returns>
        public bool DeleteKeyword(int keywordId)
        {
            ReplyKeyword keyword = this.GetReplyKeywordInfo(keywordId);
            if (keyword == null)
            {
                base.Message("编号错误");
                return false;
            }

            if (keyword.Delete() != 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "删除关键词{0}:{1}", keyword.Keyword, keyword.Content);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 保存机器人设置信息
        /// </summary>
        /// <param name="rebot"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool SaveRebotInfo(Site.RebotSetting rebot, HttpPostedFile file)
        {
            string result = UserAgent.Instance().UploadImage(file, "face");
            if (!string.IsNullOrEmpty(result))
            {
                SiteInfo.Rebot.Face = result;
            }
            SiteInfo.RebotString = rebot.ToString();
            return SiteInfo.Update(null, t => t.RebotString) != 0;
        }

       
    }
}
