using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace opcode4.utilities
{
    public static class XmlUtils
    {
        public static string ReadNodeText(XmlDocument xml, string nodePath)
        {
            var node = xml.SelectSingleNode(nodePath);
            return node == null ? "" : node.InnerText;
        }

        public static string ReadNodeText(XmlNode node)
        {
            return node == null ? "" : node.InnerText;
        }

        public static long? ReadNodeLong(XmlDocument xml, string nodePath)
        {
            var node = xml.SelectSingleNode(nodePath);
            if (node == null || string.IsNullOrEmpty(node.InnerText)) 
                return null;
            
            return Int64.Parse(node.InnerText);
        }

        public static int? ReadNodeInt(XmlDocument xml, string nodePath)
        {
            var node = xml.SelectSingleNode(nodePath);
            if (node == null || string.IsNullOrEmpty(node.InnerText))
                return null;

            return Int32.Parse(node.InnerText);
        }

        public static XmlDocument CreateXmlDocument(string xmlData)
        {
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml(xmlData);
                return xml;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Bad XML: {0} \n {1}", ex.Message, xmlData));
            }
        }

        public static string GetNode(string source, string mask)
        {
            var result = "";
            var pattern = string.Format("<{0}>(.*?)</{0}>", mask);
            var r = new Regex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var m = r.Match(source);

            if (m.Success)
            {
                result = m.Groups[1].Value;
            }
            return result;
        }



        public static string Serialize<T>(T obj)
        {
            if (obj == null)
                return string.Empty;
            
            var sr = new XmlSerializer(obj.GetType());
            var sb = new StringBuilder();
            var w = new StringWriter(sb, System.Globalization.CultureInfo.InvariantCulture);
            sr.Serialize(w, obj);


            return sb.ToString();
        }

        public static T Deserialize<T>(string xml)
        {
            if (xml == null)
                return default(T);

            if (string.IsNullOrEmpty(xml))
                return (T)Activator.CreateInstance(typeof (T));

            var reader = new StringReader(xml);
            var sr = new XmlSerializer(typeof(T));

            return (T)sr.Deserialize(reader);
        }

        public static string DataContractSerialize<T>(T obj)
        {
            if (obj == null)
                return string.Empty;

            var sb = new StringBuilder();

            using (var writer = XmlWriter.Create(sb))
            {
                var ser = new DataContractSerializer(obj.GetType());
                ser.WriteObject(writer, obj);
            }
            return sb.ToString();

        }

        public static T DataContractDeserialize<T>(string xml)
        {
            if (xml == null)
                return default(T);

            if (string.IsNullOrEmpty(xml))
                return (T)Activator.CreateInstance(typeof(T));

            using (var reader = XmlReader.Create(new StringReader(xml)))
            {
                var ser = new DataContractSerializer(typeof(T));
                return (T)ser.ReadObject(reader);
            }
        }
    }
}