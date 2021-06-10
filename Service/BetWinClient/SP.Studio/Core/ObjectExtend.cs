using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Core
{
    internal static class ObjectExtend
    {
        internal static bool IsBaseType(this Type type, Type type2)
        {
            if (type2.IsInterface)
            {
                return type.GetInterface(type2.Name) != null;
            }
            while (type != null)
            {
                if (type == type2) return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
