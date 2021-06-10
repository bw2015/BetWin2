using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace SP.Studio.Xml
{
    internal static class XmlAgent
    {
        internal static string GetAttributeValue(this XElement item, string attributeName)
        {
            return item.Attribute(attributeName) == null ? string.Empty : item.Attribute(attributeName).Value;
        }
    }
}
