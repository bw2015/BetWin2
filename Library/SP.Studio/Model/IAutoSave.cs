using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Model
{
    /// <summary>
    /// 用于实体类自动保存的接口
    /// </summary>
    interface IAutoSave
    {
        /// <summary>
        /// 自动保存的方法
        /// </summary>
        void AutoSave();
    }
}
