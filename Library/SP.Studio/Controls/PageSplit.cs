using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.ComponentModel;
using System.Resources;

using SP.Studio.Web;
using SP.Studio.Array;

namespace SP.Studio.Controls
{
    /// <summary>
    /// 分页控件
    /// </summary>
    [DefaultProperty("PageSplit"), ToolboxData("<{0}:PageSplit runat=server />")]
    public sealed class PageSplit : WebControl
    {
        public new string CssClass { get; set; }

        private int _pageSize = 20;

        [Bindable(true), DefaultValue(20), Description("分页大小")]
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        /// <summary>
        /// 当前的页码
        /// </summary>
        public int PageIndex
        {
            get
            {
                return WebAgent.GetParam(PageName, 1);
            }
        }

        public int RecordCount { get; set; }

        private string _pageName = "Page";

        /// <summary>
        /// 分页参数名
        /// </summary>
        [Bindable(true), DefaultValue("Page"), Description("页码属性名")]
        public string PageName
        {
            get { return _pageName; }
            set { _pageName = value; }
        }

        /// <summary>
        /// URL重写规则
        /// </summary>
        public string UrlRewrite { get; set; }

        private PageSplitStyle _pageStyle = PageSplitStyle.PageDropDown;

        /// <summary>
        /// 分页样式
        /// </summary>
        [Bindable(true), DefaultValue(PageSplitStyle.PageDropDown), Description("分页大小")]
        public PageSplitStyle PageStyle
        {
            get { return _pageStyle; }
            set { _pageStyle = value; }
        }

        public LanguageType Language { get; set; }

        public enum PageSplitStyle
        {
            /// <summary>
            /// 把页码列出来
            /// </summary>
            PageList,
            /// <summary>
            /// 下拉选择
            /// </summary>
            PageDropDown,
            /// <summary>
            /// 显示当前页码附近的
            /// </summary>
            NearBy
        }

