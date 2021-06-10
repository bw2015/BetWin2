using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;

namespace SP.Studio.Xml
{
    public static class XmlAgent
    {
        /// <summary>
        /// 把xml文件读到DataSet中
        /// </summary>
        public static DataSet ReadXml(string fileName)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(fileName);
            return ds;
        }

        /// <summary>
        /// 获取符合条件的子节点
        /// </summary>
        public static XmlNodeList GetNodeList(string xmlFile, string xpath)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(xmlFile);
            return xDoc.SelectNodes(xpath);
        }
    }
}
