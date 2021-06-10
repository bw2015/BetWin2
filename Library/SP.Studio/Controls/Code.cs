using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.ComponentModel;

using SP.Studio.Web;

namespace SP.Studio.Controls
{
    /// <summary>
    /// 验证码 (需要与 SP.Studio\Drawing\ValidateCodeHandler.cs 配合使用)
    /// </summary>
    [DefaultProperty("Code"), ToolboxData("<{0}:Code runat=server />")]
    public class Code : System.Web.UI.Control
    {
        public Code() : base() { }

        public Code(string name)
        {
            this.Name = name;
        }

        private string _name;
        /// <summary>
        /// session的名字
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null) _name = this.UniqueID;
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public Unit Width { get; set; }

        public Unit Height { get; set; }

        private string _path = "/ValidateCode.axd";
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
            }
        }

        /// <summary>
        /// 通过ajax来检查验证码是否正确的地址
        /// </summary>
        public string CheckUrl
        {
            get
            {
                return string.Concat(this.Path, "?ac=check&name=", this.Name);
            }
        }

        public string CssClass { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write("<span style=\"border:1px solid #CCCCCC; padding:5px;\">验证码</span>");
                return;
            }
            writer.Write(this.ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<img src=\"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==\" onclick=\"this.src='{0}?Name={1}&r=' + Math.random();\" class=\"{2}\" id=\"{3}\" data-name=\"{1}\" data-check=\"{4}\" /><script> document.getElementById('{3}').click(); </script>",
                Path, Name, CssClass, this.ClientID, this.CheckUrl);
            return sb.ToString();
        }

        /// <summary>
        /// 验证输入的校验码是否正确 
        /// </summary>
        /// <returns>当Session不存在的时候默认通过验证</returns>
        public bool Verify(string code = null)
        {
            if (string.IsNullOrEmpty(code)) code = WebAgent.QF(this.UniqueID);
            string session = (string)HttpContext.Current.Session[this.Name];
            if (string.IsNullOrEmpty(session)) return false;
            HttpContext.Current.Session.Remove(this.Name); //2014.6.1 修改无论验证是否成功都要删除掉原来的session
            return code.Equals(session, StringComparison.CurrentCultureIgnoreCase);
        }


    }
}
