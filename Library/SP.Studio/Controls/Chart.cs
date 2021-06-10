using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

using SP.Studio.Controls.Charts;

namespace SP.Studio.Controls
{
    /// <summary>
    /// 报表控件
    /// </summary>
    [DefaultProperty("Chart"), ToolboxData("<{0}:Chart runat=server />")]
    public sealed class Chart : Control
    {
        /// <summary>
        /// 报表swf文件的目录
        /// </summary>
        public static string gPath = "/Studio/resources/Chart/";

        public string Width { get; set; }

        public string Height { get; set; }

        public string CssName { get; set; }

        /// <summary>
        /// 报表数据
        /// </summary>
        public XmlDataBase Data { get; set; }

        /// <summary>
        /// 报表模式
        /// </summary>
        public ChartMode Mode { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(this.ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<embed src=\"{0}{1}.swf\" width=\"{2}\" height=\"{3}\" quality=\"high\" pluginspage=\"http://www.macromedia.com/go/getflashplayer\" type=\"application/x-shockwave-flash\" salign=\"T\" name=\"{4}\" id=\"{5}\" menu=\"false\" wmode=\"transparent\" ",
                gPath, Mode, Width, Height, this.UniqueID, this.ClientID);
            sb.AppendFormat("flashvars=\"lang=EN&amp;debugMode=undefined&amp;scaleMode=noScale&amp;dataXML={0}&amp;DOMId={1}&amp;registerWithJS=1&amp;chartWidth={2}&amp;chartHeight={3}&amp;InvalidXMLText=Invalid data.&amp;dataURL=\"></embed>",
                Data == null ? string.Empty : HttpContext.Current.Server.UrlPathEncode(Data), this.ClientID, Width, Height);

            return sb.ToString();
        }
    }
}
