using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ServiceBusAction
{
    internal static class DocumentationHelpers
    {

        /// <summary>
        /// Concate all documentations
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        internal static XPathDocument ConcateDocumentations(string pattern)
        {

            XElement xml = null;
            var path = new DirectoryInfo(Path.GetDirectoryName(typeof(DocumentationHelpers).Assembly.Location));

            foreach (var fileName in path.GetFiles(pattern))
                if (xml == null)
                    xml = XElement.Load(fileName.FullName);

                else
                    foreach (var ele in XElement.Load(fileName.FullName).Descendants())
                        xml.Add(ele);

            if (xml != null)
                return new XPathDocument(xml.CreateReader());

            return null;

        }

    }

}
