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
            {"name", "### {0}" },
            {"summary", "{0}" },
            { "param", "- {0}: {1}"},
            { "typeparam", "- Type parameter {0}: {1} " },
            {"returns", "Returns: {0}" },
            { "remarks", "Remarks: {0}" },
            { "code", "```csharp\n{0}\n```" },
            { "c", "`{0}`" },
            { "example", "Example:\n{0}" }
        };
        public readonly Dictionary<char, string> MemberTypes = new Dictionary<char, string>()
        {
            {'T', "class" },
            {'M', "method" }
        };
        public XmlConverter(XmlDocument doc) => Doc = doc;
        public string Convert()
        {
            string result = $"## Features List\n";
            var members = Doc.GetElementsByTagName("member");
            result += FeaturesList(members) + "\n## Doc\n\n";
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
                    listPrefix = $"\n{c}. "; c++;
                }
                else listPrefix = "- ";
                result += $"{listPrefix}[{longName.Substring(2)}](#{shortName.ToLower().Replace(' ', '-')})\n";
            }
            return result;
        }
        public string RenderMember(XmlNode member)
        {
            var type = MemberTypes[member.Attributes.GetNamedItem("name").InnerText[0]];
            var result = type != "class" ? "#" : "";
            foreach(XmlNode node in member.ChildNodes)
            {
                var attr = node.Attributes != null && node.Attributes.Count > 0 ? node.Attributes[0].Value : null;
                string line;
                if(Templates.ContainsKey(node.Name))
                {
                    if (attr != null)
                        line = string.Format(Templates[node.Name], attr, node.InnerText);
                    else
                        line = string.Format(Templates[node.Name], node.InnerText);
                    result += line.TrimStart().TrimEnd() + "\n\n";
                }
                else
                {
                    line = node.InnerText.TrimStart() + '\n';
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
