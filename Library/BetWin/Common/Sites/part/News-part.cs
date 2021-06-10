using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BW.Framework;

namespace BW.Common.Sites
{
    partial class News
    {
        /// <summary>
        /// 是否需要弹出提示
        /// </summary>
        public bool IsTip
        {
            get
            {
                if (this.Tip == 0) return false;
                return (DateTime.Now - this.CreateAt).TotalDays < this.Tip;
            }
        }

        public string CoverShow
        {
            get
            {
                return SysSetting.GetSetting().GetImage(this.Cover);
            }
        }
    }
}