        /// <summary>
        /// 链接地址
        /// </summary>
        private string LinkUrl(object page = null)
        {
            string url;
            if (page == null) page = "${Page}";
            if (string.IsNullOrEmpty(UrlRewrite))
            {
                url = HttpContext.Current.Request.QueryString.ToString();
                List<string> queryList = new List<string>();
                bool isPage = false;
                foreach (string key in HttpContext.Current.Request.QueryString.AllKeys)
                {
                    string value = HttpContext.Current.Request.QueryString[key];
                    if (key.Equals(PageName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        value = page.ToString();
                        isPage = true;
                    }
                    queryList.Add(string.Format("{0}={1}", key, value == "${Page}" ? value : HttpUtility.UrlEncode(value)));
                }
                if (!isPage)
                {
                    queryList.Add(string.Format("{0}={1}", this.PageName, page));
                }
                //Regex regex = new Regex(string.Format(@"^{0}=(?<PageIndex>[^\&]+)|\&{0}=(?<PageIndex>[^\&]+)", PageName), RegexOptions.IgnoreCase);
                //if (regex.IsMatch(url))
                //{
                //    url = regex.Replace(url, string.Format("&{0}={1}", PageName, page));
                //}
                //else
                //{
                //    url = string.Format("{0}&{1}={2}", url, PageName, page);
                //}
                //if (url.StartsWith("&")) url = url.Substring(1);
                //if (!url.StartsWith("?")) url = "?" + url;
                //if (!string.IsNullOrEmpty(UrlRewrite) && UrlRewrite.Contains("${Url}"))
                //{
                //    return UrlRewrite.Replace("${Url}", url);
                //}
                return string.Concat("?", string.Join("&", queryList));
            }
            else
            {
                url = Regex.Replace(this.UrlRewrite, string.Format(@"\<{0}\>", this.PageName), page.ToString(), RegexOptions.IgnoreCase);
                foreach (Match match in Regex.Matches(url, @"\<(?<Key>[^\>]+)\>"))
                {
                    url = url.Replace(match.Value, HttpContext.Current.Server.UrlEncode(WebAgent.QS(match.Groups["Key"].Value)));
                }
                return url;
            }
        }

        /// <summary>
        /// 创建列表方式的HTML结构
        /// </summary>
        private string CreateList(int maxPage)
        {
            StringBuilder sb = new StringBuilder();
            if (PageIndex > 1)
            {
                sb.AppendFormat("<a href=\"{0}\" class=\"First Page\">{1}</a>", LinkUrl(1), Get(LanguageKey.First));
                sb.AppendFormat("<a href=\"{0}\" class=\"Previous Page\">{1}</a>", LinkUrl(PageIndex - 1), Get(LanguageKey.Previous));
            }
            for (int pageIndex = 1; pageIndex <= maxPage; pageIndex++)
            {
                sb.AppendFormat("<a href=\"{0}\" title=\"{1}\" class=\"number {2}\">{1}</a>", LinkUrl(pageIndex), pageIndex, pageIndex == this.PageIndex ? "current" : "");
            }
            if (PageIndex < maxPage)
            {
                sb.AppendFormat("<a href=\"{0}\" class=\"Next Page\">{1}</a>", LinkUrl(PageIndex + 1), Get(LanguageKey.Next));
                sb.AppendFormat("<a href=\"{0}\" class=\"Last Page\">{1}</a>", LinkUrl(maxPage), Get(LanguageKey.Last));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 显示当前页码附近的几个页面
        /// </summary>
        private string CreateNearBy(int maxPage)
        {
            StringBuilder sb = new StringBuilder();
            int near = 4;
            if (PageIndex > near)
                sb.AppendFormat("<a href=\"{0}\">1...</a>", LinkUrl(1));
            if (PageIndex > 1)
                sb.AppendFormat("<a href=\"{0}\" class=\"Previous Page\">{1}</a>", LinkUrl(PageIndex - 1), this.Get(LanguageKey.Previous));
            for (var index = PageIndex - near; index <= PageIndex + near; index++)
            {
                if (index > 0 && index <= maxPage)
                {
                    sb.AppendFormat("<a href=\"{0}\"{2}>{1}</a>", LinkUrl(index), index, index == PageIndex ? " class=\"IndexOn Page\"" : "");
                }
            }
            if (PageIndex < maxPage)
                sb.AppendFormat("<a href=\"{0}\" class=\"Next Page\">{1}</a>", LinkUrl(PageIndex + 1), this.Get(LanguageKey.Next));
            if (PageIndex < maxPage - near)
                sb.AppendFormat("<a href=\"{0}\">...{1}</a>", LinkUrl(maxPage), maxPage);
            return sb.ToString();
        }

        /// <summary>
        /// 下拉列表的方式
        /// </summary>
        private string CreatePageDropDown(int maxPage)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder jump = new StringBuilder();
            sb.AppendFormat(Get(LanguageKey.Records), RecordCount);
            sb.Append(" ");
            sb.AppendFormat(Get(LanguageKey.PageIndex), PageIndex, maxPage);
            sb.Append(" ");
            sb.Append(PageIndex > 1 ? string.Format("<a href=\"{0}\">{1}</a>", LinkUrl(1), Get(LanguageKey.First)) : Get(LanguageKey.First));
            sb.Append(" ");
            sb.Append(PageIndex > 1 ? string.Format("<a href=\"{0}\">{1}</a>", LinkUrl(PageIndex - 1), Get(LanguageKey.Previous)) : Get(LanguageKey.Previous));
            sb.Append(" ");
            sb.Append(PageIndex < maxPage ? string.Format("<a href=\"{0}\">{1}</a>", LinkUrl(PageIndex + 1), Get(LanguageKey.Next)) : Get(LanguageKey.Next));
            sb.Append(" ");
            sb.Append(PageIndex < maxPage ? string.Format("<a href=\"{0}\">{1}</a>", LinkUrl(maxPage), Get(LanguageKey.Last)) : Get(LanguageKey.Last));
            sb.Append(" ");
            if (maxPage < 20)
            {
                jump.Append("<select onchange=\"location.href=this.value;\">");
                for (int index = 1; index <= maxPage; index++)
                {
                    jump.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", LinkUrl(index), index, index == PageIndex ? " selected" : "");
                }
                jump.Append("</select>");
            }
            else
            {
                jump.AppendFormat("<input type=\"text\" size=\"{0}\" value=\"{1}\" ", maxPage.ToString().Length, PageIndex);
                jump.Append("onchange=\"if(!/^\\d+$/.test(this.value)){ this.value='" + PageIndex + "'; }else{ var page = Number(this.value) > " + maxPage + " ? " + maxPage + " : Number(this.value); location.href='" + LinkUrl() + "'.replace('${Page}',page); }  \" />");
            }

            sb.AppendFormat(Get(LanguageKey.Jump), jump.ToString());
            return sb.ToString();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.WriteLine("PageSplit " + this.GetType().FullName);
                return;
            }
            writer.Write(this.ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (PageSize == 0) PageSize = 20;
            int max = RecordCount % PageSize == 0 ? RecordCount / PageSize : RecordCount / PageSize + 1;
            if (max == 0) max = 1;
            if (string.IsNullOrEmpty(PageName)) PageName = "Page";

            sb.AppendFormat("<div class=\"{0}\" id=\"{1}\" data-pagesize=\"{2}\" data-pageindex=\"{3}\" data-maxpage=\"{4}\" data-recordcount=\"{5}\">",
                CssClass, this.ClientID, this.PageSize, this.PageIndex, max, RecordCount);
            switch (PageStyle)
            {
                case PageSplitStyle.PageList:
                    sb.Append(CreateList(max));
                    break;
                case PageSplitStyle.NearBy:
                    sb.Append(CreateNearBy(max));
                    break;
                case PageSplitStyle.PageDropDown:
                    sb.Append(CreatePageDropDown(max));
                    break;
            }
            sb.AppendFormat("<input type=\"hidden\" name=\"{0}$RecordCount\" id=\"{1}_RecordCount\" value=\"{2}\" />", this.UniqueID, this.ClientID, this.RecordCount);
            sb.Append("</div>");

            return sb.ToString();
        }


        private string Get(LanguageKey language)
        {
            return SP.Studio.Controls.Language.Get(language, this.Language);
        }


    }


}
