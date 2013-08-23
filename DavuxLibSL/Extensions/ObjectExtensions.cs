using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DavuxLibSL.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToXml(this object o)
        {
            if (o == null) return "";
            MemoryStream memoryStream = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(o.GetType());
            //XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xs.Serialize(memoryStream, o);
            //memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            byte[] contents = memoryStream.ToArray();
            return new UTF8Encoding().GetString(contents, 0, contents.Length);
        }

        public static T FromXML<T>(this string str)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream(new UTF8Encoding().GetBytes(str));
            //XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            return (T)xs.Deserialize(memoryStream);
        }
    }
}
