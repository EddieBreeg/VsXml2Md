using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace VsXml2Md
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var doc = new XmlDocument();
            doc.Load(args[0]);
            var converter = new XmlConverter(doc);
            string md = converter.Convert();
            File.WriteAllText(
                Path.Join(Path.GetDirectoryName(args[0]), 
                $"{Path.GetFileNameWithoutExtension(args[0])}.md"),
                md);
        }
    }
    public class XmlConverter
    {
        public XmlDocument Doc { get; }
        public readonly Dictionary<string, string> Templates = new Dictionary<string, string>()
        {
            {"name", "\r\n### *{0}*\r\n" },
            {"summary", "{0}\r\n\r\n" },
            { "param", "- {0}: {1}\r\n\r\n"},
            { "typeparam", "- Type parameter {0}: {1}\r\n" },
            { "returns", "\r\nReturns: {0}\r\n" },
            { "remarks", "\r\nRemarks: {0}\r\n" },
            { "code", "```csharp\r\n{0}\r\n```\r\n" },
            { "c", "`{0}`" },
            { "example", "Example:\r\n{0}\r\n" }
        };
        public readonly Dictionary<char, string> MemberTypes = new Dictionary<char, string>()
        {
            {'T', "class" },
            {'M', "method" }
        };
        public XmlConverter(XmlDocument doc) => Doc = doc;
        public string Convert()
        {
            string result = $"## Features List\r\n";
            var members = Doc.GetElementsByTagName("member");
            result += FeaturesList(members) + "\r\n## Doc\r\n\r\n";
            foreach (XmlNode member in members)
                result += RenderMember(member);
            
            return result;
        }
        public string FeaturesList(XmlNodeList members)
        {
            var result = "";
            int c = 1;
            foreach (XmlNode member in members)
            {
                string longName = member.Attributes.GetNamedItem("name").InnerText;
                var nameNode = member.ChildNodes.GetXmlNode("name");
                var shortName = nameNode != null ? nameNode.InnerText : longName.Substring(2);
                string type = MemberTypes[longName[0]];
                string listPrefix;
                if (type == "class")
                {
                    listPrefix = $"\r\n{c}. "; c++;
                }
                else listPrefix = "- ";
                result += $"{listPrefix}[{longName.Substring(2)}](#{shortName.ToLower().Replace(' ', '-')})\r\n";
            }
            return result;
        }
        public string RenderMember(XmlNode member)
        {
            var type = MemberTypes[member.Attributes.GetNamedItem("name").InnerText[0]];
            var result = "";
            foreach(XmlNode node in member.ChildNodes)
            {
                var attr = node.Attributes != null && node.Attributes.Count > 0 ? node.Attributes[0].Value : null;
                string line;
                if(Templates.ContainsKey(node.Name))
                {
                    if (attr != null)
                        line = string.Format(Templates[node.Name], attr, node.InnerText.TrimStart().TrimEnd());
                    else
                        line = string.Format(Templates[node.Name], node.InnerText.TrimStart().TrimEnd());
                    result += line;
                }
                else
                {
                    line = node.InnerText.TrimStart() + "\r\n";
                }
            }
            return result;
        }
    }
    static class XmlNodeListExtensions
    {
        public static XmlNode? GetXmlNode(this XmlNodeList list, string name)
        {
            foreach(XmlNode node in list)
            {
                if (node.Name == name) return node;
            }
            return null;
        }
    }
}
