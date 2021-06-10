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
    public class WorkFlowEvent : System.Web.UI.Control
    {
        public string Name { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {

            writer.Write(string.Format("<input type=\"hidden\" name=\"WorkFlowEvent\" value=\"{0}\" />", Name));

            base.Render(writer);
        }

        

    }
}
