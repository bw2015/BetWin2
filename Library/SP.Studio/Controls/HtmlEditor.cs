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

namespace SP.Studio.Controls
{
    [Obsolete("使用 KindEditor")]
    public class HtmlEditor : System.Web.UI.Control, IPostBackDataHandler
    {
        private const string KEY = "KindEditor";

        public Unit Width { get; set; }

        public Unit Height { get; set; }

        public string Text { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            bool hasScript = true;
            if (!Context.Items.Contains(KEY))
            {
                Context.Items.Add(KEY, true);
                hasScript = false;
            }
            writer.Write(string.Format("<textarea id=\"{0}\" name=\"{1}\" style=\"width:{2};height:{3};\">{4}</textarea>", this.ClientID, this.UniqueID, this.Width, this.Height, this.Text));
            if (!hasScript) writer.Write(string.Format("<script charset=\"utf-8\" src=\"/HtmlEditor/kindeditor.js\"></script>"));
            writer.Write("<script>        KE.show({                id : '" + this.ClientID + "'        });</script>");

            base.Render(writer);
        }

        #region =============   IPostBackDataHandler   ===============
        
        public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            if (postCollection[postDataKey] != Text)
            {
                Text = postCollection[postDataKey];
                return true;
            }
            return false;

        }

        public void RaisePostDataChangedEvent()
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
}
