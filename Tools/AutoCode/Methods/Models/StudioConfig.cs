
using System;
using System.Xml.Serialization;

namespace AutoCode.Methods.Models
{
    [XmlRoot(ElementName = "Studio")]
    public class StudioConfig
    {
        [XmlElement]
        public string Copyright
        {
            get
            {
                return "SP.Studio 代码自动生成工具";
            }
            set
            {
            }
        }

        [XmlElement]
        public string DbConnection
        {
            get;
            set;
        }

        [XmlElement]
        public string ModelTemplate
        {
            get;
            set;
        }
    }
}
