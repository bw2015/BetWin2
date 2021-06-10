using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Core
{
    /// <summary>
    /// 树结构
    /// </summary>
    public static class TreeExtensions
    {
        [ThreadStatic]
        static int count;

        /// <summary>
        /// 找出对象的子对象合集
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list">列表对象</param>
        /// <param name="value">值委托</param>
        /// <param name="parent">父委托</param>
        /// <param name="self">起始查找点</param>
        public static List<T> ChildNode<T, TOutput>(this List<T> list, Func<T, TOutput> value, Func<T, TOutput> parent, TOutput self, List<T> childList = null) where TOutput : struct
        {
            if (count > 1024)
            {
                count = default(int);
                throw new Exception("子元素超过1024个！");
            }
            T p = default(T);
            if (childList == null)
            {
                count = 0;
                childList = new List<T>();
                p = list.Find(t => value.Invoke(t).Equals(self));
                if (p != null)
                    childList.Add(p);
                else
                    return null;
            }
            count++;

            foreach (T item in list)
            {
                TOutput parentObj = parent.Invoke(item);
                if (parentObj.Equals(self))
                {
                    childList.Add(item);
                    list.ChildNode(value, parent, value.Invoke(item), childList);
                }
            }
            return childList;
        }
    }
}
