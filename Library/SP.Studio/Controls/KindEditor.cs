using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.ComponentModel;
using System.Resources;
using System.Configuration;

using SP.Studio.Core;
using SP.Studio.Configuration;
using SP.Studio.Net;
using SP.Studio.Web;

namespace SP.Studio.Controls
{
    public class KindEditor : TextBox, IPostBackDataHandler
    {
        private const string KEY = "KindEditor4";

        private static string UploadHandler = "/kindeditor/uploadJson.axd";

        public static string Path = "/Studio/ke4";

        /// <summary>
        /// 静态构造
        /// </summary>
        static KindEditor()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["kindeditor.upload"]))
                UploadHandler = ConfigurationManager.AppSettings["kindeditor.upload"];
        }

        /// <summary>
        /// 编辑器的Javascript ID
        /// </summary>
        public string EditorID
        {
            get
            {
                return string.Concat("editor_", this.ClientID);
            }
        }

        #region ========== 參數設定 ===========

        /// <summary>
        /// 語言
        /// </summary>
        public LanguagePack langType { get; set; }

        private int _resizeType = 2;
        /// <summary>
        /// 是否允許改變大小
        /// 0: 不允許 1：只允許改變高 2：允許改變高寬
        /// </summary>
        public int resizeType
        {
            get
            {
                return _resizeType;
            }
            set
            {
                _resizeType = value;
            }
        }

        private bool _allowImageUpload = true;
        /// <summary>
        /// 是否允許上傳圖片
        /// </summary>
        public bool allowImageUpload
        {
            get
            {
                return _allowImageUpload;
            }
            set
            {
                _allowImageUpload = value;
            }
        }


        /// <summary>
        /// 是否允许上传FLASH文件
        /// </summary>
        public bool allowFlashUpload { get; set; }

        /// <summary>
        /// 是否是簡易版本
        /// </summary>
        public bool IsSimple { get; set; }

        /// <summary>
        /// 是否过滤html标签(如果是简易版本则强制过滤)
        /// </summary>
        public bool filterMode { get; set; }

        #endregion

        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.WriteLine(string.Format("<textarea style=\"width:{0};height:{1}; background-color:#F7F7F7;\">KindEditor V4.1.2</textarea>", this.Width, this.Height));
                return;
            }

            if (IsSimple)
            {
                //this.allowImageUpload = false;
                this.resizeType = 0;
                this.filterMode = true;
            }

            if (!Context.Items.Contains(KEY))
            {
                Context.Items.Add(KEY, true);

                writer.Write(
                    string.Format("<link rel=\"stylesheet\" href=\"{0}/themes/default/default.css\" />" +
                                    "<script charset=\"utf-8\" src=\"{0}/kindeditor.js\"></script>" +
                                    "<script charset=\"utf-8\" src=\"{0}/lang/{1}.js\"></script>", Path, this.langType));
            }

            writer.Write("<textarea id=\"{0}\" name=\"{1}\" style=\"width:{2};height:{3};\" class=\"{5}\">{4}</textarea>",
                this.ClientID, this.UniqueID, this.Width, this.Height, this.Text, this.CssClass);
            writer.Write("<script language=\"javascript\">");
            writer.Write("var {0};", this.EditorID);
            writer.Write("KindEditor.ready(function(K) { " + this.EditorID + " = K.create('#" + this.ClientID + "', { ");
            writer.Write(" langType : '{0}'", this.langType);
            writer.Write(" , filterMode : {0} ", this.filterMode ? "true" : "false"); // 关闭HTML标签过滤
            writer.Write(" , uploadJson : \"{0}\"", UploadHandler);
            if (this.ReadOnly) writer.Write(" , readonlyMode : true ");
            if (!this.allowImageUpload) writer.Write(" , allowImageUpload : false");
            writer.Write(" , allowFlashUpload : {0}", this.allowFlashUpload ? "true" : "false");
            writer.Write(" , allowMediaUpload : false");

            if (this.resizeType != 2)
                writer.Write(" , resizeType : {0}", this.resizeType);
            if (this.IsSimple)
                writer.Write(" , items : ['fontname', 'fontsize', '|', 'textcolor', 'bgcolor', 'bold', 'italic', 'underline','removeformat', '|', 'justifyleft', 'justifycenter', 'justifyright', 'insertorderedlist','insertunorderedlist', '|', 'emoticons', 'image', 'link']");

            writer.Write(" });  }); ");
            writer.Write("</script>");

        }


        #region =============   IPostBackDataHandler   ===============

        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            if (postCollection[postDataKey] != Text)
            {
                Text = postCollection[postDataKey];
                return true;
            }
            return false;

        }

        protected override void RaisePostDataChangedEvent()
        {
            OnValueChanged(EventArgs.Empty);

        }

        private static readonly object ValueChangedEvent = new object();

        /// <summary>
        /// 声明一个代理用来处理值被改变的事件 ，当组件的值发生改变时发生 ValueChanged事件
        /// </summary>
        public event EventHandler ValueChanged
        {
            add
            {
                Events.AddHandler(ValueChangedEvent, value);
            }
            remove
            {
                Events.RemoveHandler(ValueChangedEvent, value);
            }
        }


        /// <summary>
        /// 触发值被改变事件的方法
        /// </summary>
        protected virtual void OnValueChanged(EventArgs e)
        {
            if (Events != null)
            {
                EventHandler oEventHandle = (EventHandler)Events[ValueChangedEvent];
                if (oEventHandle != null) oEventHandle(this, e);
            }
        }

        #endregion
    }

    /// <summary>
    ///  處理上傳圖片的請求
    ///  在web.config裡面加入節： <add type="SP.Studio.Controls.uploadJson,SP.Studio" verb="POST" path="/kindeditor/uploadJson.axd"/>
    ///  這個方法是上傳通用方法，建議在邏輯層繼承本類，加入邏輯判斷
    /// </summary>
    public class uploadJson : IHttpHandler
    {
        private HttpContext context;

        public bool IsReusable
        {
            get { return false; }
        }

        public virtual void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            this.context = context;

            HttpPostedFile imgFile = context.Request.Files["imgFile"];
            UploadInfo upload = UploadAgent.UploadFile(imgFile, maxSize, UploadFileType.Image, Path);
            switch (WebAgent.QS("type"))
            {
                case "editor":
                    if (upload.FaildType == UploadFaildType.None)
                    {
                        context.Response.Write(SavePath + upload.SavePath);
                    }
                    else
                    {
                        WebAgent.FaidAndBack(upload.ErrorMsg);
                    }
                    break;
                default:
                    if (upload.FaildType == UploadFaildType.None)
                    {
                        uploadSuccess(SavePath + "/" + upload.SavePath);
                    }
                    else
                    {
                        showError(upload.ErrorMsg);
                    }
                    break;
            }
        }

        /// <summary>
        /// 保存的相對路徑
        /// </summary>
        public virtual string SavePath
        {
            get
            {
                return "/Upload";
            }
        }

        public virtual string Path
        {
            get
            {
                return context.Server.MapPath(SavePath);
            }
        }

        /// <summary>
        /// 允許上傳的最大尺寸
        /// </summary>
        public virtual int maxSize
        {
            get
            {
                return 200 * 1024;
            }
        }

        public void showError(string message)
        {
            Hashtable hash = new Hashtable();
            context.Response.AddHeader("Content-Type", "text/html; charset=UTF-8");
            context.Response.Write("{\"error\":1,\"message\":\"" + message + "\"}");
            context.Response.End();
        }

        public void uploadSuccess(string url)
        {
            Hashtable hash = new Hashtable();
            context.Response.AddHeader("Content-Type", "text/html; charset=UTF-8");
            context.Response.Write("{\"error\":0,\"url\":\"" + url + "\"}");
            context.Response.End();
        }
    }
}
