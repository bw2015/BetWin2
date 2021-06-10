using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.WorkFlow
{
    /// <summary>
    /// 用来标记一个方法是工作流方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class WorkFlowAttribute : Attribute
    {

    }
}
