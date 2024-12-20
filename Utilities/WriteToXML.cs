using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GiacScraper.Utilities
{
    internal class WriteToXML
    {
        public static void WriteUrlsToXml(IEnumerable<string> urls, string filePath)
        {
            using (var writer = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Urls");

                foreach (var url in urls)
                {
                    writer.WriteStartElement("Url");
                    writer.WriteString(url);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
