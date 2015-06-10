using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Fusion.VideoStreaming
{
    internal static class Inherited
    {
        static internal PropertyInfo GetConfigProperty(Type type)
        {
            var configProps = type.GetProperties()
                .Where(pi => pi.CustomAttributes.Any(ca => ca.AttributeType == typeof(ConfigAttribute)))
                .ToList();

            if (configProps.Count != 1)
            {
                return null;
            }

            return configProps[0];
        }

        static internal object LoadFromXml(Type type, string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            TextReader textReader = new StreamReader(fileName);
            object obj = serializer.Deserialize(textReader);
            textReader.Close();
            return obj;
        }

        static internal void SaveToXml(object obj, Type type, string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            TextWriter textWriter = new StreamWriter(fileName);
            serializer.Serialize(textWriter, obj);
            textWriter.Close();
        }
    }
}
